using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

[SelectionBase]
public class Player : NetworkBehaviour
{
    //public bool IsListComplete;

    [HideInInspector]
    public GameObject ShoppingCart;

    private const string HELD_POSITION_NAME = "HeldPosition";
    private Throw _throwScript = null;

    private Rigidbody _rigidbody = null;

    // Held Item Vars
    private Transform _heldItemPosition;
    private ItemVisuals _heldItemVisuals = null;

    internal ItemGenerator _currentGenerator = null;

    [HideInInspector]
    public NetworkVariableULong HeldItemType { get; private set; } = new NetworkVariableULong(
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.OwnerOnly
        },
        Item.NO_ITEMTYPE_CODE
    );

    public GameObject HeldItem { get; private set; } = null;
    public bool IsHoldingItem => HeldItemType.Value != Item.NO_ITEMTYPE_CODE;

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
        //IsListComplete = false;

        HeldItemType.OnValueChanged = OnHeldItemChange;
        MatchManager.OnMatchExit += PlayerReset;
    }



    public delegate void OnBeforeDestroyDelegate(Player player);
    public event OnBeforeDestroyDelegate OnBeforeDestroy;
    private void OnDestroy()
    {
        OnBeforeDestroy?.Invoke(this);

        if(IsServer && HeldItemType.Value != Item.NO_ITEMTYPE_CODE)
        {
            NetworkItemManager.SpawnOwnerlessItem(HeldItemType.Value, _heldItemPosition.position, _heldItemPosition.rotation);
        }
        MatchManager.OnMatchExit -= PlayerReset;

    }

    private void OnHeldItemChange(ulong previousItemType, ulong newItemType)
    {
        if(previousItemType != Item.NO_ITEMTYPE_CODE)
        {
            // Clear previously held item
            _heldItemPosition.DestroyAllChildren();
        }

        if(newItemType != Item.NO_ITEMTYPE_CODE)
        {
            _heldItemVisuals = NetworkItemManager.GetItemPrefabVisuals(newItemType);

            if(_heldItemVisuals != null)
            {
                // Place visual item on player's hand
                var generatedItem = Instantiate(_heldItemVisuals.gameObject, Vector3.zero, Quaternion.identity, _heldItemPosition);

                generatedItem.transform.localPosition = Vector3.zero;
                generatedItem.transform.localRotation = Quaternion.identity;
                generatedItem.transform.localScale = _heldItemVisuals.transform.localScale;

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
                HeldItemType.Value = Item.NO_ITEMTYPE_CODE;
            }
        }
        else
        {
            HeldItem = null;
            _heldItemVisuals = null;
        }
    }

    // This method is intended to be used when there's a need to update the
    // ItemGenerator without modifying HeldItemType, for example when updating
    // the server-side generator for another player (in case they disconnect)
    // internal void UpdateItemGenerator(ItemGeneratorOld generator)
    // {
    //     _heldItemGenerator = generator;
    // }

    public void ThrowItem(Action<Item> itemAction = null)
    {
        DropItem((item) => {
            _throwScript.ThrowItem(item);
            itemAction?.Invoke(item);
        });
    }

    public void DropItem(Action<Item> itemAction = null)
    {
        if(!IsHoldingItem)
        {
            return;
        }

        var itemVisuals = _heldItemPosition.GetComponentInChildren<ItemVisuals>();

        void positionItem(Item generatedItem)
        {
            if(!generatedItem.IsOwner)
            {
                return;
            }

            // We update position and rotation after it has been released
            // to account for the player movement
            generatedItem.transform.position = _heldItemPosition.position + itemVisuals.handPositionOffset;
            generatedItem.transform.rotation = _heldItemPosition.rotation * Quaternion.Euler(itemVisuals.handRotationOffset);

            HeldItemType.Value = Item.NO_ITEMTYPE_CODE;

            AimCanvas.Instance.DisableAim();

            itemAction?.Invoke(generatedItem);
        }

        var spawnPosition = _heldItemPosition.position + itemVisuals.handPositionOffset;
        var spawnRotation = _heldItemPosition.rotation * Quaternion.Euler(itemVisuals.handRotationOffset);

        _currentGenerator?.GeneratePlayerHeldItem(spawnPosition, spawnRotation, positionItem);
    }

    public void ReleaseCart()
    {
        if(IsDrivingCart)
        {
            GetComponent<CartControls>()?.DetachShoppingCart();
        }
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

    public void UseFirstPersonView()
    {
        var cameraScript = GetComponentInChildren<CameraScript>();
        if(cameraScript != null)
        {
            cameraScript.SetCameraOnPlayer();
        }
    }

    public void UseThirdPersonView()
    {
        var cameraScript = GetComponentInChildren<CameraScript>();
        if(cameraScript != null)
        {
            cameraScript.SetCameraOnPlayerOverview();
        }
    }

    public void PlayerReset()
    {
        if(IsDrivingCart)
        {
            GetComponent<CartControls>()?.DetachShoppingCart();
        }

        HeldItemType.Value = Item.NO_ITEMTYPE_CODE;

        IsDrivingCart = false;

        ShoppingCart = null;
    }
}
