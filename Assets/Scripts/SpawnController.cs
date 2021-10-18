using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;

public class SpawnController : NetworkBehaviour
{
    // Spawn logic
    public static int PlayerSpawnsRequired = 1;
    private HashSet<ulong> _playerInSpawn = new HashSet<ulong>();

    private static SpawnController _levelSpawnInstance = null;

    [SerializeField]
    private Collider _playerPrefabCollider = null;

    // Collider vars
    public const string SPAWN_BOUNDING_BOX_NAME = "BoundingBox";

    public PhysicMaterial BoundingBoxMaterial;

    private GameObject _boundingBox;
    private MeshFilter _boundingBoxMesh = null;
    private MeshRenderer _boundingBoxRenderer = null;
    private MeshCollider _generatedCollider;

    // Release
    private const string RELEASE_COUNTDOWN_NAME = "ReleaseCountdown";
    private const string RELEASE_COUNTDOWN_MESSAGE = "Go!";

    private const float COUNTDOWN_MESSAGE_HOLD_SECONDS = 1.5f;

    private const int COUNTDOWN_SECONDS = 3;

    private Text _releaseCountdown;

    private static readonly WaitForSeconds OneSecondWait = new WaitForSeconds(1);
    private static readonly WaitForSeconds HoldMessageWait = new WaitForSeconds(COUNTDOWN_MESSAGE_HOLD_SECONDS);

    private void Awake()
    {
        /* Spawn Logic */
        if(_levelSpawnInstance != null)
        {
            #if UNITY_EDITOR
                Debug.LogError($"[{gameObject.name}]: More than one SpawnArea in the level! This SpawnArea will be disabled");
            #endif

            Destroy(gameObject);
            return;
        }
        _levelSpawnInstance = this;

        /* Generate Collider */
        _boundingBox = transform.Find(SPAWN_BOUNDING_BOX_NAME).gameObject;

        _boundingBox.TryGetComponent(out _boundingBoxMesh);
        _boundingBox.TryGetComponent(out _boundingBoxRenderer);

        // Create inside-out mesh
        var invertedMesh = _boundingBoxMesh.mesh;
        invertedMesh.triangles = invertedMesh.triangles.Reverse().ToArray();

        // Generate dynamic collider from reversed mesh
        _generatedCollider = _boundingBox.AddComponent<MeshCollider>();
        _generatedCollider.sharedMesh = invertedMesh;
        _generatedCollider.material = BoundingBoxMaterial;

        // Disable editor bounding box rendering
        _boundingBoxRenderer.enabled = false;

        /* Release */
        transform.Find(RELEASE_COUNTDOWN_NAME).TryGetComponent(out _releaseCountdown);
    }

    private void OnDestroy()
    {
        if(_levelSpawnInstance == this)
        {
            _levelSpawnInstance = null;
        }
    }

    public override void NetworkStart()
    {
        // Since this procedure will be run when the level loads,
        // we can be assured that a new player has finished loading their local
        // version of the level
        if(IsClient)
        {
            NetworkController.SelfPlayer?.Teleport(RandomPointInSpawnFloor());

            PlayerInSpawn_ServerRpc();
        }
    }

    private Vector3 RandomPointInSpawnFloor()
    {
        // We add some padding to the RandomPoint to make sure the Player
        // won't spawn halfway inside the wall
        return new Vector3
        (
            Random.Range(_generatedCollider.bounds.min.x + _playerPrefabCollider.bounds.extents.x, _generatedCollider.bounds.max.x - _playerPrefabCollider.bounds.extents.x),
            0,
            Random.Range(_generatedCollider.bounds.min.z + _playerPrefabCollider.bounds.extents.z, _generatedCollider.bounds.max.z - _playerPrefabCollider.bounds.extents.z)
        );
    }

    private IEnumerator PerformCountdown(int countdownSeconds)
    {
        _releaseCountdown.gameObject.SetActive(true);

        for(int i = 1; i <= COUNTDOWN_SECONDS; i++)
        {
            _releaseCountdown.text = i.ToString();
            yield return OneSecondWait;
        }

        _releaseCountdown.text = RELEASE_COUNTDOWN_MESSAGE;
        yield return HoldMessageWait;

        _releaseCountdown.gameObject.SetActive(false);
        // TODO disable collider
        // TODO Fire spawn release event
    }

    /** ----- RPCs ----- **/
    [ServerRpc(RequireOwnership = false)]
    private void PlayerInSpawn_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    {
        if(!_playerInSpawn.Contains(rpcReceiveParams.Receive.SenderClientId))
        {
            print($"Added player {rpcReceiveParams.Receive.SenderClientId} to spawn");
            _playerInSpawn.Add(rpcReceiveParams.Receive.SenderClientId);

            // Check for game start
            if(_playerInSpawn.Count >= PlayerSpawnsRequired)
            {
                print("RELEASE THE PLAYERS!");
                ReleasePlayers_ClientRpc();
            }
        }
        #if UNITY_EDITOR
            else
            {
                Debug.LogWarning($"[{gameObject.name}]: Tried to move player {rpcReceiveParams.Receive.SenderClientId} to SpawnArea multiple times!");
            }
        #endif
    }

    [ClientRpc]
    private void ReleasePlayers_ClientRpc()
    {
        StartCoroutine(PerformCountdown(COUNTDOWN_SECONDS));
    }
}
