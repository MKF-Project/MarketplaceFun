using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingListUI : MonoBehaviour
{

    //public List<Image> UIItemsList;
    public Dictionary<ulong, GameObject> UIItemsDictionary;

    private Item _itemScriptPlacement = null;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UIItemsDictionary = new Dictionary<ulong, GameObject>();
        //FillUIItems();
    }

    public void FillUIItems(List<ShoppingListItem> itemList)
    {
        if (itemList.Count > transform.childCount)
        {
            return;
        }

        int i = 0;

        foreach (ShoppingListItem shoppingListItem in itemList)
        {
            GameObject itemUI = transform.GetChild(i).gameObject;
            Image itemImageUI = itemUI.GetComponent<Image>();

            ItemTypeList.ItemList[shoppingListItem.ItemCode].ItemPrefab.TryGetComponent<Item>(out _itemScriptPlacement);
            itemImageUI.sprite = _itemScriptPlacement.UISticker;

            itemImageUI.enabled = true;
            UIItemsDictionary.Add(shoppingListItem.ItemCode, itemUI);

            i++;
        }
    }

    public void EraseItems()
    {
        UIItemsDictionary = new Dictionary<ulong, GameObject>();
        foreach (Image image in GetComponentsInChildren<Image>())
        {
            image.sprite = null;
            image.enabled = false;
        }
    }

    public void CheckItem(ulong itemCode)
    {
        GameObject itemUI = UIItemsDictionary[itemCode];
        Image imageCheckUI = itemUI.transform.GetChild(0).GetComponent<Image>();
        imageCheckUI.sprite = ItemTypeList.CheckedImage;
        imageCheckUI.enabled = true;
    }

    public void UncheckItem(ulong itemCode)
    {
        GameObject itemUI = UIItemsDictionary[itemCode];
        Image imageCheckUI = itemUI.transform.GetChild(0).GetComponent<Image>();
        imageCheckUI.sprite = null;
        imageCheckUI.enabled = false;
    }
}
