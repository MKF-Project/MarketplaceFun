using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMarkerController : MonoBehaviour
{
    private const string MARKER_SPAWNER_TAG = "PointMarkerSpawner";
    
    private List<PointMarkerSpawner> _pointMarkerSpawners;

    private GameObject _buttonStart;
    private GameObject _buttonReady;
    

    public void Awake()
    {

        
        _pointMarkerSpawners = new List<PointMarkerSpawner>();
        foreach (GameObject child in gameObject.FindChildrenWithTag(MARKER_SPAWNER_TAG))
        {
            _pointMarkerSpawners.Add(child.GetComponent<PointMarkerSpawner>());
        }

    }

    public void SpawnMarker(int player, int pointType, int pointValue)
    {
        _pointMarkerSpawners[player].Spawn(pointType, pointValue);
    }
    
    public void SpawnMarkerAt(int player, int pointType, int pointValue, float positionY)
    {
        
        _pointMarkerSpawners[player].Spawn(pointType, pointValue, positionY);
    }
    
    public void SpawnMarkerAt(int player, int pointType, int pointValue, Vector3 position)
    {
        _pointMarkerSpawners[player].Spawn(pointType, pointValue, position);
    }
}
