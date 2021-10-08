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
        }
    }


    public void GenerateItem(Action<Item> itemAction = null)
    {

            Debug.Log($"[{gameObject.name}]: Trigger generate item");


        _itemAction = itemAction;
        GenerateItem_ServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void GenerateItem_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    {
        var generatedItem = Instantiate(_itemPrefab, Vector3.zero, Quaternion.identity);

        var item = generatedItem.GetComponent<Item>();
        item.ItemTypeCode = ItemTypeCode;

        var itemNetworkObject = generatedItem.GetComponent<NetworkObject>();
        itemNetworkObject.SpawnWithOwnership(rpcReceiveParams.Receive.SenderClientId, destroyWithScene: true);

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
        }
    }

}
