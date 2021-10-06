using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : NetworkBehaviour
{
    
    public PlayerData PlayerData;

    public Text NicknameText;
    // Start is called before the first frame update
    public override void NetworkStart()
    {
        if (IsOwner)
        {
            String nickname = MatchManager.Instance.Nickname;
            NicknameText.text = nickname;
            SendInfo_ServerRpc(nickname);
        }
        else if(!IsServer)
        {
            GetPlayerInfo_ServerRpc(OwnerClientId, NetworkManager.LocalClientId);
        }
    }

    [ServerRpc]
    public void SendInfo_ServerRpc(String nickname)
    {
        PlayerData = PlayerInfoController.Instance.Add(OwnerClientId, nickname);
        DisplayMyInfo();
        
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{OwnerClientId}
            }
        };
        
        SetInfoOnOwner_ClientRpc(PlayerData, clientRpcParams);
    }
    
    [ClientRpc]
    public void SetInfoOnOwner_ClientRpc(PlayerData playerData, ClientRpcParams clientRpcParams = default)
    {
        PlayerData = playerData;
        DisplayMyInfo();
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetPlayerInfo_ServerRpc(ulong ownerId, ulong clientId )
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        GetPlayerInfo_ClientRpc(PlayerInfoController.Instance.PlayerInfos[ownerId], clientRpcParams);       
    }
    
    [ClientRpc]
    public void GetPlayerInfo_ClientRpc(PlayerData playerData, ClientRpcParams clientRpcParams = default)
    {
        PlayerData = playerData;
        DisplayMyInfo();
    }



    private void DisplayMyInfo()
    {
        NicknameText.text = PlayerData.Nickname;
        //Do Color Stuff
        
    }


}
