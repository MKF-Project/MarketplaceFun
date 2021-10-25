using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Configuration;

public class NetworkItemManager : MonoBehaviour
{
    // Prefabs
    public static Dictionary<ulong, GameObject> NetworkItemPrefabs { get; private set; } = null;

    [SerializeField]
    private List<GameObject> _registeredItems = new List<GameObject>();
    private NetworkManager _netManager;
    private NetworkObject _networkItemBuffer = null;

    // Spawned Items
    private static NetworkItemManager _instance = null;
    private static Dictionary<string, GameObject> SpawnedItemList = null;

    // Spawns
    private static Action<Item> _afterSpawn = default;

    private void Awake()
    {
        if(_instance != null)
        {
            // NetworkItemManager is NOT the primary script on this GameObject,
            // so it only Destroys itself when it
            // detects a singleton already present
            Destroy(this);

            return;
        }

        _instance = this;
        gameObject.EnsureObjectDontDestroy();

        InitializePrefabs();

        SpawnedItemList = new Dictionary<string, GameObject>();

    }

    private void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
            SpawnedItemList = null;
        }
    }

    /* Prefabs */
    private void InitializePrefabs()
    {
        _netManager = GetComponent<NetworkManager>();
        NetworkItemPrefabs = new Dictionary<ulong, GameObject>(_registeredItems.Count);

        _registeredItems.ForEach(prefab => {
            // Add registered item to NetworkManager's NetworkPrefabs list
            var itemNetworkPrefab = new NetworkPrefab();
            itemNetworkPrefab.Prefab = prefab;
            itemNetworkPrefab.PlayerPrefab = false;

            _netManager.NetworkConfig.NetworkPrefabs.Add(itemNetworkPrefab);

            // then add it to our own Item Prefabs dictionary
            prefab.TryGetComponent<NetworkObject>(out _networkItemBuffer);
            var itemCode = _networkItemBuffer != null? _networkItemBuffer.PrefabHash : Item.NO_ITEMTYPE_CODE;

            if(NetworkItemPrefabs.ContainsKey(itemCode))
            {
                #if UNITY_EDITOR
                    Debug.LogError($"[NetworkItemManager]: Can't register {prefab.name} with ID {itemCode}, already registered to {NetworkItemPrefabs[itemCode].name}");
                #endif

                return;
            }

            NetworkItemPrefabs.Add(itemCode, prefab);

            _networkItemBuffer = null;
        });
    }

    /* Spawning */
    public static void RegisterItem(ulong prefabHash, ulong id, GameObject item)
    {


        string stringifiedKey = StringifyKey(prefabHash, id);
        if (!SpawnedItemList.ContainsKey(stringifiedKey))
        {
            SpawnedItemList.Add(stringifiedKey, item);
        }
        #if UNITY_EDITOR
            else
            {
                Debug.LogError($"Add same Item {prefabHash} - {id}" );
            }
        #endif
    }

    public static void UnregisterItem(ulong prefabHash, ulong id)
    {
        string stringifiedKey = StringifyKey(prefabHash, id);
        if (!SpawnedItemList.Remove(stringifiedKey))
        {
            #if UNITY_EDITOR
                Debug.LogError($"Item is not registered {prefabHash} - {id}" );
            #endif
        }

    }

    private static string StringifyKey(ulong prefabHash, ulong id)
    {
        return $"{prefabHash}{id}";
    }

    public static GameObject GetNetworkItem(ulong prefabHash, ulong id)
    {
        print("S " + prefabHash + " - " + id);

        return SpawnedItemList[StringifyKey(prefabHash, id)];
    }

    // Spawn
    public static void SpawnItemAsOwner(ulong prefabHash, Vector3 position = default, Quaternion rotation = default, Action<Item> afterSpawn = default)
    {
        if(!NetworkController.IsClient)
        {
            return;
        }

        _afterSpawn = afterSpawn;
        _instance.SpawnItem_ServerRpc(prefabHash, position, rotation);
    }

    // This one is intended to be used when a player disconnects while holding an item
    // The item should be created server-side after the player disconnects
    public static void SpawnOwnerlessItem(ulong prefabHash, Vector3 position = default, Quaternion rotation = default, Action<Item> afterSpawn = default)
    {
        if(!NetworkController.IsServer)
        {
            return;
        }

        var itemNetworkObject = Item.SpawnItemWithOwnership(prefabHash, NetworkController.ServerID, position, rotation);
        afterSpawn?.Invoke(itemNetworkObject.GetComponent<Item>());
    }

    // RPCs
    [ServerRpc(RequireOwnership = false)]
    private void SpawnItem_ServerRpc(ulong prefabHash, Vector3 position, Quaternion rotation, ServerRpcParams rpcReceiveParams = default)
    {
        var itemNetworkObject = Item.SpawnItemWithOwnership(prefabHash, rpcReceiveParams.Receive.SenderClientId, Vector3.zero, Quaternion.identity);
        if(itemNetworkObject == null)
        {
            return;
        }

        SpawnItem_ClientRpc(itemNetworkObject.PrefabHash, itemNetworkObject.NetworkObjectId, rpcReceiveParams.ReturnRpcToSender());
    }

    [ClientRpc]
    private void SpawnItem_ClientRpc(ulong prefabHash, ulong id, ClientRpcParams clientRpcParams = default)
    {
        var itemGenerated = NetworkItemManager.GetNetworkItem(prefabHash, id);

        if(itemGenerated != null)
        {
            _afterSpawn?.Invoke(itemGenerated.GetComponent<Item>());
            _afterSpawn = null;
        }
    }
}
