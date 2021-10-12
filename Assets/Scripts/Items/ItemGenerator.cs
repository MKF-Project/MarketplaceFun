using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class ItemGenerator : NetworkBehaviour
{
    public int ItemTypeCode;

    private GameObject _itemPrefab;
    private Action<Item> _itemAction = null;
    private Interactable _interactScript = null;

    private void Awake()
    {
        _itemPrefab = ItemTypeList.ItemList[ItemTypeCode].ItemPrefab;

        _interactScript = gameObject.GetComponentInChildren<Interactable>();

        if(_interactScript == null)
        {
            return;
        }

        _interactScript.OnLookEnter += showButtonPrompt;
        _interactScript.OnLookExit  += hideButtonPrompt;
        _interactScript.OnInteract  += giveItemToPlayer;
    }

    private void OnDestroy()
    {
        if(_interactScript == null)
        {
            return;
        }

        _interactScript.OnLookEnter -= showButtonPrompt;
        _interactScript.OnLookExit  -= hideButtonPrompt;
        _interactScript.OnInteract  -= giveItemToPlayer;
    }

    private void showButtonPrompt(GameObject player)
    {
        // Show UI if not holding item or driving a shopping cart
        var playerScript = player.GetComponent<Player>();
        if(playerScript != null && !playerScript.IsHoldingItem)
        {
            _interactScript.InteractUI.SetActive(true);
        }
    }

    private void hideButtonPrompt(GameObject player)
    {
        // Hide button prompt
        _interactScript.InteractUI.SetActive(false);
    }

    private void giveItemToPlayer(GameObject player)
    {
        var playerScript = player.GetComponent<Player>();
        if(!playerScript.IsHoldingItem)
        {
            playerScript.HeldItemType.Value = ItemTypeCode;
            playerScript.HeldItemGenerator = this;
            AssignPlayerItemGenerator_ServerRpc();
        }
    }


    public void GenerateItem(Action<Item> itemAction = null)
    {
        #if UNITY_EDITOR
            Debug.Log($"[{gameObject.name}]: Trigger generate item");
        #endif

        _itemAction = itemAction;
        GenerateItem_ServerRpc();
    }

    // This one is intended to be used when a player disconnects while holding an item
    // The item should be created server-side after the player disconnects
    public void GenerateOwnerlessItem(Vector3 location, Quaternion rotation)
    {
        if(!IsServer)
        {
            return;
        }

        #if UNITY_EDITOR
            Debug.Log($"[{gameObject.name}]: Trigger generate NO OWNER item");
        #endif

        SpawnItemWithOwnership(NetworkController.ServerID, location, rotation);
    }

    private NetworkObject SpawnItemWithOwnership(ulong ownerID, Vector3 location, Quaternion rotation)
    {
        var generatedItem = Instantiate(_itemPrefab, location, rotation);
        var item = generatedItem.GetComponent<Item>();
        item.ItemTypeCode = ItemTypeCode;

        var itemNetworkObject = generatedItem.GetComponent<NetworkObject>();
        itemNetworkObject.SpawnWithOwnership(ownerID, destroyWithScene: true);

        return itemNetworkObject;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AssignPlayerItemGenerator_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    {
        var player = NetworkController.GetPlayerByID(rpcReceiveParams.Receive.SenderClientId);
        if(player != null)
        {
            player.HeldItemGenerator = this;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GenerateItem_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    {
        var itemNetworkObject = SpawnItemWithOwnership(rpcReceiveParams.Receive.SenderClientId, Vector3.zero, Quaternion.identity);
        GenerateItem_ClientRpc(itemNetworkObject.PrefabHash, itemNetworkObject.NetworkObjectId, ItemTypeCode, rpcReceiveParams.ReturnRpcToSender());
    }

    [ClientRpc]
    private void GenerateItem_ClientRpc(ulong prefabHash, ulong id, int itemTypeCode, ClientRpcParams clientRpcParams = default)
    {
        GameObject itemGenerated = NetworkItemManager.GetNetworkItem(prefabHash, id);

        if (itemGenerated != null)
        {
            var itemScript = itemGenerated.GetComponent<Item>();
            itemScript.ItemTypeCode = itemTypeCode;
            _itemAction?.Invoke(itemScript);
            _itemAction = null;
        }
    }

}
