using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public class ClientConnectionChecker : NetworkBehaviour
{
    // Only one of these may exist per scene
    private static ClientConnectionChecker _instance = null;

    public delegate void AllClientsResponseDelegate();
    public static event AllClientsResponseDelegate OnAllClientResponses;

    public delegate void ClientResponseDelegate(ulong clientID);
    public static event ClientResponseDelegate OnClientResponse;

    private int _requiredClientResponses = 1;
    private HashSet<ulong> _clientResponses;

    private void Awake()
    {
        if(_instance != null)
        {
            Destroy(this);
            return;
        }

        if(IsServer)
        {
            _requiredClientResponses = NetworkController.NumberOfClients;
            _clientResponses = new HashSet<ulong>();
        }
    }

    private void OnDestroy()
    {
        if(_instance == this)
        {
            // Clear events that this
            OnAllClientResponses = null;
            OnClientResponse = null;

            _instance = null;
        }
    }

    public override void NetworkStart()
    {
        if(IsClient)
        {
            SendResponseToServer_ServerRpc();
        }
    }

    // RPCs
    [ServerRpc(RequireOwnership = false)]
    private void SendResponseToServer_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    {
        var clientID = rpcReceiveParams.Receive.SenderClientId;
        if(_clientResponses.Add(clientID))
        {
            OnClientResponse?.Invoke(clientID);
            if(_clientResponses.Count == _requiredClientResponses)
            {
                OnAllClientResponses?.Invoke();
            }
        }
    }
}
