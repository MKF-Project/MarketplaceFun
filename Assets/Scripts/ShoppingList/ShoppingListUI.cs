using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingListUI : MonoBehaviour
{
    
    //public List<Image> UIItemsList;
    public Dictionary<int, GameObject> UIItemsDictionary;

    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        UIItemsDictionary = new Dictionary<int, GameObject>();
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
            itemImageUI.sprite = ItemTypeList.ItemList[shoppingListItem.ItemCode].Image;
            itemImageUI.enabled = true;
            UIItemsDictionary.Add(shoppingListItem.ItemCode, itemUI);
            i++;
        }
    }

    public void EraseItems()
    {
        UIItemsDictionary = new Dictionary<int, GameObject>();
        foreach (Image image in GetComponentsInChildren<Image>())
        {
            image.sprite = null;
            image.enabled = false;
        }
    }

    public void CheckItem(int itemCode)
    {
        GameObject itemUI = UIItemsDictionary[itemCode];
        Image imageCheckUI = itemUI.transform.GetChild(0).GetComponent<Image>();
        imageCheckUI.sprite = ItemTypeList.CheckedImage;
        imageCheckUI.enabled = true;
    }

    public void UncheckItem(int itemCode)
    {
        GameObject itemUI = UIItemsDictionary[itemCode];
        Image imageCheckUI = itemUI.transform.GetChild(0).GetComponent<Image>();
        imageCheckUI.sprite = null;
        imageCheckUI.enabled = false;
    }
}
