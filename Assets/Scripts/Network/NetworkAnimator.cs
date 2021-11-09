using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class NetworkAnimator : MLAPI.Prototyping.NetworkAnimator
{
    public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layer) => Animator.GetCurrentAnimatorStateInfo(layer);
    public AnimatorStateInfo GetNextAnimatorStateInfo(int layer) => Animator.GetNextAnimatorStateInfo(layer);

    /**
     * These methods are almost simply passed through for the sake of convenience.
     * Only difference is checking ownership of netObject before attempting to animate.
     *
     * If any more Animator methods are required, just add them here.
     */
    public void SetBool(string paramName, bool value) { if(IsOwner) Animator.SetBool(paramName, value); }
    public void SetBool(int paramHash,    bool value) { if(IsOwner) Animator.SetBool(paramHash, value); }

    public void SetFloat(string paramName, float value) { if(IsOwner) Animator.SetFloat(paramName, value); }
    public void SetFloat(int paramHash,    float value) { if(IsOwner) Animator.SetFloat(paramHash, value); }

    public void SetInteger(string paramName, int value) { if(IsOwner) Animator.SetInteger(paramName, value); }
    public void SetInteger(int paramHash,    int value) { if(IsOwner) Animator.SetInteger(paramHash, value); }

    // This is really the reason why this class exists at all,
    // as MLAPI's NetworkAnimator doesn't implement SetTrigger by default
    // so we are implementing it here
    public void SetTrigger(string triggerName) => SetTrigger(Animator.StringToHash(triggerName));
    public void SetTrigger(int triggerHash)
    {
        if(IsOwner)
        {
            Animator.SetTrigger(triggerHash);
            SetTrigger_ServerRpc(triggerHash);
        }
    }

    [ServerRpc]
    private void SetTrigger_ServerRpc(int triggerHash)
    {
        if(!IsOwner) // Owner was set by the function caller
        {
            Animator.SetTrigger(triggerHash);
        }
        SetTrigger_ClientRpc(triggerHash);
    }

    [ClientRpc]
    private void SetTrigger_ClientRpc(int triggerHash)
    {
        if(IsOwner || IsServer) // Both Owner and Server were set previously
        {
            return;
        }

        Animator.SetTrigger(triggerHash);
    }
}
