using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMarker : MonoBehaviour
{
    private int _points;

    private int _pointType;

    public void SetPoints(int pointType, int point)
    {
        _pointType = pointType;
        _points = point;
        transform.localScale = transform.localScale + (new Vector3(0.1f, 0.1f, 0.05f) * _points);
    }
}
