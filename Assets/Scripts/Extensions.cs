using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions
{
    public static IEnumerable<GameObject> FindChildrenWithTag(this GameObject root, string tag, bool includeInactive = true)
    {
        return root.GetComponentsInChildren<Transform>(includeInactive).Where(child => child.tag == tag).Select(child => child.gameObject);
    }

    public static void toggleMenu(this MonoBehaviour menuScript) => MenuManager.toggleMenu(menuScript.gameObject);
    public static void toggleMenuDelayed(this MonoBehaviour menuScript) => MenuManager.toggleMenuDelayed(menuScript.gameObject);
}
