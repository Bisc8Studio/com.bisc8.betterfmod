#if B8FMOD_UNITY_NETCODE
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public sealed class FmodUnityNetcodeTransport : MonoBehaviour, IFmodMultiplayerTransport
{
    private const string ButtonActionMessageName = "BISC8.BetterFMOD.ButtonAction";
    private const NetworkDelivery ButtonActionDelivery = NetworkDelivery.ReliableFragmentedSequenced;

    private NetworkManager registeredManager;
    private bool registered;

    public bool CanSendFmodCommands
    {
        get
        {
            TryRegister();

            NetworkManager manager = NetworkManager.Singleton;
            return manager != null
                && manager.IsListening
                && manager.CustomMessagingManager != null;
        }
    }

    private void OnEnable()
    {
        FmodCommands.MultiplayerTransport = this;
        TryRegister();
    }

    private void Update()
    {
        TryRegister();
    }

    private void OnDisable()
    {
        Unregister();

        if (FmodCommands.MultiplayerTransport == this)
            FmodCommands.MultiplayerTransport = null;
    }

    public void SendFmodButtonAction(FmodButtonActionPayload payload)
    {
        if (payload == null || !CanSendFmodCommands)
            return;

        string json = JsonUtility.ToJson(payload);
        NetworkManager manager = NetworkManager.Singleton;

        if (manager == null || manager.CustomMessagingManager == null)
            return;

        if (manager.IsServer)
        {
            BroadcastJson(manager, json);
            return;
        }

        using FastBufferWriter writer = CreateWriter(json);
        manager.CustomMessagingManager.SendNamedMessage(ButtonActionMessageName, NetworkManager.ServerClientId, writer, ButtonActionDelivery);
    }

    private void TryRegister()
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null || !manager.IsListening || manager.CustomMessagingManager == null)
            return;

        if (registered && registeredManager == manager)
            return;

        Unregister();

        manager.CustomMessagingManager.RegisterNamedMessageHandler(ButtonActionMessageName, OnButtonActionMessage);
        registeredManager = manager;
        registered = true;
    }

    private void Unregister()
    {
        if (!registered || registeredManager == null || registeredManager.CustomMessagingManager == null)
        {
            registered = false;
            registeredManager = null;
            return;
        }

        registeredManager.CustomMessagingManager.UnregisterNamedMessageHandler(ButtonActionMessageName);
        registered = false;
        registeredManager = null;
    }

    private void OnButtonActionMessage(ulong senderClientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out string json);

        if (string.IsNullOrWhiteSpace(json))
            return;

        NetworkManager manager = NetworkManager.Singleton;
        if (manager != null && manager.IsServer)
        {
            BroadcastJson(manager, json);
            return;
        }

        PlayJson(json);
    }

    private static void BroadcastJson(NetworkManager manager, string json)
    {
        if (manager == null || manager.CustomMessagingManager == null || string.IsNullOrWhiteSpace(json))
            return;

        if (manager.IsClient)
            PlayJson(json);

        foreach (ulong clientId in manager.ConnectedClientsIds)
        {
            if (clientId == NetworkManager.ServerClientId)
                continue;

            using FastBufferWriter writer = CreateWriter(json);
            manager.CustomMessagingManager.SendNamedMessage(ButtonActionMessageName, clientId, writer, ButtonActionDelivery);
        }
    }

    private static FastBufferWriter CreateWriter(string json)
    {
        FastBufferWriter writer = new FastBufferWriter(json.Length * 4 + 128, Allocator.Temp);
        writer.WriteValueSafe(json);
        return writer;
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
