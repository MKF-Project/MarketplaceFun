﻿using System;
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
        return !object.Equals(list.SingleOrDefault(predicate), default(T));
    }

    public static bool None<T>(this IEnumerable<T> list, Func<T, bool> predicate)
    {
        return !list.Any(predicate);
    }

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