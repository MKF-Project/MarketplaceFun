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
    public const float ITEM_DESTROY_DELAY = 4f;

    public GameObject Prefab => NetworkItemManager.NetworkItemPrefabs[ItemTypeCode];

    public Sprite UISticker;

    public List<ShelfItemGroup> ShelfItemGroups;

    private ItemVisuals _itemVisuals = null;
    private NetworkObject _networkObject;
    private Rigidbody _rigidbody = null;
    private AudioSource _itemSource;

    [HideInInspector]
    public bool IsOnThrow;

    private bool _isBeingDestroyed = false;
    private WaitForSeconds _destroyTimeout = new WaitForSeconds(ITEM_DESTROY_DELAY);

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

    public ulong ThrowerId;

    [Header("SFX")]
    public List<AudioClip> ItemHitSounds;

    private void Awake()
    {
        if(!TryGetComponent(out _rigidbody))
        {
            #if UNITY_EDITOR
                Debug.LogError($"[{name}]: Rigidbody not found!");
            #endif
        }

        if(!TryGetComponent(out _itemSource))
        {
            #if UNITY_EDITOR
                Debug.LogError($"[{name}]: AudioSource not found!");
            #endif
        }
    }

    // Object needs to be registered not before NetworkStart, like in Awake()
    // Because before this the object doesn't have a networkID
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
        if(_rigidbody.velocity.sqrMagnitude <= 0.1)
        {
            IsOnThrow = false;
            if(IsOwner && !_isBeingDestroyed)
            {
                TriggerDestroyItem();
            }
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag != ShoppingCartItem.SHOPPING_CART_TAG)
        {
            PlayHitSound();
        }

        if(IsOnThrow)
        {
            GameObject hitObject = other.gameObject;
            if(hitObject.CompareTag("Player"))
            {
                takeEffect = hitObject.GetComponent<TakeEffect>();
                takeEffect.OnTakeEffect(0, ThrowerId);

                IsOnThrow = false;
                if(IsOwner && !_isBeingDestroyed)
                {
                    TriggerDestroyItem();
                }
            }
        }
    }

    public GameObject GetShelfItemGroup(ShelfType shelfType)
    {
        // Belt shelves don't have an item group, as the items appear
        // individually one after another
        if(shelfType == ShelfType.Belt)
        {
            return null;
        }

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

    public void PlayHitSound() => _itemSource.PlayOneShot(ItemHitSounds[UnityEngine.Random.Range(0, ItemHitSounds.Count)]);

    private void TriggerDestroyItem()
    {
        _isBeingDestroyed = true;
        StartCoroutine(nameof(DestroyAfterSeconds));
    }

    private IEnumerator DestroyAfterSeconds()
    {
        yield return _destroyTimeout;
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
