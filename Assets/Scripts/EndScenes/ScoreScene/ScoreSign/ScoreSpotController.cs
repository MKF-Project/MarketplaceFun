using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSpotController : MonoBehaviour
{

    public List<ScoreSpot> ScoreSpots;


    public void StartPointsAt(int index, int points)
    {
        ScoreSpots[index].StartPoints(points);
    }

    public void AddPointsAt(int index, int points)
    {
        ScoreSpots[index].AddPoints(points);
    }

    public void TurnSpotOn(int index)
    {
        ScoreSpots[index].TurnSpotOn();
    }
}
