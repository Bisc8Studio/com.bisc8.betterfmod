#if B8FMOD_UNITY_NETCODE
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public sealed class FmodUnityNetcodeTransport : MonoBehaviour, IFmodMultiplayerTransport
{
    private const string MessageName = "B8FmodButtonAction";
    private bool registered;

    public bool CanSendFmodCommands
    {
        get
        {
            NetworkManager manager = NetworkManager.Singleton;
            return manager != null
                && manager.IsListening
                && manager.CustomMessagingManager != null;
        }
    }

    private void OnEnable()
    {
        FmodCommands.MultiplayerTransport = this;
        TryRegisterHandler();
    }

    private void Update()
    {
        TryRegisterHandler();
    }

    private void OnDisable()
    {
        UnregisterHandler();

        if (FmodCommands.MultiplayerTransport == this)
            FmodCommands.MultiplayerTransport = null;
    }

    public void SendFmodButtonAction(FmodButtonActionPayload payload)
    {
        if (payload == null || !CanSendFmodCommands)
            return;

        TryRegisterHandler();

        string json = JsonUtility.ToJson(payload);
        NetworkManager manager = NetworkManager.Singleton;

        if (manager.IsServer)
        {
            RelayToClients(json);
            return;
        }

        SendJson(NetworkManager.ServerClientId, json);
    }

    private void TryRegisterHandler()
    {
        if (registered || !CanSendFmodCommands)
            return;

        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(MessageName, OnNamedMessage);
        registered = true;
    }

    private void UnregisterHandler()
    {
        if (!registered)
            return;

        NetworkManager manager = NetworkManager.Singleton;
        if (manager != null && manager.CustomMessagingManager != null)
            manager.CustomMessagingManager.UnregisterNamedMessageHandler(MessageName);

        registered = false;
    }

    private void OnNamedMessage(ulong senderClientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string json);

        NetworkManager manager = NetworkManager.Singleton;
        if (manager != null && manager.IsServer)
            RelayToClients(json);
        else
            PlayJson(json);
    }

    private void RelayToClients(string json)
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null)
            return;

        foreach (ulong clientId in manager.ConnectedClientsIds)
        {
            if (manager.IsHost && clientId == manager.LocalClientId)
            {
                PlayJson(json);
                continue;
            }

            SendJson(clientId, json);
        }
    }

    private void SendJson(ulong clientId, string json)
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null || manager.CustomMessagingManager == null)
            return;

        using (FastBufferWriter writer = new FastBufferWriter((json.Length * 4) + 128, Allocator.Temp))
        {
            writer.WriteValueSafe(json);
            manager.CustomMessagingManager.SendNamedMessage(MessageName, clientId, writer, NetworkDelivery.ReliableSequenced);
        }
    }

    private static void PlayJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        FmodButtonActionPayload payload = JsonUtility.FromJson<FmodButtonActionPayload>(json);
        FmodCommands.ReceiveMultiplayerButtonAction(payload);
    }
}
#endif
