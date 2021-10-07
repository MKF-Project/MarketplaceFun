using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;

[SelectionBase]
public class Player : NetworkBehaviour
{
    public bool IsListComplete;

    private const string HELD_POSITION_NAME = "HeldPosition";
    private Throw _throwScript = null;

    // Held Item Vars
    private Transform _heldItemPosition;

    [HideInInspector]
    public NetworkVariableInt HeldItemType = new NetworkVariableInt(
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.OwnerOnly
        },
        Item.NO_ITEMTYPE_CODE
    );

    [HideInInspector]
    public ItemGenerator HeldItemGenerator = null;

    public GameObject HeldItem { get; private set; } = null;
    public bool IsHoldingItem => HeldItemType.Value != Item.NO_ITEMTYPE_CODE;

    [HideInInspector]
    public bool IsDrivingCart;

    public bool CanInteract => !(IsDrivingCart || IsHoldingItem);

    public override void NetworkStart()
    {
        NetworkController.RegisterPlayer(this);
    }

    private void Awake()
    {
        _throwScript = GetComponent<Throw>();
        _heldItemPosition = gameObject.transform.Find(HELD_POSITION_NAME);

        #if UNITY_EDITOR
            if(_throwScript == null)
            {
                Debug.LogError($"[{gameObject.name}]: Could not find Throw Script");
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
            var meshObject = itemPrefab?.GetComponent<Item>()?.GetItemVisuals();

            if(meshObject != null)
            {
                // Place visual item on player's hand
                var generatedItem = Instantiate(meshObject, Vector3.zero, Quaternion.identity, _heldItemPosition);

                generatedItem.transform.localPosition = Vector3.zero;
                generatedItem.transform.localRotation = Quaternion.identity;
                generatedItem.transform.localScale = meshObject.transform.localScale;

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
                HeldItemType.Value = Item.NO_ITEMTYPE_CODE;
            }
        }
        else
        {
            HeldItem = null;
            HeldItemGenerator = null;
        }
    }

    public void ThrowItem()
    {
        DropItem(_throwScript.ThrowItem);
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

                generatedItem.transform.position = _heldItemPosition.position;
                generatedItem.transform.rotation = _heldItemPosition.rotation;

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
}
