using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCamera : MonoBehaviour
{
    // Start is called before the first frame update

    private const string WIN_SPOT_TAG = "WinSpot";

    public GameObject ScoreCamera;

    public float Speed;
    
    [HideInInspector]
    public List<Transform> WinSpots;

    private Transform _target;

    private bool _goToPosition;
    
    void Start()
    {
        _goToPosition = false;
        WinSpots = new List<Transform>();
        foreach (GameObject child in gameObject.FindChildrenWithTag(WIN_SPOT_TAG))
        {
            WinSpots.Add(child.GetComponent<Transform>());
        }

        ScoreSceneManager.OnWin += GoToWinSpot;
    }

    private void OnDestroy()
    {
        ScoreSceneManager.OnWin -= GoToWinSpot;
    }

    void Update()
    {
        if (_goToPosition)
        {
            float step =  Speed * Time.deltaTime; 
            ScoreCamera.transform.position = Vector3.MoveTowards(ScoreCamera.transform.position, _target.position, step);

            if (Vector3.Distance(ScoreCamera.transform.position, _target.position) < 0.01f)
            {
                ScoreCamera.transform.position = _target.position;
                _goToPosition = false;
            }
        }
    }
    
    public void GoToWinSpot(int spotIndex)
    {
        spotIndex = spotIndex - 1;
        _target = WinSpots[spotIndex];
        _goToPosition = true;
    }
}
