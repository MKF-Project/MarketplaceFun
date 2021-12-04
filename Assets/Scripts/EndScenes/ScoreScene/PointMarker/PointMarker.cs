using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMarker : MonoBehaviour
{
    private int _points;

    private int _pointType;

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetPoints(int pointType, int point)
    {
        _pointType = pointType;
        _points = point;
        transform.localScale = transform.localScale + (new Vector3(0.1f, 0.1f, 0.05f) * _points);
        
        //Colocar cor
        _meshRenderer.material = ScoreConfig.ScoreTypeDictionary[pointType].ScoreColor;
    }
}
