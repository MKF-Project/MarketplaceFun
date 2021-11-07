using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public enum ShelfType
{
    Standard = 0,
    Freezer  = 1,
    Basket   = 2,
    Pallet   = 3,
    Display  = 4
}

public class Shelf : NetworkBehaviour
{
    private const string ITEM_GROUP_VISUALS_NAME = "ItemGroupVisuals";

    public ShelfType Type;

    [SerializeField]
    private ItemGenerator _itemGenerator = null;

    private ItemGenerator _itemGeneratorInternal = null;
    public ItemGenerator ItemGenerator
    {
        get => _itemGeneratorInternal;
        set
        {
            if(_itemGeneratorInternal != null)
            {
                _itemGeneratorInternal.OnRestocked -= RestockItem;
                _itemGeneratorInternal.OnDepleted -= ClearShelf;

                _itemGeneratorInternal.UnregisterShelf(this);
                // Don't do anything with the old generator after Unregistering from it,
                // we can't guarantee that it hasn't already been destroyed afterwards
            }

            _itemGeneratorInternal = value;
            if(value != null)
            {
                value.OnRestocked += RestockItem;
                value.OnDepleted += ClearShelf;

                value.RegisterShelf(this);
                // Don't do anything after Register, since the new Generator might
                // have changed this reference after this point
            }
        }
    }

    private Player _playerBuffer;

    private ulong _lastItemStocked = Item.NO_ITEMTYPE_CODE;

    private Action<Item> _itemAction = null;
    private Interactable _interactScript = null;

    private GameObject _itemGroupVisuals;

    // Note: Check comments on ItemGenerator for an overview of the
    // order of events between ItemGenerator and Shelf
    private void Awake()
    {
        _interactScript = gameObject.GetComponentInChildren<Interactable>();
        _itemGroupVisuals = transform.Find(ITEM_GROUP_VISUALS_NAME).gameObject;

        if(_interactScript == null)
        {
            return;
        }

        _interactScript.OnLookEnter += ShowButtonPrompt;
        _interactScript.OnLookExit  += HideButtonPrompt;
        _interactScript.OnInteract  += GiveItemToPlayer;
    }

    public override void NetworkStart()
    {
        // Don't subscribe to ItemGenerator events if the inspector var is not defined
        // or if we already defined the generator through some other script
        if(_itemGenerator == null || ItemGenerator != null)
        {
            return;
        }

        ItemGenerator = _itemGenerator;
    }

    private void OnDestroy()
    {
        if(_interactScript == null)
        {
            return;
        }

        _interactScript.OnLookEnter -= ShowButtonPrompt;
        _interactScript.OnLookExit  -= HideButtonPrompt;
        _interactScript.OnInteract  -= GiveItemToPlayer;

        ItemGenerator = null;
    }

    private void ShowButtonPrompt(GameObject player)
    {
        if(ItemGenerator.IsDepleted)
        {
            return;
        }

        var playerScript = player.GetComponent<Player>();
        if(playerScript != null && !playerScript.IsHoldingItem)
        {
            _interactScript.InteractUI.SetActive(true);
        }
    }

    private void HideButtonPrompt(GameObject player)
    {
        _interactScript.InteractUI.SetActive(false);
    }

    private void GiveItemToPlayer(GameObject player)
    {
        if(ItemGenerator.IsDepleted)
        {
            return;
        }

        if(player.TryGetComponent<Player>(out _playerBuffer))
        {
            ItemGenerator.GiveItemToPlayer(_playerBuffer);
        }
    }

    private void RestockItem(ulong itemID)
    {
        if(itemID != _lastItemStocked)
        {
            _itemGroupVisuals.DestroyAllChildren();

            var item = NetworkItemManager.GetItemPrefabScript(itemID);
            var itemGroupVisuals = item?.GetShelfItemGroup(Type);

            if(itemGroupVisuals != null)
            {
                var visualsInstance = Instantiate(itemGroupVisuals, transform.position, transform.rotation);

                visualsInstance.transform.SetParent(_itemGroupVisuals.transform, false);
                visualsInstance.transform.localPosition = Vector3.zero;
                visualsInstance.transform.localRotation = Quaternion.identity;

                _lastItemStocked = itemID;
            }
            else
            {
                _lastItemStocked = Item.NO_ITEMTYPE_CODE;
            }

        }

        _itemGroupVisuals.SetActive(true);
    }

    private void ClearShelf()
    {
        _itemGroupVisuals.SetActive(false);
    }
}
