using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class LobbyPositionController : NetworkBehaviour
{

    public Vector3 Position1;
    public Vector3 Position2;
    public Vector3 Position3;
    public Vector3 Position4;

    public Dictionary<ulong, int> playerPositions;

    //public Vector3 OwnerPosition;
    
    // Start is called before the first frame update
    public override void NetworkStart()
    {
        if (IsServer)
        {
            playerPositions = new Dictionary<ulong, int>();
            NetworkManager.OnClientConnectedCallback += OccupyPosition;
            NetworkManager.OnClientDisconnectCallback += FreePosition;
            
            LobbyPosition player = NetworkManager.ConnectedClients[OwnerClientId].PlayerObject.GetComponent<LobbyPosition>();
            
            player.PositionPlayer(Position1,1);
        }

        if (!IsOwnedByServer)
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        NetworkManager.OnClientConnectedCallback -= OccupyPosition;
        NetworkManager.OnClientDisconnectCallback -= FreePosition;
    }
    

  

    
    public void OccupyPosition(ulong playerId)
    {
        //Player player = NetworkManager.ConnectedClients[playerId].PlayerObject.GetComponent<Player>();
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{playerId}
            }
        };

        if (!playerPositions.ContainsValue(2))
        {
            SendPosition_ClientRpc(Position2, 2,clientRpcParams);
            playerPositions.Add(playerId, 2);
        }
        else if (!playerPositions.ContainsValue(3))
        {
            SendPosition_ClientRpc(Position3,3, clientRpcParams);
            playerPositions.Add(playerId, 3);
        }
        else if (!playerPositions.ContainsValue(4))
        {
            SendPosition_ClientRpc(Position4,4, clientRpcParams);
            playerPositions.Add(playerId, 4);
        }
    }

    [ClientRpc]
    public void SendPosition_ClientRpc(Vector3 position,int index, ClientRpcParams clientRpcParams = default)
    {
        
        LobbyPosition player = NetworkManager.ConnectedClients[NetworkManager.LocalClientId].PlayerObject.GetComponent<LobbyPosition>();
            
        player.PositionPlayer(position, index);
    }
    

    public void FreePosition(ulong playerId)
    {
        playerPositions.Remove(playerId);
    }

}
