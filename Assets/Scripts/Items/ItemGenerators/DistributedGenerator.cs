using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public class DistributedGenerator : ItemGenerator
{
    private const int NO_SEED = int.MaxValue;

    [SerializeField, Tooltip("If selected, Shelves without groups will be considered part of the same group. Otherwise, they will each be part of a separate group")]
    private bool _groupSingleShelves = false;
    public bool GroupSingleShelves => _groupSingleShelves;

    private NetworkVariableInt _seed = new NetworkVariableInt
    (
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        },
        NO_SEED
    );

    private SortedDictionary<string, ulong> _groupItems = new SortedDictionary<string, ulong>();
    private Dictionary<string, List<Shelf>> _shelvesPerGroup = new Dictionary<string, List<Shelf>>();

    public override ulong RequestItemInStock(Shelf shelf) => _groupItems.TryGetValue(GetShelfGroup(shelf), out var itemID)? itemID : Item.NO_ITEMTYPE_CODE;
    public override bool RequestIsDepleted(Shelf shelf) => RequestItemInStock(shelf) == Item.NO_ITEMTYPE_CODE;

    protected override void Awake()
    {
        base.Awake();

        if(IsServer)
        {
            _seed.Value = Random.Range(int.MinValue, int.MaxValue);
        }
    }

    public override void RegisterShelf(Shelf shelf)
    {

        var group = GetShelfGroup(shelf);

        if(!_groupItems.ContainsKey(group))
        {
            _groupItems.Add(group, Item.NO_ITEMTYPE_CODE);
        }

        if(_shelvesPerGroup.TryGetValue(group, out var list))
        {
            list.Add(shelf);
        }

        else
        {
            _shelvesPerGroup.Add(group, new List<Shelf>());
            _shelvesPerGroup[group].Add(shelf);
        }

        base.RegisterShelf(shelf);
    }

    protected override void Start()
    {
        InitializeGeneratables();

        if(_seed.Value != NO_SEED)
        {
            DistributeGroupItems(_seed.Value);
        }

        // Defer Shelf set items until first seed update from server
        else
        {
            _seed.OnValueChanged = (int oldSeed, int newSeed) =>
            {
                if(oldSeed == NO_SEED)
                {
                    DistributeGroupItems(newSeed);
                }

                _seed.OnValueChanged = null;
            };
        }
    }

    private void DistributeGroupItems(int seed)
    {
        var rng = new System.Random(seed);
        var generatables = new HashSet<ulong>();

        var keys = _groupItems.Keys.ToList();
        for(int i = 0; i < keys.Count; i++)
        {
            if(ItemPool.Count > 0)
            {
                var randomIndex = rng.Next(ItemPool.Count);
                var itemID = ItemPool[randomIndex];
                ItemPool.RemoveAt(randomIndex);

                _groupItems[keys[i]] = itemID;
                generatables.Add(itemID);

                _shelvesPerGroup[keys[i]].ForEach(shelf => InvokeOnShelfRestocked(shelf, itemID));
            }
        }

        InvokeOnOwnGeneratablesDefined(generatables);
    }

    private string GetShelfGroup(Shelf shelf) => shelf.Group != Shelf.SHELF_NO_GROUP || GroupSingleShelves? shelf.Group : $"{shelf.NetworkObject.PrefabHashGenerator}-{shelf.NetworkObjectId}";
}
