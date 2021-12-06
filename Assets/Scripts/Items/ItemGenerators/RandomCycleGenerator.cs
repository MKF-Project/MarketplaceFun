using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

// Every time a Player interacts with one of the shelves that use this generator,
// Select one a new item from it's list Randomly. May sometimes select the same item.
public class RandomCycleGenerator : ItemGenerator
{
    private NetworkVariableInt _randomIndex = new NetworkVariableInt
    (
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        },
        0
    );

    public override ulong RequestItemInStock(Shelf shelf) => ItemPool[_randomIndex.Value];
    public override bool RequestIsDepleted(Shelf shelf) => false;

    protected override void Awake()
    {
        base.Awake();

        if(IsServer)
        {
            _randomIndex.Value = Random.Range(0, ItemPool.Count);
            InvokeOnOwnGeneratablesDefined(ItemPool);
        }
    }

    public override void NetworkStart()
    {
        base.NetworkStart();

        _randomIndex.OnValueChanged = OnItemShuffle;
    }

    public override ulong TakeItem(Shelf shelf)
    {
        var itemTaken = base.TakeItem(shelf);
        if(itemTaken != Item.NO_ITEMTYPE_CODE)
        {
            UpdateRandomIndex_ServerRpc();
        }

        return itemTaken;
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
