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
}
