﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public abstract class ItemGenerator : NetworkBehaviour
{
    // List Generation
    public delegate void OnGeneratablesDefinedDelegate(IEnumerable<ulong> generatables);
    public static event OnGeneratablesDefinedDelegate OnGeneratablesDefined;

    private static bool _hasDefinedGeneratables = false;
    public static HashSet<ulong> Generatables { get; private set; } = new HashSet<ulong>();

    private static bool _isLongLivedGenEventSet = false;

    // Generation Events
    public delegate void OnDepletedDelegate();
    public event OnDepletedDelegate OnDepleted;
    protected void InvokeOnDepleted() => OnDepleted?.Invoke();

    public delegate void OnShelfDepletedDelegate(Shelf shelf);
    public event OnShelfDepletedDelegate OnShelfDepleted;
    protected void InvokeOnShelfDepleted(Shelf shelf) => OnShelfDepleted?.Invoke(shelf);

    public delegate void OnRestockedDelegate(ulong itemID);
    public event OnRestockedDelegate OnRestocked;
    protected void InvokeOnRestocked(ulong itemID) => OnRestocked?.Invoke(itemID);

    public delegate void OnShelfRestockedDelegate(Shelf shelf, ulong itemID);
    public event OnShelfRestockedDelegate OnShelfRestocked;
    protected void InvokeOnShelfRestocked(Shelf shelf, ulong itemID) => OnShelfRestocked?.Invoke(shelf, itemID);

    // Selecting Item
    [SerializeField]
    protected List<GameObject> _itemPool = null;
    protected NetworkObject _netObjectBuffer = null;
    public List<ulong> ItemPool { get; protected set; }

    public abstract ulong RequestItemInStock(Shelf shelf);
    public abstract bool RequestIsDepleted(Shelf shelf);
    public virtual bool RequestIsStocked(Shelf shelf) => !RequestIsDepleted(shelf);

    // Populated on Server. Defines which items from the pool this generator
    // can actually generate during the match. Used to create shopping lists.
    // Must have been set by the time NetworkStart happens.
    public List<ulong> GeneratableItems => IsServer? _generatableItems : null;
    protected List<ulong> _generatableItems = null;

    // Spawning Item
    protected Action<Item> _afterSpawn = null;
    protected Player _currentPlayer = null;

    private static void PopulateGeneratables()
    {
        var generatorInstances = GameObject.FindObjectsOfType<ItemGenerator>();
        for(int i = 0; i < generatorInstances.Length; i++)
        {
            generatorInstances[i].GeneratableItems?.ForEach(itemID => Generatables.Add(itemID));
        }

        _hasDefinedGeneratables = true;
        OnGeneratablesDefined?.Invoke(Generatables);
    }

    private static void ClearGeneratables(string sceneName)
    {
        _hasDefinedGeneratables = false;
        Generatables.Clear();
    }

    protected virtual void Awake()
    {
        // We set the event once per game start,
        // and keep it there until the game quits
        if(!_isLongLivedGenEventSet)
        {
            _isLongLivedGenEventSet = true;
            SceneManager.OnMatchLoaded += ClearGeneratables;
        }

        if(_itemPool == null)
        {
            #if UNITY_EDITOR
                Debug.LogError($"[{gameObject.name}/{nameof(ItemGenerator)}]: Itempool is not populated. You probably forgot to fill it with items, or you are using an ItemGenerator script directly instead of one within a GameObject on the scene");
            #endif
            ItemPool = new List<ulong>();
            return;
        }

        ItemPool = new List<ulong>(_itemPool.Count);
        for(int i = 0; i < _itemPool.Count; i++)
        {
            if(!_itemPool[i].TryGetComponent<NetworkObject>(out _netObjectBuffer))
            {
                #if UNITY_EDITOR
                    Debug.LogWarning($"[{gameObject.name}/{nameof(ItemGenerator)}]: Prefab {_itemPool[i].name} is not an Item. Skipping...");
                #endif
                continue;
            }

            ItemPool.Add(_netObjectBuffer.PrefabHash);
        }
    }

    public override void NetworkStart()
    {
        if(IsServer && !_hasDefinedGeneratables)
        {
            PopulateGeneratables();
        }
    }

    protected virtual void Start()
    {
        if(RequestIsStocked(null))
        {
            OnRestocked?.Invoke(RequestItemInStock(null));
        }
    }

    // Initialization
    /** -- ORDER OF EVENTS --
     *
     * 1. ItemGenerator/Shelf Awake() (No set order between them)
     * 2. Register Shelves (During NetworkStart)
     * 3. First Item Stock on ItemGenerator.Start()
     *
     * Notes:
     * - Make sure NOT to register a shelf before ItemGenerator.Awake()
     *   (To be certain, no sooner than NetworkStart())
     * - If you think a shelf might be attached to a generator after Start(),
     *   remember to check the current stock manually.
     */

    // By default, an item generator does not need to know which
    // shelves are registered to it, so this method does nothing.
    // But in some cases we might want to keep track of each of the
    // shelves that use this ItemGenerator
    // (if we wanted to, for example, Spawn a different item for each shelf under this generator)
    public virtual void RegisterShelf(Shelf shelf)
    {
        #if UNITY_EDITOR
            Debug.Log($"[ItemGenerator]: {shelf.name} registered to {gameObject.name}");
        #endif
    }

    // As with the RegisterShelf method, this does nothing by default.
    // Use this if you need to know when a Shelf stops
    // making use of this ItemGenerator.
    public virtual void UnregisterShelf(Shelf shelf)
    {
        #if UNITY_EDITOR
            Debug.Log($"[ItemGenerator]: {shelf.name} unregistered from {gameObject.name}");
        #endif
    }

    // Spawn

    // Remove an item from the shelf
    public virtual ulong TakeItem(Shelf shelf) => RequestIsDepleted(shelf)? Item.NO_ITEMTYPE_CODE : RequestItemInStock(shelf);

    // Remove Item from shelf, then give it to the player
    public virtual void GiveItemToPlayer(Shelf shelf, Player player) => GiveSpecificItemToPlayer(player, TakeItem(shelf));

    public void GiveSpecificItemToPlayer(Player player, ulong itemID)
    {
        if(itemID != Item.NO_ITEMTYPE_CODE)
        {
            _currentPlayer = player;
            _currentPlayer._currentGenerator = this;

            player.HeldItemType.Value = itemID;
        }
    }

    public void GeneratePlayerHeldItem(Vector3 position = default, Quaternion rotation = default, Action<Item> afterSpawn = default)
    {
        if(!NetworkController.IsClient || _currentPlayer == null)
        {
            return;
        }

        _afterSpawn = afterSpawn;
        GenerateItem_ServerRpc(_currentPlayer.HeldItemType.Value, position, rotation);

        _currentPlayer._currentGenerator = null;
        _currentPlayer = null;
    }

    // RPCs
    [ServerRpc(RequireOwnership = false)]
    protected void GenerateItem_ServerRpc(ulong prefabHash, Vector3 position, Quaternion rotation, ServerRpcParams rpcReceiveParams = default)
    {
        var itemNetworkObject = Item.SpawnItemWithOwnership(prefabHash, rpcReceiveParams.Receive.SenderClientId, position, rotation);
        if(itemNetworkObject == null)
        {
            return;
        }

        GenerateItem_ClientRpc(itemNetworkObject.PrefabHash, itemNetworkObject.NetworkObjectId, rpcReceiveParams.ReturnRpcToSender());
    }

    [ClientRpc]
    protected void GenerateItem_ClientRpc(ulong prefabHash, ulong id, ClientRpcParams clientRpcParams = default)
    {
        var itemGenerated = NetworkItemManager.GetNetworkItem(prefabHash, id);

        if(itemGenerated != null)
        {
            _afterSpawn?.Invoke(itemGenerated.GetComponent<Item>());
            _afterSpawn = null;
        }
    }

    // Editor Utils
    protected virtual void OnDrawGizmosSelected()
    {
        var shelves = GameObject.FindObjectsOfType<Shelf>();
        for(int i = 0; i < shelves.Length; i++)
        {
            if(shelves[i]._itemGenerator == this)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, shelves[i].transform.position);
            }
        }
    }
}
