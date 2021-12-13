using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceCollision : MonoBehaviour
{
    private AudioSource _diceSource;
    private PointMarker _parentMarker = null;

    private void Awake()
    {
        TryGetComponent(out _diceSource);

        _parentMarker = GetComponentInParent<PointMarker>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if(_parentMarker != null)
        {
            _diceSource.PlayOneShot(_parentMarker.DiceHitSounds[Random.Range(0, _parentMarker.DiceHitSounds.Count)]);
        }
    }
}
