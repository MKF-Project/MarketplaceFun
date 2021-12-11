using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingListUI : MonoBehaviour
{

    private static ShoppingListUI _instance = null;

    //public List<Image> UIItemsList;
    public Dictionary<ulong, GameObject> UIItemsDictionary;

    [SerializeField]
    private Sprite StartingCheckedImage;
    public static Sprite CheckedImage { get; private set; }

    private void Awake()
    {
        if(_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        CheckedImage = StartingCheckedImage;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
        }
    }

    private void Start()
    {
        UIItemsDictionary = new Dictionary<ulong, GameObject>();
        //FillUIItems();
    }

    public static void FillUIItems(List<ShoppingListItem> itemList)
    {
        if (itemList.Count > _instance.transform.childCount)
        {
            return;
        }

        //Jump first
        int i = 1;

        foreach (ShoppingListItem shoppingListItem in itemList)
        {
            GameObject itemUI = _instance.transform.GetChild(i).gameObject;
            Image itemImageUI = itemUI.GetComponent<Image>();

            itemImageUI.sprite = NetworkItemManager.GetItemPrefabScript(shoppingListItem.ItemCode).UISticker;

            itemImageUI.enabled = true;
            _instance.UIItemsDictionary.Add(shoppingListItem.ItemCode, itemUI);

            i++;
        }
    }

    public static void EraseItems()
    {
        _instance.UIItemsDictionary = new Dictionary<ulong, GameObject>();
        for(int i = 0; i < _instance.transform.childCount; i++)
        {
            var childTransform = _instance.transform.GetChild(i);
            if(i == 0)
            {
                childTransform.gameObject.SetActive(false);
            }
            else
            {
                var itemUI = childTransform.GetComponent<Image>();
                itemUI.sprite = null;
                itemUI.enabled = false;

                var checkMark = itemUI.transform.GetChild(0).GetComponent<Image>();
                checkMark.sprite = null;
                checkMark.enabled = false;
            }
        }
    }

    public static void CheckItem(ulong itemCode)
    {
        GameObject itemUI = _instance.UIItemsDictionary[itemCode];
        Image imageCheckUI = itemUI.transform.GetChild(0).GetComponent<Image>();
        imageCheckUI.sprite = CheckedImage;
        imageCheckUI.enabled = true;
    }

    public static void UncheckItem(ulong itemCode)
    {
        GameObject itemUI = _instance.UIItemsDictionary[itemCode];
        Image imageCheckUI = itemUI.transform.GetChild(0).GetComponent<Image>();
        imageCheckUI.sprite = null;
        imageCheckUI.enabled = false;
    }
}
