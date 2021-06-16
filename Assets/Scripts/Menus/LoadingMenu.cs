using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour
{
    // Events
    public delegate void OnCancelDelegate();
    public static event OnCancelDelegate OnCancel;

    private const string hostMessage = "Creating Lobby...";
    private const string joinMessage = "Connecting to";
    [SerializeField] private Text _loadMessage = null;

    private void Awake()
    {
        ConnectionMenu.OnGoToLobby += initializeLoadingMenu;
    }

    private void OnDestroy()
    {
        ConnectionMenu.OnGoToLobby -= initializeLoadingMenu;
    }

    private void initializeLoadingMenu(bool isHost, NetworkTransportTypes type, string address) {
        this.toggleMenu();
        _loadMessage.text = isHost? hostMessage : $"{joinMessage} {address}...";
    }

    // Button Actions
    public void cancelConnection() => OnCancel?.Invoke();
}
