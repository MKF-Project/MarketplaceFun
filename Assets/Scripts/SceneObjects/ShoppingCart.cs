using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

[SelectionBase]
public class ShoppingCart : NetworkBehaviour
{
    private const string ITEM_POSITIONS_TAG = "Item";
    private const float COLLISION_COOLDOWN = 2;

    private int _nextIndex = 0;

    private int[] _itemCodes;
    private bool[] _occupiedPositions;
    private List<GameObject> _itemPositions;

    private float _lastCollision = 0;

    private void Awake()
    {
        _itemPositions = gameObject.FindChildrenWithTag(ITEM_POSITIONS_TAG);
        _itemCodes = new int[_itemPositions.Count];
        _occupiedPositions = new bool[_itemPositions.Count];
    }

    private void Start()
    {

    }

    private void Update()
    {
    }

    private void OnCollisionEnter(Collision other)
    {
        if(IsServer && other.gameObject.tag == "Item") {
            var itemScript = other.gameObject.GetComponent<Item>();
            if(itemScript == null || !itemScript.IsOnThrow)
            {
                return;
            }

            if(Time.unscaledTime - _lastCollision > COLLISION_COOLDOWN)
            {
                print("Set type");
                setNextItem_ClientRpc(itemScript.ItemTypeCode);
                setNextItem(itemScript.ItemTypeCode);
                _lastCollision = Time.unscaledTime;
            }
        }
    }

    private void setNextItem(int itemTypeCode)
    {
        // Destroy previous model
        while(_itemPositions[_nextIndex].transform.childCount > 0)
        {
            var child = _itemPositions[_nextIndex].transform.GetChild(0);

            child.parent = child.root;
            Destroy(child.gameObject);
        }


        _occupiedPositions[_nextIndex] = true;
        _itemCodes[_nextIndex] = itemTypeCode;

        var itemPrefab = ItemTypeList.ItemList[itemTypeCode].ItemPrefab;
        var meshObject = itemPrefab.transform.Find("Cube").gameObject;

        if(meshObject != null)
        {
            GameObject generatedItem = Instantiate(meshObject, Vector3.zero, Quaternion.identity, _itemPositions[_nextIndex].transform);

            generatedItem.transform.localPosition = Vector3.zero;
            generatedItem.transform.localRotation = Quaternion.identity;
            generatedItem.transform.localScale = meshObject.transform.localScale;
        }

        _nextIndex = (_nextIndex + 1) % _itemPositions.Count;
    }

    [ClientRpc]
    private void setNextItem_ClientRpc(int itemTypeCode)
    {
        if(!IsServer)
        {
            setNextItem(itemTypeCode);
        }
    }
}
