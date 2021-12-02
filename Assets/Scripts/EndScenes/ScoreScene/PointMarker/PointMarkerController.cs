using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMarkerController : MonoBehaviour
{
    private const string MARKER_SPAWNER_TAG = "PointMarkerSpawner";

    public Camera MainScoreCamera;
    
    private List<PointMarkerSpawner> _pointMarkerSpawners;

    private GameObject _buttonStart;
    private GameObject _buttonReady;
    

    public void Awake()
    {
        Camera.current.enabled = false;
        MainScoreCamera.enabled = true;
        
        _pointMarkerSpawners = new List<PointMarkerSpawner>();
        foreach (GameObject child in gameObject.FindChildrenWithTag(MARKER_SPAWNER_TAG))
        {
            _pointMarkerSpawners.Add(child.GetComponent<PointMarkerSpawner>());
        }

    }

    public void SpawnMarkerAt(int player, int pointType, int pointValue)
    {
        _pointMarkerSpawners[player].Spawn(pointType,pointValue);
    }
}
