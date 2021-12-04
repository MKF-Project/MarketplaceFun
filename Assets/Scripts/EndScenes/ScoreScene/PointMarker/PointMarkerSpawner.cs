using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMarkerSpawner : MonoBehaviour
{
    public GameObject PointMarkerPrefab;
 
    public void Spawn(int pointType, int pointValue)
    {
        GameObject spawnedMarker = Instantiate(PointMarkerPrefab, transform.position, Quaternion.identity);
        spawnedMarker.GetComponent<PointMarker>().SetPoints(pointType, pointValue);
    }
    
    public void Spawn(int pointType, int pointValue, float positionY)
    {
        Vector3 position = new Vector3(transform.position.x, positionY, transform.position.z);
        GameObject spawnedMarker = Instantiate(PointMarkerPrefab, position, Quaternion.identity);
        spawnedMarker.GetComponent<PointMarker>().SetPoints(pointType, pointValue);
    }
    
    public void Spawn(int pointType, int pointValue, Vector3 position)
    {
        GameObject spawnedMarker = Instantiate(PointMarkerPrefab, position, Quaternion.identity);
        spawnedMarker.GetComponent<PointMarker>().SetPoints(pointType, pointValue);
    }
}
