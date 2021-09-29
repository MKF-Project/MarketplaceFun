using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPosition : NetworkBehaviour
{
    private bool _isOnLobby;
    private Vector3 _lobbyPosition;
    private int _index;
    private String _nickname;

    private Dictionary<ulong, LobbyInfo> _lobbyInfos;
    
    void Start()
    {
        _isOnLobby = true;
        SceneManager.OnMatchLoaded += ExitLobby;

        if (!IsOwner)
        {
            RequestOthersInfo_ServerRpc();
        }

        /*if (IsServer)
        {
            _lobbyInfos = new Dictionary<ulong, LobbyInfo>();
            NetworkManager.OnClientDisconnectCallback += RemoveLobbyInfo;
        }
        */
    }

    private void OnDestroy()
    {
        SceneManager.OnMatchLoaded -= ExitLobby;
        //NetworkManager.OnClientDisconnectCallback += RemoveLobbyInfo;

    }

    private void Update()
    {
        if (_isOnLobby && IsOwner)
        {
            transform.position = _lobbyPosition;
        }
    }
    
    
    public void PositionPlayer(Vector3 position, int index)
    {
        _lobbyPosition = position;
        _index = index;
        _nickname = MatchManager.Instance.Nickname;
        PositionNickname_ServerRpc(OwnerClientId, index, position, MatchManager.Instance.Nickname);
    }
    
    [ServerRpc]
    public void PositionNickname_ServerRpc(ulong clientId, int index, Vector3 position, String nickname)
    {
        //LobbyInfo lobbyInfo = new LobbyInfo(index, position, nickname);
        //_lobbyInfos.Add(clientId, lobbyInfo);
        PositionNickname_ClientRpc(index, position, nickname);
    }
    
    [ClientRpc]
    public void PositionNickname_ClientRpc(int index, Vector3 position, String nickname)
    {
        _lobbyPosition = position;
        _index = index;
        _nickname = nickname;
        PositionNickname();
    }
    
    public void PositionNickname()
    {
        LobbyMenu lobbyMenu = MenuManager.instance.LobbyMenu;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(new Vector3(_lobbyPosition.x,2,_lobbyPosition.z));

        GameObject nickname = null;
        switch (_index)
        {
            case 1:
                nickname = lobbyMenu.Nickname1;
                break;
            case 2:
                nickname = lobbyMenu.Nickname2;
                break;
            case 3:
                nickname = lobbyMenu.Nickname3;
                break;
            case 4:
                nickname = lobbyMenu.Nickname4;
                break;
        }

        if (nickname != null)
        {
            nickname.transform.position = screenPosition;
            nickname.SetActive(true);
            nickname.GetComponent<Text>().text = _nickname;
        }
        
    }

    [ServerRpc]
    public void RequestOthersInfo_ServerRpc()
    {
        /*
        foreach (ulong clientId in _lobbyInfos.Keys)
        {
            LobbyInfo lobbyInfo = _lobbyInfos[clientId];
            FillOthersInfos_ClientRpc(clientId, lobbyInfo.Index, lobbyInfo.Position, lobbyInfo.Nickname);
        }
        */
        PositionNickname_ClientRpc(_index, _lobbyPosition, _nickname);
    }

    [ClientRpc]
    public void FillOthersInfos_ClientRpc(ulong clientId, int index, Vector3 position, String nickname)
    {
        LobbyPosition lobbyPositionOther = NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<LobbyPosition>();

        lobbyPositionOther.FillClientInformation(index, position, nickname);

    }

    public void FillClientInformation(int index, Vector3 position, String nickname)
    {
        _index = index;
        _nickname = nickname;
        PositionNickname();
    }

    public void ExitLobby(String scene)
    {
        _isOnLobby = false;
        Destroy(this);
    }

    public void RemoveLobbyInfo(ulong clientId)
    {
        _lobbyInfos.Remove(clientId);
    }
}
