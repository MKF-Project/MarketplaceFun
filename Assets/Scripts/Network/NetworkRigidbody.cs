using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Prototyping;


public class NetworkRigidbody : NetworkBehaviour
{

    private Rigidbody _rigidbody;
    private NetworkTransform _netTransform;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _netTransform = GetComponent<NetworkTransform>();

        #if UNITY_EDITOR
            if(_rigidbody == null)
            {
                Debug.LogError($"[{gameObject.name}::NetworkRigidbody]: Rigidbody not found!");
            }

            if(_netTransform == null)
            {
                Debug.LogError($"[{gameObject.name}::NetworkRigidbody]: NetworkTransform not found!");
            }
        #endif

        NetworkObject.DontDestroyWithOwner = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        // Only run this if you are the player that collided with the object
        if(MatchManager.Instance.IsMainPlayer(other.gameObject) && !IsOwner)
        {
            // Disable Network transform while we wait for Onwership confirmation
            // so that the rigidbody reacts immediately
            _netTransform.enabled = false;
            RequestObjectOwnership_ServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestObjectOwnership_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    {
        print($"Onwership request, {rpcReceiveParams.Receive.SenderClientId}");
        var clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {rpcReceiveParams.Receive.SenderClientId}
            }
        };

        NetworkObject.ChangeOwnership(rpcReceiveParams.Receive.SenderClientId);
        RespondObjectOwnership_ClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void RespondObjectOwnership_ClientRpc(ClientRpcParams clientRpcParams = default)
    {
        _netTransform.enabled = true;
    }
}
