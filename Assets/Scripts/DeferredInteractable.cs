using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script let's us add an interactable collider to
// objects that don't have an Interactable script in an
// of themselves.
public class DeferredInteractable : Interactable
{
    public Interactable SourceInteractable = null;

    public override bool Configured => SourceInteractable != null? SourceInteractable.Configured : false;

    // We purposefully do nothing on awake, since
    // DeferredInteractable does not create UI or LayerMasks
    protected override void Awake()
    {

    }

    public override void TriggerLookEnter(Player player, Collider enteredCollider)
    {
        if(Configured && isActiveAndEnabled)
        {
            SourceInteractable.TriggerLookEnter(player, enteredCollider);
        }
    }

    public override void TriggerLookExit(Player player, Collider exitedCollider)
    {
        if(Configured && isActiveAndEnabled)
        {
            SourceInteractable.TriggerLookExit(player, exitedCollider);
        }
    }

    public override void TriggerInteract(Player player, Collider interactedCollider)
    {
        if(Configured && isActiveAndEnabled)
        {
            SourceInteractable.TriggerInteract(player, interactedCollider);
        }
    }
}
