using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
    private const string BELT_ITEM_DISPLAY_PATH = "Canvas/ItemDisplay";

    private const string START_CURTAIN_TRIGGER = "StartCurtainFlap";
    private const string END_CURTAIN_TRIGGER = "EndCurtainFlap";

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
    private Image _itemDisplay = null;
    private Animator _animator = null;
    private Stack<BeltItem> _beltItemPool = new Stack<BeltItem>();
    private List<BeltItem> _itemsInBelt = new List<BeltItem>();

    [Range(0, 1)] public float StartCurtainPathPercent = 0;
    [Range(0, 1)] public float EndCurtainPathPercent = 1;

    private Collider _interactColliderBuffer;

    protected override void Awake()
    {
        base.Awake();

        _waypoints = FindWaypoints();
        _pathSections = CreateWaypointSections(_waypoints);

        // We use a simple for loop here, instead of _pathSection.Sum()
        // because sum casts floats to doubles (then back to float at the end),
        // we don't want that.
        _totalPathLength = 0;
        for(int i = 0; i < _pathSections.Count; i++)
        {
            _totalPathLength += _pathSections[i].length;
        }

        NextItemInterval.OnValueChanged = IntervalChanged;

        _beltItemBaseTemplate = transform.Find(BELT_ITEM_TEMPLATE).gameObject;
        _beltItemBaseTemplate.SetActive(false);

        transform.Find(BELT_ITEM_DISPLAY_PATH).TryGetComponent(out _itemDisplay);
        _itemDisplay.color = Color.clear;
        _itemDisplay.preserveAspect = true;

        TryGetComponent(out _animator);
    }

    protected override void OnDestroy()
    {
        StopCoroutine(_exposeNextItemCoroutine);

        ItemGenerator.OnDepleted -= HideItemDisplay;
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

        if(ItemGenerator != null)
        {
            ItemGenerator.OnDepleted += HideItemDisplay;
            SetNextItemDisplay(ItemGenerator.ItemInStock);
        }
    }

    private void Update()
    {
        int i = 0;
        while(i < _itemsInBelt.Count)
        {
            var item = _itemsInBelt[i];
            var previousComplete = item.pathCompletePercent;

            item.pathCompletePercent = Mathf.Clamp01(item.pathCompletePercent + Time.deltaTime / _timeExposed);

            // Item has just passed Start curtain
            if(previousComplete < StartCurtainPathPercent && item.pathCompletePercent >= StartCurtainPathPercent)
            {
                _animator.SetTrigger(START_CURTAIN_TRIGGER);
                if(_displayUpdateRequested)
                {
                    _displayUpdateRequested = false;
                    SetNextItemDisplay(ItemGenerator.ItemInStock);
                }
            }

            // Item has just passed End curtain
            if(previousComplete < EndCurtainPathPercent && item.pathCompletePercent >= EndCurtainPathPercent)
            {
                _animator.SetTrigger(END_CURTAIN_TRIGGER);
            }

            if(item.pathCompletePercent == 1)
            {
                RemoveItemFromBelt(i);

                // Don't update i if we removed an item from the list,
                // since the next item now exists in the same index
                continue;
            }

            if(GetBeltPositionAtPercent(item.pathCompletePercent, out var newPosition))
            {
                item.item.transform.position = newPosition;
                _itemsInBelt[i] = item;
            }

            i++;
        }
    }

    private bool GetBeltPositionAtPercent(float pathAmount, out Vector3 worldPosition)
    {
        if(_pathSections.Count == 0 || pathAmount < 0 || pathAmount > 1)
        {
            worldPosition = Vector3.zero;
            return false;
        }

        var lengthTraveled = _totalPathLength * pathAmount;

        // Find out which section this percentage corresponds to
        int currSection = 0;
        var cumulativeSectionLength = _pathSections[currSection].length;
        while(lengthTraveled > cumulativeSectionLength)
        {
            currSection++;
            cumulativeSectionLength += _pathSections[currSection].length;
        }

        var travelInThisSection = lengthTraveled - cumulativeSectionLength + _pathSections[currSection].length;
        var travelPercentInThisSection = travelInThisSection / _pathSections[currSection].length;

        worldPosition = Vector3.Lerp(_pathSections[currSection].start.transform.position, _pathSections[currSection].end.transform.position, travelPercentInThisSection);
        return true;
    }

    protected override void InteractWithShelf(Player player, Collider interactedTrigger)
    {
        if(ItemGenerator != null && FindItemWithCollider(interactedTrigger, out var item) && item.visualItemID != Item.NO_ITEMTYPE_CODE)
        {
            // Give item to player
            ItemGenerator.GiveSpecificItemToPlayer(player, item.visualItemID);

            // Remove item from view
            var itemIndex = _itemsInBelt.IndexOf(item);

            RemoveItemFromBelt(itemIndex);
            RemoveItemFromBelt_ServerRpc(itemIndex);

            HideButtonPrompt(player, interactedTrigger);
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

    // Deliberately turned into no-ops
    protected override void RestockItem(ulong itemID) {}
    protected override void ClearShelf() {}

    // The server controls the rate of belt item spawns
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
                var itemTaken = ItemGenerator.TakeItem();
                SendItemOnBelt(itemTaken);
                SendItemOnBelt_ClientRpc(itemTaken);
            }
        }
    }

    private void SendItemOnBelt(ulong itemID)
    {
        HideItemDisplay();
        _displayUpdateRequested = true;

        if(itemID == Item.NO_ITEMTYPE_CODE)
        {
            return;
        }

        // Get belt item from pool
        BeltItem itemTemplate;
        if(!_beltItemPool.TryPop(out itemTemplate))
        {
            var itemObject = Instantiate(_beltItemBaseTemplate, transform);

            itemTemplate.item = itemObject;
            itemTemplate.visualItemID = Item.NO_ITEMTYPE_CODE;
        }

        // Replace visuals only if pooled item was different
        if(itemTemplate.visualItemID != itemID)
        {
            itemTemplate.visualItemID = itemID;
            PlaceVisualsOnBeltItem(itemTemplate);
        }

        // Put item in belt queue
        itemTemplate.pathCompletePercent = 0;
        itemTemplate.item.SetActive(true);

        _itemsInBelt.Add(itemTemplate);
    }

    private void PlaceVisualsOnBeltItem(BeltItem beltItem)
    {
        var itemVisuals = beltItem.item.transform.GetChild(BELT_ITEM_VISUALS_INDEX);
        itemVisuals.DestroyAllChildren();

        var visuals = NetworkItemManager.GetItemPrefabVisuals(beltItem.visualItemID);
        if(visuals != null)
        {
            var generatedItem = Instantiate(visuals.gameObject, Vector3.zero, Quaternion.identity, itemVisuals);
            generatedItem.transform.localPosition = Vector3.zero;
            generatedItem.transform.localRotation = Quaternion.identity;
        }
    }

    private void RemoveItemFromBelt(int itemIndex)
    {
        var item = _itemsInBelt[itemIndex];
        _itemsInBelt.RemoveAt(itemIndex);
        ReturnItemToPool(item);
    }

    private void ReturnItemToPool(BeltItem item)
    {
        _beltItemPool.Push(item);
        item.item.SetActive(false);
    }

    // Display
    private bool _displayUpdateRequested = false;

    private void SetNextItemDisplay(ulong item)
    {
        if(item == Item.NO_ITEMTYPE_CODE)
        {
            HideItemDisplay();
            return;
        }

        _itemDisplay.sprite = NetworkItemManager.GetItemPrefabScript(item).UISticker;
        _itemDisplay.color = Color.white;
    }

    private void HideItemDisplay()
    {
        _itemDisplay.color = Color.clear;
    }

    // Waypoints
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

    private List<PathSection> CreateWaypointSections(List<GameObject> waypoints)
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

    private void IntervalChanged(float before, float after)
    {
        _nextItemIntervalWait = new WaitForSeconds(after);
    }

    // RPCs
    [ClientRpc]
    private void SendItemOnBelt_ClientRpc(ulong itemID)
    {
        if(IsServer)
        {
            return;
        }

        SendItemOnBelt(itemID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveItemFromBelt_ServerRpc(int itemIndex, ServerRpcParams rpcReceiveParams = default)
    {
        if(rpcReceiveParams.Receive.SenderClientId != NetworkController.ServerID)
        {
            RemoveItemFromBelt(itemIndex);
        }

        RemoveItemFromBelt_ClientRpc(itemIndex, rpcReceiveParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void RemoveItemFromBelt_ClientRpc(int itemIndex, ulong clientInitiatorID)
    {
        if(NetworkController.SelfID == clientInitiatorID || IsServer)
        {
            return;
        }

        RemoveItemFromBelt(itemIndex);
    }

    // Editor Utils
    protected override void OnDrawGizmosSelected()
    {
        const float ENDPOINT_MARKER_SIZE = 0.2f;
        const float CURTAIN_MARKER_SIZE = 0.1f;

        base.OnDrawGizmosSelected();

        _waypoints = FindWaypoints();
        _pathSections = CreateWaypointSections(_waypoints);

        _totalPathLength = 0;
        for(int i = 0; i < _pathSections.Count; i++)
        {
            _totalPathLength += _pathSections[i].length;
        }

        if(_pathSections.Count > 0)
        {
            // Draw Start and End markers
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_pathSections[0].start.transform.position, ENDPOINT_MARKER_SIZE);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_pathSections[_pathSections.Count-1].end.transform.position, ENDPOINT_MARKER_SIZE);

            // Draw connecting line
            Gizmos.color = Color.yellow;
            _pathSections.ForEach(sect => Gizmos.DrawLine(sect.start.transform.position, sect.end.transform.position));
        }

        Vector3 markerPosition;
        if(GetBeltPositionAtPercent(StartCurtainPathPercent, out markerPosition))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(markerPosition, CURTAIN_MARKER_SIZE);
        }

        if(GetBeltPositionAtPercent(EndCurtainPathPercent, out markerPosition))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(markerPosition, CURTAIN_MARKER_SIZE);
        }
    }
}
