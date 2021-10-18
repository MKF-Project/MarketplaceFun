using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public const string SPAWN_BOUNDING_BOX_NAME = "BoundingBox";

    public PhysicMaterial BoundingBoxMaterial;

    private GameObject _boundingBox;
    private MeshFilter _boundingBoxMesh = null;
    private MeshRenderer _boundingBoxRenderer = null;

    private void Awake()
    {
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
}
