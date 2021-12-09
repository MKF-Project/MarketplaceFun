using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerAudio : NetworkBehaviour
{
    [Header("Sounds")]
    public List<AudioClip> Steps;

    private AudioSource _playerSource;

    private void Awake()
    {
        _playerSource = GetComponentInChildren<AudioSource>();
    }

    private void Start()
    {
        if(IsOwner)
        {
            _playerSource.spatialBlend = 0;
        }
    }

    // Call Sound Functions
    public void AnimationStepForward(AnimationEvent caller)
    {
        if(caller.animatorClipInfo.weight > 0.5f)
        {
            PlayStep();
        }
    }

    public void AnimationStepSideways(AnimationEvent caller)
    {
        if(caller.animatorClipInfo.weight >= 0.5f)
        {
            PlayStep();
        }
    }

    public void PlayStep() => _playerSource.PlayOneShot(Steps[Random.Range(0, Steps.Count)]);
}
