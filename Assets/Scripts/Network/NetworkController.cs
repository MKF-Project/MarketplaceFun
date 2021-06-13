using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports;
using MLAPI.Transports.UNET;
using MLAPI.Transports.PhotonRealtime;

public enum NetworkTransportTypes {
  Direct,
  Relayed,
  None
}

public class NetworkController : MonoBehaviour
{
    // Events
    public delegate void OnConnectedDelegate(bool isHost);
    public static event OnConnectedDelegate OnConnected;

    public delegate void OnDisconnectedDelegate(bool wasHost);
    public static event OnDisconnectedDelegate OnDisconnected;

    private const ushort _port = 53658;

    private NetworkManager _netManager = null;
    private UNetTransport _ipTransport = null;
    private PhotonRealtimeTransport _relayedTransport = null;

    private NetworkTransport _transport
    {
        get
        {
            return _netManager?.NetworkConfig.NetworkTransport;
        }

        set
        {
            if(_netManager != null)
            {
                _netManager.NetworkConfig.NetworkTransport = value;
            }
        }
    }

    private NetworkTransportTypes _transportType
    {
        get
        {
            var curr = _transport;
            if(curr == null)
            {
                return NetworkTransportTypes.None;
            }

            return curr == _ipTransport? NetworkTransportTypes.Direct : NetworkTransportTypes.Relayed;
        }

        set
        {
            switch(value)
            {
                case NetworkTransportTypes.Direct:
                    _transport = _ipTransport;
                    break;

                case NetworkTransportTypes.Relayed:
                    _transport = _relayedTransport;
                    break;

                case NetworkTransportTypes.None:
                    _transport = null;
                    break;
            }
        }
    }

    private void Start()
    {
        _netManager = GetComponent<NetworkManager>();
        _ipTransport = GetComponent<UNetTransport>();
        _relayedTransport = GetComponent<PhotonRealtimeTransport>();

        // Event Subscribings
        ConnectionMenu.OnGoToLobby += startLobbyConnection;

        // Disconnect Events
        LoadingMenu.OnCancel += disconnect;
        LobbyMenu.OnCancelMatch += disconnect;
    }

    private void startLobbyConnection(bool isHost, NetworkTransportTypes transportType, string address)
    {
        print("Starting Connection");

        /* Setup Transport */
        _transportType = transportType;
        if(_transport is UNetTransport unet)
        {
            unet.ConnectAddress = address;
            unet.ConnectPort = _port;
            unet.ServerListenPort = _port;

            if(isHost) // Make sure IP address to Host the server at is the localhost
            {
                unet.ConnectAddress = "127.0.0.1";
            }

        }
        else if(_transport is PhotonRealtimeTransport photon)
        {
            photon.RoomName = address;
        }

        /* Setup Connect Events */
        // Defer connection event to trigger together with the _netManager events
        if(isHost)
        {
            // Run self unsubscribing Action on Host Started
            Action hostIsConnected = null;
            hostIsConnected = () => {
                _netManager.OnServerStarted -= hostIsConnected;

                #if UNITY_EDITOR
                    Debug.Log("Host Connected.");
                #endif

                OnConnected?.Invoke(true);
            };

            _netManager.OnServerStarted += hostIsConnected;
        }
        else
        {
            // Run self unsubscribing Action on this Client connected
            Action<ulong> clientIsConnected = null;
            clientIsConnected = (ulong clientID) => {
                if(clientID == _netManager.ServerClientId)
                {
                    _netManager.OnClientConnectedCallback -= clientIsConnected;

                    #if UNITY_EDITOR
                        Debug.Log($"Client Connected. ID: {clientID}");
                    #endif

                    OnConnected?.Invoke(false);
                }
            };

            _netManager.OnClientConnectedCallback += clientIsConnected;
        }

        /* Start Connection */
        if(isHost)
        {
            _netManager.StartHost();
        }
        else
        {
            _netManager.StartClient();
        }
    }

    private IEnumerator disconnectAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        disconnect();
    }

    private void disconnect()
    {
        // Can't disconnect if you're neither a Server nor Client (Host is both)
        if(!(_netManager.IsServer || _netManager.IsClient))
        {
            return;
        }

        if(_netManager.IsHost)
        {
            _netManager.StopHost();
            OnDisconnected?.Invoke(true);
        }
        /* Not valid for this Game, as all Servers are also Hosts */
        // else if(_netManager.IsServer)
        // {
        //     _netManager.StopServer();
        // }
        else if(_netManager.IsClient)
        {
            _netManager.StopClient();
            OnDisconnected?.Invoke(false);
        }
    }
}