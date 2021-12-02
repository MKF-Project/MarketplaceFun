using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMarkerSpawner : MonoBehaviour
{
    public GameObject PointMarkerPrefab;

    private List<Vector3> _pointsPositions;

    private void Awake()
    {
        _pointsPositions = new List<Vector3>();
    }

    public void Spawn(int pointType, int pointValue)
    {
        GameObject spawnedMarker = Instantiate(PointMarkerPrefab, transform.position, Quaternion.identity);
        spawnedMarker.GetComponent<PointMarker>().SetPoints(pointType, pointValue);
    }
    
    public void Spawn(int pointType, int pointValue, Vector3 position)
    {
        GameObject spawnedMarker = Instantiate(PointMarkerPrefab, position, Quaternion.identity);
        spawnedMarker.GetComponent<PointMarker>().SetPoints(pointType, pointValue);
    }
}
