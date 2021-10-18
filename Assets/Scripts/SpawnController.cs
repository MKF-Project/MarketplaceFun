using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class SpawnController : NetworkBehaviour
{
    public const string SPAWN_BOUNDING_BOX_NAME = "BoundingBox";

    // Spawn logic
    public static int PlayerSpawnsRequired = 1;
    private HashSet<ulong> _playerInSpawn = new HashSet<ulong>();

    private static SpawnController _levelSpawnInstance = null;

    // Collider vars
    public PhysicMaterial BoundingBoxMaterial;

    private GameObject _boundingBox;
    private MeshFilter _boundingBoxMesh = null;
    private MeshRenderer _boundingBoxRenderer = null;

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
        var generatedColiider = _boundingBox.AddComponent<MeshCollider>();
        generatedColiider.sharedMesh = invertedMesh;
        generatedColiider.material = BoundingBoxMaterial;

        // Disable editor bounding box rendering
        _boundingBoxRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        if(_levelSpawnInstance == this)
        {
            _levelSpawnInstance = null;
        }
    }

    private void Start()
    {
        // Since this procedure will be run when the level loads,
        // we can be assured that a new player has finished loading their local
        // version of the level
        if(IsClient)
        {
            // TODO add randomness to spawn location, keeping it inside bbox
            NetworkController.SelfPlayer?.Teleport(transform.position);

            PlayerInSpawn_ServerRpc();
        }
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
            }
        }
        #if UNITY_EDITOR
            else
            {
                Debug.LogWarning($"[{gameObject.name}]: Tried to move player {rpcReceiveParams.Receive.SenderClientId} to SpawnArea multiple times!");
            }
        #endif
    }
}
