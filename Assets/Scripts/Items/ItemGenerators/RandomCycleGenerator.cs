using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

// Picks a random Item from the list at the start of the game
// The random item chosen will NOT change after it has been defined.
public class RandomCycleGenerator : ItemGenerator
{
    private NetworkVariableInt _randomIndex = new NetworkVariableInt(
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        },
        0
    );

    public override ulong ItemInStock => ItemPool[_randomIndex.Value];
    public override bool IsDepleted => false;

    protected override void Awake()
    {
        base.Awake();

        if(IsServer)
        {
            _randomIndex.Value = Random.Range(0, ItemPool.Count);
        }

    }

    public override void NetworkStart()
    {
        base.NetworkStart();

        _randomIndex.OnValueChanged = OnItemShuffle;
    }

    public override void GiveItemToPlayer(Player player)
    {
        base.GiveItemToPlayer(player);

        player.HeldItemType.Value = ItemPool[_randomIndex.Value];
        UpdateRandomIndex_ServerRpc();
    }

    private void OnItemShuffle(int previous, int next)
    {
        InvokeOnRestocked(ItemPool[next]);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateRandomIndex_ServerRpc()
    {
        _randomIndex.Value = Random.Range(0, ItemPool.Count);
    }
}
