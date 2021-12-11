using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerAudio : NetworkBehaviour
{
    private const float MUTE_TIMEOUT = 0.5f;

    // TODO: find a better solution for this. Currently, we detect diagonal walk with
    // item in hand by initializing the float with a VERY specfic byte sequence
    private readonly float WALK_ITEM_WEIGHT = BitConverter.ToSingle(new byte[] {0xDF, 0xD3, 0xE1, 0x3E}, 0);
    private const float WALK_WEIGHT = 0.5f;

    [Header("Sounds")]
    public List<AudioClip> Steps;

    private AudioSource _playerSource;
    private WaitForSeconds _muteTimeout = new WaitForSeconds(MUTE_TIMEOUT);

    private void Awake()
    {
        _playerSource = GetComponentInChildren<AudioSource>();

        StartCoroutine(UnmuteAfterSeconds());

        SceneManager.OnSceneLoaded += StartMuted;
    }

    private void OnDestroy()
    {
        SceneManager.OnSceneLoaded -= StartMuted;
    }

    private void Start()
    {
        if(IsOwner)
        {
            _playerSource.spatialBlend = 0;
        }
    }

    private void StartMuted(string scene) => StartCoroutine(UnmuteAfterSeconds());

    private IEnumerator UnmuteAfterSeconds()
    {
        _playerSource.mute = true;

        yield return _muteTimeout;

        _playerSource.mute = false;
    }

    // Call Sound Functions
    public void AnimationStepForward(AnimationEvent caller) => AnimationStep(caller, WALK_WEIGHT, false);
    public void AnimationStepSideways(AnimationEvent caller) => AnimationStep(caller, WALK_WEIGHT, true);

    public void ItemAnimationStepForward(AnimationEvent caller) => AnimationStep(caller, WALK_ITEM_WEIGHT, false);
    public void ItemAnimationStepSideways(AnimationEvent caller) => AnimationStep(caller, WALK_ITEM_WEIGHT, true);

    private void AnimationStep(AnimationEvent caller, float threshold, bool considerEqual)
    {
        var thresholdBytes = BitConverter.GetBytes((Single) threshold);
        var weightBytes = BitConverter.GetBytes((Single) caller.animatorClipInfo.weight);

        if(caller.animatorClipInfo.weight > threshold || (considerEqual && caller.animatorClipInfo.weight == threshold))
        {
            PlayStep();
        }
    }

    public void PlayStep() => _playerSource.PlayOneShot(Steps[UnityEngine.Random.Range(0, Steps.Count)]);
}
