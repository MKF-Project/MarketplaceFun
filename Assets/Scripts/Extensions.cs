using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions
{
    // TAGs
    private static IEnumerable<GameObject> FindFromTag(GameObject root, string tag, bool includeInactive)
    {
        return root.GetComponentsInChildren<Transform>(includeInactive).Where(child => child.tag == tag).Select(child => child.gameObject);
    }

    public static List<GameObject> FindChildrenWithTag(this GameObject root, string tag, bool includeInactive = true)
    {
        return new List<GameObject>(FindFromTag(root, tag, includeInactive));
    }

    public static GameObject FindChildWithTag(this GameObject root, string tag, bool includeInactive = true)
    {
        return FindFromTag(root, tag, includeInactive).FirstOrDefault();
    }

    // Menu
    public static void toggleMenu(this MonoBehaviour menuScript) => MenuManager.toggleMenu(menuScript.gameObject);
    public static void toggleMenuDelayed(this MonoBehaviour menuScript) => MenuManager.toggleMenuDelayed(menuScript.gameObject);

    // Lists
    public static bool Unique<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
        return list.Count(predicate) == 1;
    }
}
