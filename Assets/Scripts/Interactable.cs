using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Interactable : NetworkBehaviour
{
    public const string TAG_NAME = "Interactable";
    public const string LAYER_NAME = "Interact";
    public static readonly int LAYER_MASK = LayerMask.NameToLayer(LAYER_NAME);

    private Collider _interactionCollider = null;
    private bool _configured = false;

    private void Awake()
    {
        if(gameObject.tag != TAG_NAME || gameObject.layer != LAYER_MASK)
        {
            #if UNITY_EDITOR
                Debug.LogWarning($"[{gameObject}::Interactable]: Could not find suitable interactable gameobject. Tag: {gameObject.tag}, Layer: {LayerMask.LayerToName(gameObject.layer)}");
            #endif

            return;
        }

        _interactionCollider = GetComponent<Collider>();
        _configured = true;
    }

    public delegate void OnLookEnterDelegate(GameObject player);
    public event OnLookEnterDelegate OnLookEnter;
    public void TriggerLookEnter(GameObject player)
    {
        #if UNITY_EDITOR
            Debug.Log($"[{gameObject}::OnLookEnter]: {player}, Configured: ${_configured}");
        #endif

        if(_configured)
        {
            OnLookEnter?.Invoke(player);
        }
    }

    public delegate void OnLookExitDelegate(GameObject player);
    public event OnLookExitDelegate OnLookExit;
    public void TriggerLookExit(GameObject player)
    {
        #if UNITY_EDITOR
            Debug.Log($"[{gameObject}::OnLookExit]: {player}, Configured: ${_configured}");
        #endif

        if(_configured)
        {
            OnLookExit?.Invoke(player);
        }
    }

    public delegate void OnInteractDelegate(GameObject player);
    public event OnInteractDelegate OnInteract;
    public void TriggerInteract(GameObject player)
    {
        #if UNITY_EDITOR
            Debug.Log($"[{gameObject}::OnInteract]: {player}, Configured: ${_configured}");
        #endif

        if(_configured)
        {
            OnInteract?.Invoke(player);
        }
    }

}