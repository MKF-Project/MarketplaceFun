using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;

public class ShoppingCartAudio : NetworkBehaviour
{
    [Header("SFX")]
    public List<AudioClip> CartHitSounds;
    public float HitSoundCooldown = 0.1f;

    [Header("Configurations")]
    [SerializeField] private AudioSource _cartHitSource;
    [SerializeField] private AudioSource _cartMovementSource;

    [Min(0.1f)] public float MinimumSoundSpeed = 0.1f;
    [Min(0.1f)] public float MaximumSoundSpeed = 3.5f;

    [Range(-3f, 3f)] public float MinimumSpeedPitch = 0.8f;
    [Range(-3f, 3f)] public float MaximumSpeedPitch = 1f;

    private float _lastHitSound = 0;
    private bool _isCurrentPlayer;
    private Rigidbody _rigidbody;

    private NetworkVariableVector3 _netVelocity = new NetworkVariableVector3
    (
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.OwnerOnly
        },
        Vector3.zero
    );

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _isCurrentPlayer = false;
    }

    private void Start()
    {
        _lastHitSound = Time.time;
    }

    private void Update()
    {
        // Test if _rigidBod was not destroyed
        // if(_rigidbody.gameObject == null)
        // {
        //     return;
        // }

        Vector3 velocity;
        if(IsOwner)
        {
            _netVelocity.Value = _rigidbody.velocity;
            velocity = _rigidbody.velocity;
        }
        else
        {
            velocity = _netVelocity.Value;
        }

        // Get the cart's speed relative to it's forward axis
        var forwardSpeed = Mathf.Abs(Vector3.Dot(_rigidbody.transform.forward, velocity));

        if(forwardSpeed < MinimumSoundSpeed)
        {
            _cartMovementSource.volume = 0;
            _cartMovementSource.pitch = MinimumSpeedPitch;
        }
        else if(forwardSpeed > MaximumSoundSpeed)
        {
            _cartMovementSource.volume = 1;
            _cartMovementSource.pitch = MaximumSpeedPitch;
        }
        else
        {
            var calculatedVolume = (forwardSpeed - MinimumSpeedPitch) / (MaximumSoundSpeed - MinimumSoundSpeed);
            _cartMovementSource.volume = _isCurrentPlayer? 1 : calculatedVolume;
            _cartMovementSource.pitch = (MaximumSpeedPitch - MinimumSpeedPitch) * calculatedVolume + MinimumSpeedPitch;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(Time.time - _lastHitSound > HitSoundCooldown && CartHitSounds != null && CartHitSounds.Count > 0)
        {
            _cartHitSource.PlayOneShot(CartHitSounds[Random.Range(0, CartHitSounds.Count)]);
            _lastHitSound = Time.time;
        }
    }

    public void UpdateSoundTimer()
    {
        _lastHitSound = Time.time;
    }

    public void UseCartRigidbody(Rigidbody cartBody)
    {
        _rigidbody = cartBody;
        _isCurrentPlayer = false;
        _cartMovementSource.spatialBlend = 1;
    }

    public void UsePlayerRigidbody(Player player)
    {
        if(player.TryGetComponent(out _rigidbody))
        {
            _isCurrentPlayer = player.IsOwner;
            _cartMovementSource.spatialBlend = player.IsOwner? 0 : 1;
        }
    }
}
