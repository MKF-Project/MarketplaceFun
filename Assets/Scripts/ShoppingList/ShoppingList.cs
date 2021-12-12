using System;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using Random = System.Random;

public class ShoppingList : NetworkBehaviour
{
    private static System.Random _rng = null;

    public const int ITEM_LIST_AMOUNT = 5;

    public Dictionary<ulong, ShoppingListItem> ItemDictionary;

    public int _quantityChecked;

    private int _randomSeed;

    private void Start()
    {
        _quantityChecked = 0;
        ItemDictionary = new Dictionary<ulong, ShoppingListItem>();

        if (IsOwner)
        {
            Debug.Log("Colocou Owner");
            NetworkController.OnDisconnected += EraseListClient;
        }

        if (IsServer)
        {
            Debug.Log("Colocou Server");
            _randomSeed = (int) Time.time;
            NetworkController.OnOtherClientDisconnected += EraseListServer;

            ItemGenerator.OnGeneratablesDefined += GeneratePlayerLists;
        }

        MatchManager.OnMatchExit += EraseSelfList;
    }

    private void OnDestroy()
    {
        // Don't verify IsOwner or IsClient (bug if verify)
        NetworkController.OnDisconnected -= EraseListClient;
        NetworkController.OnOtherClientDisconnected -= EraseListServer;
        MatchManager.OnMatchExit -= EraseSelfList;

        ItemGenerator.OnGeneratablesDefined -= GeneratePlayerLists;
    }

    // Only on server
    public void GeneratePlayerLists(IEnumerable<ulong> setOfPossibleItems)
    {
        if(!IsServer)
        {
            return;
        }

        if(_rng == null)
        {
            _rng = new System.Random();
        }

        var itemList = new List<ulong>(setOfPossibleItems);
        var itemAmount = Math.Min(ITEM_LIST_AMOUNT, itemList.Count);

        var shoppingList = new List<ShoppingListItem>(itemAmount);
        while(shoppingList.Count < itemAmount)
        {
            var randomIndex = _rng.Next(itemList.Count);

            var listItem = new ShoppingListItem(itemList[randomIndex]);
            shoppingList.Add(listItem);
            ItemDictionary.Add(itemList[randomIndex], listItem);

            itemList.RemoveAt(randomIndex);
        }

        ReceiveList_ClientRpc(new SerializedShoppingList(shoppingList));
    }

    // Populate List on Client RPC
    [ClientRpc]
    private void ReceiveList_ClientRpc(SerializedShoppingList serializedShoppingList)
    {
        var itemList = serializedShoppingList.Array.ToList();
        if(!IsServer)
        {
            foreach(ShoppingListItem item in itemList)
            {
                ItemDictionary.Add(item.ItemCode, item);
            }
        }

        if(IsOwner)
        {
            Debug.Log("Im owner: " + GetComponent<NetworkObject>().OwnerClientId);
            ShoppingListUI.FillUIItems(itemList);
        }
    }

    public void EraseSelfList()
    {
        ItemDictionary = new Dictionary<ulong, ShoppingListItem>();
        if(IsOwner)
        {
            ShoppingListUI.EraseItems();
        }
        _quantityChecked = 0;
    }

    public void EraseListServer(ulong playerId)
    {
        ItemDictionary = new Dictionary<ulong, ShoppingListItem>();
    }

    public void EraseListClient(bool wasHost, bool connectionWasLost)
    {
        ItemDictionary = new Dictionary<ulong, ShoppingListItem>();
        if(IsOwner)
        {
            ShoppingListUI.EraseItems();
        }
    }

    public bool AssertItemWillCompleteList(ulong itemCode) => ItemDictionary.ContainsKey(itemCode) && !ItemDictionary[itemCode].Caught && _quantityChecked + 1 >= ItemDictionary.Count;

    public bool CheckItem(ulong itemCode)
    {
        if(!ItemDictionary.ContainsKey(itemCode))
        {
            return false;
        }

        ShoppingListItem listItem = ItemDictionary[itemCode];
        if(listItem.Caught)
        {
            return false;
        }

        listItem.Caught = true;
        ItemDictionary.Remove(itemCode);
        ItemDictionary.Add(itemCode, listItem);
        ShoppingListUI.CheckItem(itemCode);
        _quantityChecked++;

        WarnItemChecked_ServerRpc(NetworkController.SelfID);

        return true;
    }

    public void UncheckItem(ulong itemCode)
    {
        if(!ItemDictionary.ContainsKey(itemCode))
        {
            return;
        }

        ShoppingListItem listItem = ItemDictionary[itemCode];
        listItem.Caught = false;
        ItemDictionary.Remove(itemCode);
        ItemDictionary.Add(itemCode, listItem);
        ShoppingListUI.UncheckItem(itemCode);
        _quantityChecked--;

        WarnItemUnchecked_ServerRpc(NetworkController.SelfID);
    }


    public bool IsListChecked() => _quantityChecked == ItemDictionary.Count;

    [ServerRpc]
    public void WarnItemChecked_ServerRpc(ulong playerId)
    {
        if(!IsOwner)
        {
            _quantityChecked++;
        }

        WarnItemChecked_ClientRpc(playerId);
    }

    [ClientRpc]
    public void WarnItemChecked_ClientRpc(ulong playerId)
    {
        if(!IsOwner && !IsServer)
        {
            _quantityChecked++;
        }

        PlayerProgress.Instance.AddItemToPlayer(playerId);
    }

    [ServerRpc]
    public void WarnItemUnchecked_ServerRpc(ulong playerId)
    {
        if(!IsOwner)
        {
            _quantityChecked--;
        }

        WarnItemUnchecked_ClientRpc(playerId);
    }

    [ClientRpc]
    public void WarnItemUnchecked_ClientRpc(ulong playerId)
    {
        if(!IsOwner && !IsServer)
        {
            _quantityChecked--;
        }

        PlayerProgress.Instance.RemoveItemToPlayer(playerId);
    }
}
