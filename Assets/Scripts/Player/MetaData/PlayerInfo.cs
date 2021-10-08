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

    public PlayerDisplay PlayerDisplay;
    // Start is called before the first frame update
    public override void NetworkStart()
    {
        if (IsOwner)
        {
            String nickname = MatchManager.Instance.Nickname;
            PlayerDisplay.DisplayNickname(nickname);
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
        IEnumerator coroutine = WaitToGetPlayerInfo(ownerId, clientId);
        StartCoroutine(coroutine);

        //GetPlayerInfo_ClientRpc(PlayerInfoController.Instance.PlayerInfos[ownerId], clientRpcParams);       
    }
    
    [ClientRpc]
    public void GetPlayerInfo_ClientRpc(PlayerData playerData, ClientRpcParams clientRpcParams = default)
    {
        PlayerData = playerData;
        DisplayMyInfo();
    }

    IEnumerator WaitToGetPlayerInfo(ulong ownerId, ulong clientId)
    {
        yield return new WaitUntil(() => PlayerInfoController.Instance.PlayerInfos.ContainsKey(ownerId));
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };
        GetPlayerInfo_ClientRpc(PlayerInfoController.Instance.PlayerInfos[ownerId], clientRpcParams);
    }


    private void DisplayMyInfo()
    {
        PlayerDisplay.DisplayNickname(PlayerData.Nickname);

        PlayerDisplay.SetColor(PlayerData.Color);
        
    }


}
