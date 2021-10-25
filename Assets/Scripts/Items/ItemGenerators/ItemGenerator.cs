using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;

public abstract class ItemGenerator: NetworkBehaviour
{
    [SerializeField]
    protected List<GameObject> _itemPool = null;
    protected NetworkObject _netObjectBuffer = null;
    public List<ulong> ItemPool { get; protected set; }

    public abstract bool IsDepleted { get; }
    public virtual bool IsStocked => !IsDepleted;

    public abstract ulong TakeItem();

    protected virtual void Awake()
    {
        ItemPool = new List<ulong>(_itemPool.Count);
        for(int i = 0; i < _itemPool.Count; i++)
        {
            if(!_itemPool[i].TryGetComponent<NetworkObject>(out _netObjectBuffer))
            {
                #if UNITY_EDITOR
                    Debug.LogWarning($"[{gameObject.name}/{nameof(ItemGenerator)}]: Prefab {_itemPool[i].name} is not an Item. Skipping...");
                #endif
                continue;
            }

            ItemPool.Add(_netObjectBuffer.PrefabHash);
        }
    }

}
