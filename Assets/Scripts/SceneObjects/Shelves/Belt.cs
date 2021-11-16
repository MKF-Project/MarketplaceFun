using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

[SelectionBase]
public class Belt : Shelf
{
    private struct PathSection
    {
        public GameObject start;
        public GameObject end;
        public float length;
    }

    private struct BeltItem
    {
        public GameObject item;
        public ulong visualItemID;
        public float pathCompletePercent;
    }

    private const string BELT_WAYPOINTS_CONTAINER = "Waypoints";
    private const string BELT_ITEM_TEMPLATE = "Belt_Item";
    private const int BELT_ITEM_INTERACT_INDEX = 0;
    private const int BELT_ITEM_VISUALS_INDEX = 1;

    [SerializeField, Min(0.1f)] private float _nextItemInterval = 2;
    [SerializeField, Min(0.1f)] private float _timeExposed = 1;

    public NetworkVariableFloat NextItemInterval { get; private set; } = new NetworkVariableFloat
    (
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        }, 0
    );

    public List<GameObject> _waypoints { get; private set; }
    private List<PathSection> _pathSections;
    private float _totalPathLength;

    private WaitForSeconds  _nextItemIntervalWait;
    private Coroutine _exposeNextItemCoroutine = null;

    private GameObject _beltItemBaseTemplate = null;
    private Stack<BeltItem> _beltItemPool = new Stack<BeltItem>();
    private List<BeltItem> _itemsInBelt = new List<BeltItem>();

    private Collider _interactColliderBuffer;

    protected override void Awake()
    {
        base.Awake();

        _waypoints = FindWaypoints();
        _pathSections = CreateWaypointPaths(_waypoints);
        _totalPathLength = _pathSections.Sum(section => section.length);

        NextItemInterval.OnValueChanged = IntervalChanged;

        _beltItemBaseTemplate = transform.Find(BELT_ITEM_TEMPLATE).gameObject;
        _beltItemBaseTemplate.SetActive(false);
    }

    protected override void OnDestroy()
    {
        StopCoroutine(_exposeNextItemCoroutine);
    }

    public override void NetworkStart()
    {
        base.NetworkStart();

        if(IsServer)
        {
            NextItemInterval.OnValueChanged = IntervalChanged;
            NextItemInterval.Value = _nextItemInterval;
            _exposeNextItemCoroutine = StartCoroutine(ExposeNextItem());
        }

    }

    private void Update()
    {
        // if(_pathSections.Count == 0)
        // {
        //     return;
        // }

        int i = 0;
        while(i < _itemsInBelt.Count)
        {
            var item = _itemsInBelt[i];
            item.pathCompletePercent = Mathf.Clamp01(item.pathCompletePercent + Time.deltaTime / _timeExposed);
            if(item.pathCompletePercent == 1)
            {
                _itemsInBelt.RemoveAt(i);
                ReturnItemToPool(item);

                // Don't update i if we removed an item from the list,
                // since the next item now exists in the same index
                continue;
            }

            var lengthTraveled = _totalPathLength * item.pathCompletePercent;
            int currSection = 0;
            var cumulativeSectionLength = _pathSections[currSection].length;
            while(lengthTraveled > cumulativeSectionLength)
            {
                currSection++;
                cumulativeSectionLength += _pathSections[currSection].length;
            }

            var travelInThisSection = lengthTraveled - cumulativeSectionLength + _pathSections[currSection].length;
            var travelPercentInThisSection = travelInThisSection / _pathSections[currSection].length;

            item.item.transform.position = Vector3.Lerp(_pathSections[currSection].start.transform.position, _pathSections[currSection].end.transform.position, travelPercentInThisSection);

            _itemsInBelt[i] = item;

            i++;
        }
    }

    protected override void InteractWithShelf(Player player, Collider interactedTrigger)
    {
        print(interactedTrigger);

        if(ItemGenerator != null && FindItemWithCollider(interactedTrigger, out var item) && item.visualItemID != Item.NO_ITEMTYPE_CODE)
        {
            // Give item to player
            ItemGenerator.GiveSpecificItemToPlayer(player, item.visualItemID);

            // Remove item from view
            _itemsInBelt.Remove(item);
            ReturnItemToPool(item);
            HideButtonPrompt(player, interactedTrigger);

            // Remove item ServerRPC

        }

    }

    private bool FindItemWithCollider(Collider collider, out BeltItem result)
    {
        for(int i = 0; i < _itemsInBelt.Count; i++)
        {
            if(_itemsInBelt[i].item.transform.GetChild(BELT_ITEM_INTERACT_INDEX).TryGetComponent(out _interactColliderBuffer) && _interactColliderBuffer == collider)
            {
                result = _itemsInBelt[i];
                return true;
            }
        }

        result.item = null;
        result.pathCompletePercent = 0;
        result.visualItemID = Item.NO_ITEMTYPE_CODE;
        return false;
    }

    protected override void RestockItem(ulong itemID)
    {

    }

    protected override void ClearShelf()
    {

    }

    private IEnumerator ExposeNextItem()
    {
        if(!IsServer)
        {
            yield break;
        }

        while(true)
        {
            yield return _nextItemIntervalWait;
            if(ItemGenerator != null && ItemGenerator.IsStocked)
            {
                SendItemOnBelt(ItemGenerator.TakeItem());
                // SendItemOnBelt_ClientRpc
            }
        }
    }

    private void SendItemOnBelt(ulong itemID)
    {
        if(itemID == Item.NO_ITEMTYPE_CODE)
        {
            return;
        }

        print($"[{gameObject.name}]: Moving Item {itemID} in belt");

        // Get belt item from pool
        BeltItem itemTemplate;
        if(!_beltItemPool.TryPop(out itemTemplate))
        {
            var itemObject = Instantiate(_beltItemBaseTemplate, transform);

            itemTemplate.item = itemObject;
            itemTemplate.visualItemID = Item.NO_ITEMTYPE_CODE;
        }

        PlaceVisualsOnBeltItem(ref itemTemplate, itemID);

        // Put item in belt queue
        itemTemplate.pathCompletePercent = 0;
        itemTemplate.item.SetActive(true);

        _itemsInBelt.Add(itemTemplate);
    }

    private void PlaceVisualsOnBeltItem(ref BeltItem beltItem, ulong itemID)
    {
        var itemVisuals = beltItem.item.transform.GetChild(BELT_ITEM_VISUALS_INDEX);
        if(beltItem.visualItemID != itemID)
        {
            itemVisuals.DestroyAllChildren();
        }

        var visuals = NetworkItemManager.GetItemPrefabVisuals(itemID);
        if(visuals != null)
        {
            var generatedItem = Instantiate(visuals.gameObject, Vector3.zero, Quaternion.identity, itemVisuals);
            generatedItem.transform.localPosition = Vector3.zero;
            generatedItem.transform.localRotation = Quaternion.identity;

        }

        beltItem.visualItemID = itemID;
    }

    private void ReturnItemToPool(BeltItem item)
    {
        _beltItemPool.Push(item);
        item.item.SetActive(false);
    }

    private void IntervalChanged(float before, float after)
    {
        _nextItemIntervalWait = new WaitForSeconds(after);
    }

    private List<GameObject> FindWaypoints()
    {
        var container = transform.Find(BELT_WAYPOINTS_CONTAINER);

        List<GameObject> res = new List<GameObject>(container.childCount);
        for(int i = 0; i < container.childCount; i++)
        {
            res.Add(container.GetChild(i).gameObject);
        }

        return res;
    }

    private List<PathSection> CreateWaypointPaths(List<GameObject> waypoints)
    {
        List<PathSection> res = new List<PathSection>(waypoints.Count - 1);
        for(int i = 0; i < waypoints.Count - 1; i++)
        {
            PathSection next;
            next.start = waypoints[i];
            next.end = waypoints[i + 1];
            next.length = (next.end.transform.position - next.start.transform.position).magnitude;

            res.Add(next);
        }

        return res;
    }

    // Editor Utils
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.yellow;

        var sections = CreateWaypointPaths(FindWaypoints());
        sections.ForEach(sect => Gizmos.DrawLine(sect.start.transform.position, sect.end.transform.position));
    }
}
