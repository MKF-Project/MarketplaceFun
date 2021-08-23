using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingListUI : MonoBehaviour
{
    
    public List<Image> UIItemsList;

    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        UIItemsList = GetComponentsInChildren<Image>().ToList();
        //FillUIItems();
    }

    public void FillUIItems(List<ShoppingListItem> itemList)
    {
        int i = 0;
        foreach (ShoppingListItem shoppingListItem in itemList)
        {
            UIItemsList[i].sprite = ItemTypeList.ItemList[shoppingListItem.ItemCode].Image;
            UIItemsList[i].enabled = true;
            i++;
        }
    }

    public void EraseItems()
    {
        foreach (Image image in UIItemsList)
        {
            image.sprite = null;
            image.enabled = false;
        }
    }
}
