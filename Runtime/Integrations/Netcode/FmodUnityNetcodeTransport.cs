#if B8FMOD_UNITY_NETCODE
using Unity.Netcode;
using UnityEngine;

public sealed class FmodUnityNetcodeTransport : NetworkBehaviour, IFmodMultiplayerTransport
{
    public bool CanSendFmodCommands
    {
        get
        {
            NetworkManager manager = NetworkManager.Singleton;
            return manager != null
                && manager.IsListening
                && IsSpawned;
        }
    }

    private void OnEnable()
    {
        FmodCommands.MultiplayerTransport = this;
    }

    public override void OnNetworkSpawn()
    {
        FmodCommands.MultiplayerTransport = this;
    }

    private void OnDisable()
    {
        if (FmodCommands.MultiplayerTransport == this)
            FmodCommands.MultiplayerTransport = null;
    }

    public void SendFmodButtonAction(FmodButtonActionPayload payload)
    {
        if (payload == null || !CanSendFmodCommands)
            return;

        string json = JsonUtility.ToJson(payload);

        if (IsServer)
        {
            PlayButtonClientRpc(json);
            return;
        }

        PlayButtonServerRpc(json);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayButtonServerRpc(string json)
    {
        PlayButtonClientRpc(json);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayButtonClientRpc(string json)
    {
        PlayJson(json);
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
