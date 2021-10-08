using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Interactable : NetworkBehaviour
{
    public const string TAG_NAME = "Interactable";
    public const string LAYER_NAME = "Interact";
    public const string UI_NAME = "InteractCanvas";

    private static int _layerMask = -1;
    public static int LAYER_MASK
    {
        get => _layerMask;
    }


    private Collider _interactionCollider = null;
    public GameObject InteractUI { get; private set; }
    private bool _configured = false;

    private void Awake()
    {
        if(_layerMask == -1)
        {
            _layerMask = 1 << LayerMask.NameToLayer(LAYER_NAME);
        }

        if(gameObject.tag != TAG_NAME || 1 << gameObject.layer != LAYER_MASK)
        {
            
                Debug.LogError($"[{gameObject.name}::Interactable]: Could not find suitable interactable gameobject. Tag: {gameObject.tag}, Layer: {LayerMask.LayerToName(gameObject.layer)}");
            

            return;
        }

        InteractUI = gameObject.transform.Find(UI_NAME)?.gameObject;
        _interactionCollider = GetComponent<Collider>();
        _configured = true;
    }

    public delegate void OnLookEnterDelegate(GameObject player);
    public event OnLookEnterDelegate OnLookEnter;
    public void TriggerLookEnter(GameObject player)
    {
        if(_configured)
        {
            OnLookEnter?.Invoke(player);
        }
    }

    public delegate void OnLookExitDelegate(GameObject player);
    public event OnLookExitDelegate OnLookExit;
    public void TriggerLookExit(GameObject player)
    {
        if(_configured)
        {
            OnLookExit?.Invoke(player);
        }
    }

    public delegate void OnInteractDelegate(GameObject player);
    public event OnInteractDelegate OnInteract;
    public void TriggerInteract(GameObject player)
    {
        if(_configured)
        {
            OnInteract?.Invoke(player);
        }
    }

}