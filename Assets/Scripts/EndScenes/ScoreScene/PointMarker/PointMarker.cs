using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMarker : MonoBehaviour
{
    private int _points;

    private int _pointType;
    
    private List<GameObject> DiceObjects;

    private void Awake()
    {
        DiceObjects = gameObject.FindChildrenWithTag("Dice");
        //_meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetPoints(int pointType, int point)
    {
        _pointType = pointType;
        _points = point;
        transform.localScale = transform.localScale + (new Vector3(0.1f, 0.1f, 0.05f) * _points);
        
        //Colocar cor
        if (point < 6)
        {
            GameObject dice = DiceObjects[point-1];
            dice.SetActive(true);
            MeshRenderer meshRenderer = dice.GetComponent<MeshRenderer>();

            int colorIndex = 0;
            if (point == 1)
            {
                colorIndex = 1;
            }

            Material[] materials = meshRenderer.materials;
            materials[colorIndex] = ScoreConfig.ScoreTypeDictionary[pointType].ScoreColor;;
            meshRenderer.materials = materials;
        }

        
    }
}
