using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Sounds")]
    public List<AudioClip> Steps;

    private AudioSource _playerSource;
    private Player _playerScript;

    private void Awake()
    {
        TryGetComponent(out _playerScript);
        _playerSource = GetComponentInChildren<AudioSource>();
    }

    private void Start()
    {
        if(_playerScript.IsOwner)
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
