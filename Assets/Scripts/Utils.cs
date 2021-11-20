using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct RigidbodyTemplate
{
    public float mass;
    public float drag;
    public float angularDrag;
    public float maxAngularVelocity;
    public float maxDepenetrationVelocity;
    public bool detectCollisions;
    public bool useGravity;
    public bool isKinematic;
    public bool freezeRotation;
    public RigidbodyInterpolation interpolationMode;
    public CollisionDetectionMode collisionDetectionMode;
    public RigidbodyConstraints constraints;
}

public static class Utils
{
    // Coroutines Cache
    internal const float SHORT_WAIT_SECONDS = 1;
    internal const float LONG_WAIT_SECONDS = 5;

    internal static readonly WaitForEndOfFrame EndOfFrameWait = new WaitForEndOfFrame();
    internal static readonly WaitForFixedUpdate FixedUpdateWait = new WaitForFixedUpdate();
    internal static readonly WaitForSeconds ShortWait = new WaitForSeconds(SHORT_WAIT_SECONDS);
    internal static readonly WaitForSeconds LongWait = new WaitForSeconds(LONG_WAIT_SECONDS);

    /** --- EXTENSIONS --- **/
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

    public static List<GameObject> FindChildrenWithTag(this Transform root, string tag, bool includeInactive = true)
    {
        return new List<GameObject>(FindFromTag(root.gameObject, tag, includeInactive));
    }

    public static GameObject FindChildWithTag(this Transform root, string tag, bool includeInactive = true)
    {
        return FindFromTag(root.gameObject, tag, includeInactive).FirstOrDefault();
    }

    // Menu
    public static void toggleMenu(this MonoBehaviour menuScript) => MenuManager.toggleMenu(menuScript.gameObject);
    public static void toggleMenuDelayed(this MonoBehaviour menuScript) => MenuManager.toggleMenuDelayed(menuScript.gameObject);

    // Lists
    public static bool Unique<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
        // Evaluate Enumerator has no more than one element that satifies the predicate
        // without iterating over the entire list if more than one exists
        return list.Where(predicate).Take(2).Count() == 1;
    }

    public static bool None<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
        return !list.Any(predicate);
    }

    // Stack
    public static bool TryPeek<T>(this Stack<T> stack, out T result)
    {
        if(stack.Count > 0) {
            result = stack.Peek();
            return true;
        }

        result = default(T);
        return false;
    }

    public static bool TryPop<T>(this Stack<T> stack, out T result)
    {
        if(stack.Count > 0) {
            result = stack.Pop();
            return true;
        }

        result = default(T);
        return false;
    }

    // Queue
    public static bool TryPeek<T>(this Queue<T> queue, out T result)
    {
        if(queue.Count > 0) {
            result = queue.Peek();
            return true;
        }

        result = default(T);
        return false;
    }

    public static bool TryDequeue<T>(this Queue<T> queue, out T result)
    {
        if(queue.Count > 0) {
            result = queue.Dequeue();
            return true;
        }

        result = default(T);
        return false;
    }

    // DontDestroyOnLoad
    public static void EnsureObjectDontDestroy(this GameObject gameobject)
    {
        // DontDestroy Scene's build index is always -1
        const int DontDestroySceneBuildIndex = -1;
        if(gameobject.scene.buildIndex == DontDestroySceneBuildIndex)
        {
            // We don't need to do anything if the Object
            // is already on the DontDestroy scene
            return;
        }

        // We make sure that this GameObject will be place on the
        // DontDestroyOnLoad Scene by first placing it on the top
        // of the hierarchy tree. Unity gives Warnings when moving
        // child objects to this Scene
        gameobject.transform.SetParent(null);
        GameObject.DontDestroyOnLoad(gameobject);
    }

    // Destroy
    public static void DestroyAllChildren(this Transform rootObj)
    {
        while(rootObj.childCount > 0)
        {
            var child = rootObj.GetChild(0);
            child.SetParent(null);
            GameObject.Destroy(child.gameObject);
        }
    }

    public static void DestroyAllChildren(this GameObject rootObj) => rootObj.transform.DestroyAllChildren();

    // Rigidbody
    public static RigidbodyTemplate ExtractToTemplate(this Rigidbody rb)
    {
        RigidbodyTemplate res;
        res.mass                     = rb.mass;
        res.drag                     = rb.drag;
        res.angularDrag              = rb.angularDrag;
        res.maxAngularVelocity       = rb.maxAngularVelocity;
        res.maxDepenetrationVelocity = rb.maxDepenetrationVelocity;
        res.detectCollisions         = rb.detectCollisions;
        res.useGravity               = rb.useGravity;
        res.isKinematic              = rb.isKinematic;
        res.freezeRotation           = rb.freezeRotation;
        res.interpolationMode        = rb.interpolation;
        res.collisionDetectionMode   = rb.collisionDetectionMode;
        res.constraints              = rb.constraints;

        GameObject.Destroy(rb.GetComponent(rb.GetType()));

        return res;
    }

    public static Rigidbody ImportFromTemplate(this RigidbodyTemplate rigidbodyTemplate, GameObject target)
    {
        var rb = target.GetComponent<Rigidbody>();
        if(rb == null)
        {
            rb = target.AddComponent<Rigidbody>();
        }

        rb.mass                     = rigidbodyTemplate.mass;
        rb.drag                     = rigidbodyTemplate.drag;
        rb.angularDrag              = rigidbodyTemplate.angularDrag;
        rb.maxAngularVelocity       = rigidbodyTemplate.maxAngularVelocity;
        rb.maxDepenetrationVelocity = rigidbodyTemplate.maxDepenetrationVelocity;
        rb.detectCollisions         = rigidbodyTemplate.detectCollisions;
        rb.useGravity               = rigidbodyTemplate.useGravity;
        rb.isKinematic              = rigidbodyTemplate.isKinematic;
        rb.freezeRotation           = rigidbodyTemplate.freezeRotation;
        rb.interpolation            = rigidbodyTemplate.interpolationMode;
        rb.collisionDetectionMode   = rigidbodyTemplate.collisionDetectionMode;
        rb.constraints              = rigidbodyTemplate.constraints;

        return rb;
    }

    /** --- OTHER UTILS --- **/
}
