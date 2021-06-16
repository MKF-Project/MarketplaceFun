using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private const string menuTag = "Menu";
    private static List<GameObject> _menus;

    public GameObject startingMenu = null;

    private void Awake()
    {
        _menus = _menus ?? new List<GameObject>(gameObject.FindChildrenWithTag(menuTag));
        initializeMenus();
    }

    private void Start()
    {
        // Deactivates all menus except the starting one
        toggleMenu(startingMenu);
    }

    private void OnDestroy()
    {
        _menus = null;
    }

    public static void toggleMenu(GameObject activeMenu)
    {
        // If no menu was given, deactivate all of them
        if(activeMenu == null)
        {
            _menus.ForEach(menu => menu.SetActive(false));
            return;
        }

        if(activeMenu.tag != menuTag)
        {
            return;
        }

        _menus.ForEach(menu => menu.SetActive(menu == activeMenu));
    }

    private void initializeMenus()
    {
        // Activate all menus and trigger their Awake() functions
        _menus.ForEach(menu => menu.SetActive(true));
    }
}
