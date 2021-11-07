using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

[Serializable]
public struct ShelfItemGroup
{
    public ShelfType ShelfType;
    public GameObject ItemGroupPrefab;
}

[SelectionBase]
public class Item : NetworkBehaviour
{
    public const ulong NO_ITEMTYPE_CODE = ulong.MinValue;

    public GameObject Prefab => NetworkItemManager.NetworkItemPrefabs[ItemTypeCode];

    public Sprite UISticker;

    public List<ShelfItemGroup> ShelfItemGroups;

    private ItemVisuals _itemVisuals = null;
    private NetworkObject _networkObject;

    [HideInInspector]
    public bool IsOnThrow;

    public ulong ItemTypeCode => NetworkObject.PrefabHash;

    [Header("Effect")]
    public int EffectType;

    public ItemVisuals ItemVisuals
    {
        get
        {
            if(_itemVisuals == null)
            {
                _itemVisuals = transform.Find(ItemVisuals.ITEM_VISUALS_NAME)?.GetComponent<ItemVisuals>();
            }

            return _itemVisuals;
        }
    }

    public TakeEffect takeEffect;

    // Object needs to be registered not before NetworkStart, like Awake
    // Because before this the object doesn't have an networkId
    public override void NetworkStart()
    {
        _networkObject = GetComponent<NetworkObject>();
        IsOnThrow = false;
        EffectType = 0;
        RegisterItem();
    }

    private void OnDestroy()
    {
        UnregisterItem();
    }

    private void RegisterItem()
    {
        NetworkItemManager.RegisterItem(_networkObject.PrefabHash, _networkObject.NetworkObjectId, gameObject);
    }

    private void UnregisterItem()
    {
        NetworkItemManager.UnregisterItem(_networkObject.PrefabHash, _networkObject.NetworkObjectId);
    }

    private void Update()
    {
        if(IsOnThrow)
        {
            if(gameObject.GetComponent<Rigidbody>().velocity.sqrMagnitude <= 0.1)
            {
                TriggerDestroyItem();
            }
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        if(IsOnThrow)
        {
            GameObject hitObject = other.gameObject;
            if(hitObject.CompareTag("Player"))
            {
                takeEffect = hitObject.GetComponent<TakeEffect>();
                takeEffect.OnTakeEffect(0);
                TriggerDestroyItem();
            }
        }
    }

    public GameObject GetShelfItemGroup(ShelfType shelfType)
    {
        for(int i = 0; i < ShelfItemGroups.Count; i++)
        {
            if(ShelfItemGroups[i].ShelfType == shelfType)
            {
                return ShelfItemGroups[i].ItemGroupPrefab;
            }
        }

        return null;
    }

    public static NetworkObject SpawnItemWithOwnership(ulong prefabHash, ulong ownerID, Vector3 location, Quaternion rotation)
    {
        if(!NetworkController.IsServer)
        {
            return null;
        }

        var generatedItem = Instantiate(NetworkItemManager.NetworkItemPrefabs[prefabHash], location, rotation);

        var itemNetworkObject = generatedItem.GetComponent<NetworkObject>();
        itemNetworkObject.SpawnWithOwnership(ownerID, destroyWithScene: true);

        return itemNetworkObject;
    }

    private void TriggerDestroyItem()
    {
        IsOnThrow = false;
        StartCoroutine(nameof(DestroyAfterSeconds), 4f);
    }

    private IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        DestroyItem_ServerRpc();
    }

    // RPCs
    [ServerRpc]
    public void DestroyItem_ServerRpc()
    {
        DestroyItem_ClientRpc();
    }

    [ClientRpc]
    public void DestroyItem_ClientRpc()
    {
        Destroy(gameObject);
    }
}
