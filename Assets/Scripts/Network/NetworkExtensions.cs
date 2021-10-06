using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public static class NetworkExtensions
{
    public static ClientRpcParams ReturnRpcToSender(this ServerRpcParams serverRpc)
    {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { serverRpc.Receive.SenderClientId }
            }
        };
    }

    public static ClientRpcParams ReturnRpcToOthers(this ServerRpcParams serverRpc)
    {
        if(!NetworkController.IsServer)
        {
            return default(ClientRpcParams);
        }

        var clientIDs = NetworkController.getClientIDs().Where(client => client != serverRpc.Receive.SenderClientId).ToArray();

        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clientIDs
            }
        };
    }
}
