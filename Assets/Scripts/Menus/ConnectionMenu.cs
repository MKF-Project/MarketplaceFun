using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionMenu : MonoBehaviour
{
    // Events
    public delegate void OnGoToLobbyDelegate(bool isHost, NetworkTransportTypes transport, string address);
    public static event OnGoToLobbyDelegate OnGoToLobby;

    public delegate void OnBackDelegate();
    public static event OnBackDelegate OnBack;

    private bool _isHost;

    // Title
    private const string _joinText = "Join";
    private const string _hostText = "Host";
    [SerializeField] private Text _menuTitle = null;

    // Address Input
    [SerializeField] private InputField _addressInput = null;

    // Address Placeholder
    private const string _placeholderIPPrompt = " Enter IP/Address...";
    private const string _placeholderPhotonPrompt = " Enter Room Name...";
    private const string _placeholderIgnore = " [Ignored]";
    [SerializeField] private Text _addressPlaceholder = null;

    // Transport Dropdown
    [SerializeField] private Dropdown _transportDropdown = null;

    // Nickname
    public static string Nickname { get; private set; }
    [SerializeField] private Text _nicknameText = null;

    private void Awake()
    {
        GameMenu.OnJoinGame += initializeJoinMenu;
        GameMenu.OnHostGame += initializeHostMenu;

        LoadingMenu.OnCancel += this.toggleMenu;
    }

    private void OnDestroy()
    {
        GameMenu.OnJoinGame -= initializeJoinMenu;
        GameMenu.OnHostGame -= initializeHostMenu;

        LoadingMenu.OnCancel -= this.toggleMenu;
    }

    private void initializeJoinMenu() => initializeConnectionMenu(false);
    private void initializeHostMenu() => initializeConnectionMenu(true);
    private void initializeConnectionMenu(bool isHost)
    {
        MenuManager.toggleMenu(gameObject);

        _isHost = isHost;
        _menuTitle.text = $"{(isHost? _hostText : _joinText)} Game:";

        onChangeTransport();
    }

    // Dropdown Action
    public void onChangeTransport()
    {
        var isIPHost = _transportDropdown.value == 0 && _isHost;

        _addressInput.interactable = !isIPHost;
        if(_addressInput.interactable)
        {
            _addressPlaceholder.text = _transportDropdown.value == 0? _placeholderIPPrompt : _placeholderPhotonPrompt;
        }
        else
        {
            _addressPlaceholder.text = _placeholderIgnore;
        }

        if(isIPHost)
        {
            _addressInput.text = "";
        }

        #if UNITY_EDITOR
            const string DEBUG_IP_HOST = "127.0.0.1";
            const string DEBUG_PHOTON_HOST = "sala";

            if(_transportDropdown.value == 0)
            {
                if(!_isHost)
                {
                    _addressInput.text = DEBUG_IP_HOST;
                }
            }
            else
            {
                _addressInput.text = DEBUG_PHOTON_HOST;
            }
        #endif
    }

    // Button Actions
    public void goToLobby() {
        if(OnGoToLobby == null) {
            return;
        }

        // Gather Info
        var transport = _transportDropdown.value == 0? NetworkTransportTypes.Direct : NetworkTransportTypes.Relayed;
        var address = _addressInput.text;

        if(address == "" && (!_isHost || transport != NetworkTransportTypes.Direct)) {
            return;
        }

        // Store chosen nickname
        Nickname = _nicknameText.text;

        // Call Event
        OnGoToLobby(_isHost, transport, address);
    }
    public void back() => OnBack?.Invoke();
}
