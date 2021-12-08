using System;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using Random = System.Random;

public class ShoppingList : NetworkBehaviour
{
    public const int ITEM_LIST_AMOUNT = 5;

    public Dictionary<ulong, ShoppingListItem> ItemDictionary;
    public ShoppingListUI ShoppingListUi;

    public int _quantityChecked;

    private int _randomSeed;

    private void Start()
    {
        _quantityChecked = 0;
        ItemDictionary = new Dictionary<ulong, ShoppingListItem>();

        if (IsOwner)
        {
            ShoppingListUi = GameObject.FindGameObjectsWithTag("ShoppingListUI")[0].GetComponent<ShoppingListUI>();
            Debug.Log("Colocou Owner");
            NetworkController.OnDisconnected += EraseListClient;
        }

        if (IsServer)
        {
            Debug.Log("Colocou Server");
            _randomSeed = (int) Time.time;
            NetworkController.OnOtherClientDisconnected += EraseListServer;

            ItemGenerator.OnGeneratablesDefined += GenerateList;
        }

        MatchManager.OnMatchExit += EraseSelfList;
    }

    private void OnDestroy()
    {
        // Don't verify IsOwner or IsClient (bug if verify)
        NetworkController.OnDisconnected -= EraseListClient;
        NetworkController.OnOtherClientDisconnected -= EraseListServer;
        MatchManager.OnMatchExit -= EraseSelfList;

        ItemGenerator.OnGeneratablesDefined -= GenerateList;
    }

    // Only on server
    public void GenerateList(IEnumerable<ulong> setOfPossibleItems)
    {
        if(!IsServer)
        {
            return;
        }

        var random = new Random(_randomSeed);
        var itemList = new List<ulong>(setOfPossibleItems);

        var numberOfItems = Mathf.Min(ITEM_LIST_AMOUNT, itemList.Count);
        while(numberOfItems > 0)
        {
            int randomIndex = random.Next(0, itemList.Count);

            ShoppingListItem shoppingListItem = new ShoppingListItem(itemList[randomIndex]);

            // Debug.Log(GetComponent<NetworkObject>().OwnerClientId + ": " + allItemsList[randomIndex].Name);

            ItemDictionary.Add(itemList[randomIndex], shoppingListItem);
            itemList.RemoveAt(randomIndex);
            numberOfItems--;
        }

        SerializedShoppingList serializedShoppingList = new SerializedShoppingList(ItemDictionary.Values.ToList());
        ReceiveList_ClientRpc(serializedShoppingList);

    }

    // Populate List on Client RPC
    [ClientRpc]
    public void ReceiveList_ClientRpc(SerializedShoppingList serializedShoppingList)
    {

        List<ShoppingListItem> itemList = serializedShoppingList.Array.ToList();
        if (!IsServer)
        {
            foreach (ShoppingListItem item in itemList)
            {
                ItemDictionary.Add(item.ItemCode, item);
            }
        }

        if (IsOwner)
        {
            Debug.Log("Im owner: " + GetComponent<NetworkObject>().OwnerClientId);
            ShoppingListUi.FillUIItems(itemList);
        }
    }

    public void EraseSelfList()
    {
        ItemDictionary = new Dictionary<ulong, ShoppingListItem>();
        if (IsOwner)
        {
            ShoppingListUi.EraseItems();
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
        if (IsOwner)
        {
            ShoppingListUi.EraseItems();
        }
    }

    public bool CheckItem(ulong itemCode)
    {
        if(!ItemDictionary.ContainsKey(itemCode))
        {
            return false;
        }

        ShoppingListItem listItem = ItemDictionary[itemCode];
        if (listItem.Caught)
        {
            return false;
        }

        listItem.Caught = true;
        ItemDictionary.Remove(itemCode);
        ItemDictionary.Add(itemCode, listItem);
        ShoppingListUi.CheckItem(itemCode);
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
        ShoppingListUi.UncheckItem(itemCode);
        _quantityChecked--;

        WarnItemUnchecked_ServerRpc(NetworkController.SelfID);
    }


    public bool IsListChecked()
    {
        if (_quantityChecked == ItemDictionary.Count)
        {
            return true;
        }

        return false;
    }

    [ServerRpc]
    public void WarnItemChecked_ServerRpc(ulong playerId)
    {
        WarnItemChecked_ClientRpc(playerId);
    }
    
    [ClientRpc]
    public void WarnItemChecked_ClientRpc(ulong playerId)
    {
        PlayerProgress.Instance.AddItemToPlayer(playerId);
    }
    
    
    [ServerRpc]
    public void WarnItemUnchecked_ServerRpc(ulong playerId)
    {
        WarnItemUnchecked_ClientRpc(playerId);
    }
    
    [ClientRpc]
    public void WarnItemUnchecked_ClientRpc(ulong playerId)
    {
        PlayerProgress.Instance.RemoveItemToPlayer(playerId);
    }
}
