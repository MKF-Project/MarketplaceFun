using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

// Picks a random Item from the list at the start of the game
// The random item chosen will NOT change after it has been defined.
public class SameRandomGenerator : ItemGenerator
{
    // Intended to be used for debugging in the editor.
    // Does not affect final build
    #if UNITY_EDITOR
        [Header("Debug")]

        [Tooltip("Set a fixed index for this Generator. Intended for Debug ONLY. Does nothing on a final build")]
        [SerializeField]
        private int _debugIndex = -1;
    #endif

    private NetworkVariableInt _randomIndex = new NetworkVariableInt(
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        },
        0
    );

    public override bool IsDepleted => false;

    protected override void Awake()
    {
        base.Awake();

        if(IsServer)
        {
            #if UNITY_EDITOR
                if(_debugIndex >= 0 && _debugIndex < ItemPool.Count)
                {
                    _randomIndex.Value = _debugIndex;
                    return;
                }
            #endif

            var value = Random.Range(0, ItemPool.Count);

            _randomIndex.Value = value;
        }
    }

    public override ulong TakeItem()
    {
        return IsDepleted? Item.NO_ITEMTYPE_CODE : ItemPool[_randomIndex.Value];
    }
}
