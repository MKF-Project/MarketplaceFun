using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraManager : MonoBehaviour
{
    [SerializeField] private Camera _menuCamera;
    [SerializeField] private Camera _lobbyCamera;

    private void Awake()
    {
        LobbyMenu.OnEnterLobby += EnableLobbyCamera;
        LobbyMenu.OnCancelMatch += EnableMenuCamera;
    }

    private void OnDestroy()
    {
        LobbyMenu.OnEnterLobby -= EnableLobbyCamera;
        LobbyMenu.OnCancelMatch -= EnableMenuCamera;
    }

    private void Start()
    {
        EnableMenuCamera();
    }

    private void EnableMenuCamera()
    {
        _menuCamera.enabled = true;
        _lobbyCamera.enabled = false;
    }

    private void EnableLobbyCamera()
    {
        _menuCamera.enabled = false;
        _lobbyCamera.enabled = true;
    }
}
