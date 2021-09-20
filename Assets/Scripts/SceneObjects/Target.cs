using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Target : MonoBehaviour
{
    public Material Material;
    public GameObject prefabPoint;

    public void OnCollisionEnter(Collision other)
    {
        Instantiate(prefabPoint, other.contacts[0].point,Quaternion.identity);

        Debug.Log("Colidiu");
        Material.color = Random.ColorHSV();
    }
}
