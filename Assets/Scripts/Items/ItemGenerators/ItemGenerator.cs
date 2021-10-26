﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public abstract class ItemGenerator : NetworkBehaviour
{
    // Generation Events
    public delegate void OnDepletedDelegate();
    public event OnDepletedDelegate OnDepleted;
    protected void InvokeOnDepleted() => OnDepleted?.Invoke();

    public delegate void OnRestockedDelegate(ulong itemID);
    public event OnRestockedDelegate OnRestocked;
    protected void InvokeOnRestocked(ulong itemID) => OnRestocked?.Invoke(itemID);

    // Selecting Item
    [SerializeField]
    protected List<GameObject> _itemPool = null;
    protected NetworkObject _netObjectBuffer = null;
    public List<ulong> ItemPool { get; protected set; }

    public abstract ulong ItemInStock { get; }
    public abstract bool IsDepleted { get; }
    public virtual bool IsStocked => !IsDepleted;

    // Spawning Item
    protected Action<Item> _afterSpawn = null;
    protected Player _currentPlayer = null;

    public virtual void GiveItemToPlayer(Player player)
    {
        _currentPlayer = player;
        _currentPlayer._currentGenerator = this;
    }

    protected virtual void Awake()
    {
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
        if(IsStocked)
        {
            OnRestocked?.Invoke(ItemInStock);
        }
    }

    // Spawn
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
}