﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

[SelectionBase]
public class Player : NetworkBehaviour
{
    public bool IsListComplete;

    private const string HELD_POSITION_NAME = "HeldPosition";
    private Throw _throwScript = null;

    private Rigidbody _rigidbody = null;

    // Held Item Vars
    private Transform _heldItemPosition;

    [HideInInspector]
    public NetworkVariableInt HeldItemType { get; private set; } = new NetworkVariableInt(
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.OwnerOnly
        },
        Item.NO_ITEMTYPE_CODE
    );

    private ItemGenerator _heldItemGenerator = null;
    [HideInInspector]
    public ItemGenerator HeldItemGenerator
    {
        get => _heldItemGenerator;
        set
        {
            _heldItemGenerator = value;
            HeldItemType.Value = value != null? value.ItemTypeCode : Item.NO_ITEMTYPE_CODE;
        }
    }

    public GameObject HeldItem { get; private set; } = null;
    public bool IsHoldingItem => HeldItemType.Value != Item.NO_ITEMTYPE_CODE && HeldItemGenerator != null;

    [HideInInspector]
    public bool IsDrivingCart;

    public bool CanInteract => !(IsDrivingCart || IsHoldingItem);

    public override void NetworkStart()
    {
        NetworkController.RegisterPlayer(this);

        if(IsOwner)
        {
            transform.Rotate(Vector3.up, 180);
        }
    }

    private void Awake()
    {
        _throwScript = GetComponent<Throw>();
        _rigidbody = GetComponent<Rigidbody>();
        _heldItemPosition = gameObject.transform.Find(HELD_POSITION_NAME);

        #if UNITY_EDITOR
            if(_throwScript == null)
            {
                Debug.LogError($"[{gameObject.name}]: Could not find Throw Script");
            }

            if(_rigidbody == null)
            {
                Debug.LogError($"[{gameObject.name}]: Could not find Player Rigidbody");
            }

            if(_heldItemPosition == null)
            {
                Debug.LogError($"[{gameObject.name}]: Could not find Held Item Position");
            }
        #endif

        IsDrivingCart = false;
        IsListComplete = false;

        HeldItemType.OnValueChanged = onHeldItemChange;
    }

    public delegate void OnBeforeDestroyDelegate(Player player);
    public event OnBeforeDestroyDelegate OnBeforeDestroy;
    private void OnDestroy()
    {
        OnBeforeDestroy?.Invoke(this);

        if(IsServer && HeldItemType.Value != Item.NO_ITEMTYPE_CODE)
        {
            HeldItemGenerator?.GenerateOwnerlessItem(_heldItemPosition.position, _heldItemPosition.rotation);
        }
    }

    private void onHeldItemChange(int previousItemType, int newItemType)
    {
        if(previousItemType != Item.NO_ITEMTYPE_CODE)
        {
            // Clear previously held item
            _heldItemPosition.DestroyAllChildren();
        }

        if(newItemType != Item.NO_ITEMTYPE_CODE)
        {
            var itemPrefab = ItemTypeList.ItemList[newItemType].ItemPrefab;
            var itemVisuals = itemPrefab?.GetComponent<Item>()?.ItemVisuals;

            if(itemVisuals != null)
            {
                // Place visual item on player's hand
                var generatedItem = Instantiate(itemVisuals.gameObject, Vector3.zero, Quaternion.identity, _heldItemPosition);

                generatedItem.transform.localPosition = Vector3.zero;
                generatedItem.transform.localRotation = Quaternion.identity;
                generatedItem.transform.localScale = itemVisuals.transform.localScale;

                generatedItem.GetComponent<ItemVisuals>()?.EnableHandVisuals();

                HeldItem = generatedItem;

                if(IsOwner)
                {
                    AimCanvas.Instance.ActivateAim();
                }
            }

            // Was unable to acquire a valid object visual,
            // should therefore update others that it is holding no item
            else if(IsOwner)
            {
                HeldItemGenerator = null;
            }
        }
        else
        {
            HeldItem = null;
        }
    }

    // This method is intended to be used when there's a need to update the
    // ItemGenerator without modifying HeldItemType, for example when updating
    // the server-side generator for another player (in case they disconnect)
    internal void UpdateItemGenerator(ItemGenerator generator)
    {
        _heldItemGenerator = generator;
    }

    public void ThrowItem(Action<Item> itemAction = null)
    {
        DropItem((item) => {
            _throwScript.ThrowItem(item);
            itemAction?.Invoke(item);
        });
    }

    public void DropItem(Action<Item> itemAction = null)
    {
        if(IsHoldingItem)
        {
            void positionItem(Item generatedItem)
            {
                if(!generatedItem.IsOwner)
                {
                    return;
                }

                var itemVisuals = _heldItemPosition.GetComponentInChildren<ItemVisuals>();

                generatedItem.transform.position = _heldItemPosition.position + itemVisuals.handPositionOffset;
                generatedItem.transform.rotation = _heldItemPosition.rotation;
                generatedItem.transform.eulerAngles += itemVisuals.handRotationOffset;

                HeldItemType.Value = Item.NO_ITEMTYPE_CODE;

                AimCanvas.Instance.DisableAim();

                itemAction?.Invoke(generatedItem);
            }

            HeldItemGenerator.GenerateItem(positionItem);
        }
    }

    public void ListComplete()
    {
        MatchMessages.Instance.EditMessage("Your list is complete");
        MatchMessages.Instance.ShowMessage();
        IsListComplete = true;
    }

    public void Teleport(Vector3 position, Vector3 eulerAngles = default)
    {
        if(!IsOwner)
        {
            return;
        }

        transform.position = position;

        if(eulerAngles != default(Vector3))
        {
            var newRotation = eulerAngles + _rigidbody.rotation.eulerAngles;
            _rigidbody.rotation = Quaternion.Euler(newRotation);
        }
    }
}
