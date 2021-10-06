using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private static MenuManager _instance;

    private const string menuTag = "Menu";
    private static List<GameObject> _menus;

    public GameObject startingMenu = null;

    private void Awake()
    {
        _instance = _instance ?? this;
        _menus = _menus ?? gameObject.FindChildrenWithTag(menuTag);
        initializeMenus();
    }

    private void Start()
    {
        // Deactivates all menus except the starting one
        toggleMenu(startingMenu);
    }

    private void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
            _menus = null;
        }
    }

    private void initializeMenus()
    {
        // Activate all menus and trigger their Awake() functions
        _menus.ForEach(menu => menu.SetActive(true));
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

    public static void toggleMenuDelayed(GameObject activeMenu) => _instance.StartCoroutine(menuToggleCoroutine(activeMenu));

    private static IEnumerator menuToggleCoroutine(GameObject activeMenu)
    {
        yield return new WaitForEndOfFrame();
        toggleMenu(activeMenu);
    }
}
