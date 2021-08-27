using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    protected const string TAG_NAME = "Interactable";
    protected const string LAYER_NAME = "Interact";
    protected readonly int LAYER_MASK = LayerMask.NameToLayer(LAYER_NAME);

    protected Collider _interactionCollider = null;

    protected virtual void Awake()
    {
        if(gameObject.tag != TAG_NAME || gameObject.layer != LAYER_MASK)
        {
            return;
        }

        _interactionCollider = GetComponent<Collider>();
    }

    public virtual void OnLookEnter(GameObject player)
    {

    }

    public virtual void OnLookExit(GameObject player)
    {

    }

    public virtual void OnInteract(GameObject player)
    {

    }

}
