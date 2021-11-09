using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class ItemGeneratorOld : NetworkBehaviour
{
    [SerializeField]
    private GameObject _itemPrefab = null;

    [HideInInspector]
    public ulong ItemTypeCode { get; private set; } = Item.NO_ITEMTYPE_CODE;

    private Action<Item> _itemAction = null;
    private Interactable _interactScript = null;

    // private void Awake()
    // {
    //     NetworkObject itemNetObject = null;
    //     if(_itemPrefab != null)
    //     {
    //         itemNetObject = _itemPrefab.GetComponent<NetworkObject>();
    //     }

    //     if(itemNetObject != null && NetworkItemManager.NetworkItemPrefabs.ContainsKey(itemNetObject.PrefabHash))
    //     {
    //         ItemTypeCode = itemNetObject.PrefabHash;
    //         _itemPrefab = NetworkItemManager.NetworkItemPrefabs[ItemTypeCode];
    //     }
    //     else
    //     {
    //         ItemTypeCode = Item.NO_ITEMTYPE_CODE;
    //         _itemPrefab = null;
    //     }

    //     _interactScript = gameObject.GetComponentInChildren<Interactable>();

    //     if(_interactScript == null)
    //     {
    //         return;
    //     }

    //     _interactScript.OnLookEnter += showButtonPrompt;
    //     _interactScript.OnLookExit  += hideButtonPrompt;
    //     _interactScript.OnInteract  += giveItemToPlayer;
    // }

    // private void OnDestroy()
    // {
    //     if(_interactScript == null)
    //     {
    //         return;
    //     }

    //     _interactScript.OnLookEnter -= showButtonPrompt;
    //     _interactScript.OnLookExit  -= hideButtonPrompt;
    //     _interactScript.OnInteract  -= giveItemToPlayer;
    // }

    // private void showButtonPrompt(GameObject player)
    // {
    //     // Show UI if not holding item or driving a shopping cart
    //     var playerScript = player.GetComponent<Player>();
    //     if(playerScript != null && !playerScript.IsHoldingItem)
    //     {
    //         _interactScript.InteractUI.SetActive(true);
    //     }
    // }

    // private void hideButtonPrompt(GameObject player)
    // {
    //     // Hide button prompt
    //     _interactScript.InteractUI.SetActive(false);
    // }

    // private void giveItemToPlayer(GameObject player)
    // {
    //     var playerScript = player.GetComponent<Player>();
    //     if(!playerScript.IsHoldingItem)
    //     {
    //         playerScript.HeldItemGenerator = this;
    //         AssignPlayerItemGenerator_ServerRpc();
    //     }
    // }


    // public void GenerateItem(Action<Item> itemAction = null)
    // {
    //     #if UNITY_EDITOR
    //         Debug.Log($"[{gameObject.name}]: Trigger generate item");
    //     #endif

    //     _itemAction = itemAction;
    //     GenerateItem_ServerRpc();
    // }

    // // This one is intended to be used when a player disconnects while holding an item
    // // The item should be created server-side after the player disconnects
    // public void GenerateOwnerlessItem(Vector3 location, Quaternion rotation)
    // {
    //     if(!IsServer)
    //     {
    //         return;
    //     }

    //     #if UNITY_EDITOR
    //         Debug.Log($"[{gameObject.name}]: Trigger generate NO OWNER item");
    //     #endif

    //     SpawnItemWithOwnership(NetworkController.ServerID, location, rotation);
    // }

    // private NetworkObject SpawnItemWithOwnership(ulong ownerID, Vector3 location, Quaternion rotation)
    // {
    //     if(_itemPrefab == null)
    //     {
    //         return null;
    //     }

    //     var generatedItem = Instantiate(_itemPrefab, location, rotation);

    //     var itemNetworkObject = generatedItem.GetComponent<NetworkObject>();
    //     itemNetworkObject.SpawnWithOwnership(ownerID, destroyWithScene: true);

    //     return itemNetworkObject;
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void AssignPlayerItemGenerator_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    // {
    //     var player = NetworkController.GetPlayerByID(rpcReceiveParams.Receive.SenderClientId);
    //     if(player != null)
    //     {
    //         player.UpdateItemGenerator(this);
    //     }
    // }

    // [ServerRpc(RequireOwnership = false)]
    // private void GenerateItem_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    // {
    //     var itemNetworkObject = SpawnItemWithOwnership(rpcReceiveParams.Receive.SenderClientId, Vector3.zero, Quaternion.identity);
    //     if(itemNetworkObject == null)
    //     {
    //         return;
    //     }

    //     GenerateItem_ClientRpc(itemNetworkObject.PrefabHash, itemNetworkObject.NetworkObjectId, ItemTypeCode, rpcReceiveParams.ReturnRpcToSender());
    // }

    // [ClientRpc]
    // private void GenerateItem_ClientRpc(ulong prefabHash, ulong id, ulong itemTypeCode, ClientRpcParams clientRpcParams = default)
    // {
    //     GameObject itemGenerated = NetworkItemManager.GetNetworkItem(prefabHash, id);

    //     if (itemGenerated != null)
    //     {
    //         var itemScript = itemGenerated.GetComponent<Item>();
    //         _itemAction?.Invoke(itemScript);
    //         _itemAction = null;
    //     }
    // }

}
