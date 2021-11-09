using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

/** RandomDistributedStartGenerator
 *
 * Distributes the items on the list between each one of the shelves interacting
 * with this generator. Each shelf will get a different item.
 * If there are more shelves than items, some shelves will remain empty.
 * If there are more items than shelves, some items will not appear on any of the shelves.
 * The items chosen for each shelf will NOT change during the duration of the match.
 */
public class RandomDistributedStartGenerator : ItemGenerator
{
    private class InternalDistributedItemGenerator: ItemGenerator
    {
        private ulong _stockedItem = Item.NO_ITEMTYPE_CODE;
        private RandomDistributedStartGenerator _originalGenerator;

        public override bool IsDepleted => false;
        public override ulong ItemInStock => _stockedItem;

        internal void InitializeInternalGenerator(ulong stockedItem, RandomDistributedStartGenerator originalGenerator)
        {
            _originalGenerator = originalGenerator;

            _stockedItem = stockedItem;
            ItemPool = new List<ulong>(1);
            ItemPool.Add(stockedItem);
        }

        protected override void Awake()
        {
            // We skip the default ItemGenerator.Awake(),
            // since we will set a custom ItemPool in the method above
        }

        public override void UnregisterShelf(Shelf shelf)
        {
            base.UnregisterShelf(shelf);
            if(IsServer)
            {
                _originalGenerator.ReassignItemToPool(_stockedItem);
            }
            Destroy(this);
        }
    }

    private List<Shelf> _shelvesInWaitList = new List<Shelf>();

    // This generator doesn't assign items directly, instead it
    // assigns other private generators for each of the shelves
    public override bool IsDepleted => true;
    public override ulong ItemInStock => Item.NO_ITEMTYPE_CODE;

    // protected override void Start()
    // {
    //     if(!IsServer)
    //     {
    //         return;
    //     }

    //     while(_shelvesInWaitList.Count > 0 && ItemPool.Count > 0)
    //     {
    //         var rndIndex = Random.Range(0, _shelvesInWaitList.Count);

    //         // TODO: RPC Assign on Clients
    //         AssignInternalGenerator(ItemPool[0], _shelvesInWaitList[rndIndex]);

    //         ItemPool.RemoveAt(0);
    //         _shelvesInWaitList.RemoveAt(rndIndex);

    //     }
    // }

    public override void RegisterShelf(Shelf shelf)
    {
        if(!IsServer)
        {
            return;
        }

        if(ItemPool.Count > 0)
        {
            var rndIndex = Random.Range(0, ItemPool.Count);
            var randomItem = ItemPool[rndIndex];
            ItemPool.RemoveAt(rndIndex);

            // TODO: RPC Assign on Clients
            AssignInternalGenerator(randomItem, shelf);
        }
        else
        {
            #if UNITY_EDITOR
                Debug.Log($"[{gameObject.name}/{nameof(RandomDistributedStartGenerator)}]: Shelf '{shelf.name}' added to waiting list");
            #endif
            _shelvesInWaitList.Add(shelf);
        }
    }

    public override void UnregisterShelf(Shelf shelf)
    {
        if(!IsServer)
        {
            return;
        }

        _shelvesInWaitList.Remove(shelf);
    }

    // Re-add the item released by one of the previous shelves back into the ItemPool
    // so that other pending shelves may claim it
    private void ReassignItemToPool(ulong item)
    {
        if(item == Item.NO_ITEMTYPE_CODE)
        {
            return;
        }

        if(_shelvesInWaitList.Count > 0)
        {
            // If we have shelves waiting for an item, give it to one of them at random
            var rndIndex = Random.Range(0, _shelvesInWaitList.Count);
            var nextShelf = _shelvesInWaitList[rndIndex];
            _shelvesInWaitList.RemoveAt(rndIndex);

            // TODO: RPC Assign on Clients
            AssignInternalGenerator(item, nextShelf);
        }

        // If no shelves are waiting, just place the item back into the pool
        else
        {
            ItemPool.Add(item);
        }
    }

    private void AssignInternalGenerator(ulong stockedItem, Shelf targetShelf)
    {
        var customGenerator = gameObject.AddComponent<InternalDistributedItemGenerator>();

        customGenerator.InitializeInternalGenerator(stockedItem, this);
        targetShelf.ItemGenerator = customGenerator;
    }
}
