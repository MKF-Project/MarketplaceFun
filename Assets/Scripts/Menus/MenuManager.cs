using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private const string menuTag = "Menu";
    private static List<GameObject> _menus;

    private void Awake()
    {
        _menus = _menus ?? new List<GameObject>(gameObject.FindChildrenWithTag(menuTag));
    }

    public static void toggleMenu(GameObject activeMenu)
    {
        if(activeMenu.tag != menuTag)
        {
            return;
        }

        _menus.ForEach(menu => menu.SetActive(menu == activeMenu));
    }
}
