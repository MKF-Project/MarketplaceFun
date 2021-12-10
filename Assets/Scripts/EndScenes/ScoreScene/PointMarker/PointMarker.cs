using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMarker : MonoBehaviour
{
    private int _points;

    private int _pointType;

    private List<GameObject> DiceObjects;
    
    private bool _goToPosition;

    public float Speed;

    private Vector3 _target;

    private void Awake()
    {
        _goToPosition = false;
        DiceObjects = gameObject.FindChildrenWithTag("Dice");
        //_meshRenderer = GetComponent<MeshRenderer>();
        ScoreSceneManager.OnWin += PrepareToWin;
    }

    private void OnDestroy()
    {
        ScoreSceneManager.OnWin -= PrepareToWin;
    }

    public void SetPoints(int pointType, int point)
    {
        _pointType = pointType;
        _points = point;
        transform.localScale = new Vector3(0.2f, 0.2f, 0.2f) * _points;

        //Colocar cor
        if (point < 6)
        {
            GameObject dice = DiceObjects[point - 1];
            dice.SetActive(true);
            MeshRenderer meshRenderer = dice.GetComponent<MeshRenderer>();

            int colorIndex = 0;
            if (point == 1)
            {
                colorIndex = 1;
            }

            Material[] materials = meshRenderer.materials;
            materials[colorIndex] = ScoreConfig.ScoreTypeDictionary[pointType].ScoreColor;
            ;
            meshRenderer.materials = materials;
        }


    }

    
    void Update()
    {
        if (_goToPosition)
        {
            float step =  Speed * Time.deltaTime; 
            transform.position = Vector3.MoveTowards(transform.position, _target, step);

            if (Vector3.Distance(transform.position, _target) < 0.01f)
            {
                transform.position = _target;
                _goToPosition = false;
            }
        }
    }

    public void PrepareToWin(int winnerIndex)
    {
        switch (winnerIndex)
        {
            case 1:
                ScrollToSide(-3);
                break;
            case 2:
                ScrollToSide(-1);
                break;
            case 3:
                ScrollToSide(1);
                break;
            case 4:
                ScrollToSide(3);
                break;
        }
    }
    
    public void ScrollToSide(float positionX)
    {
        Vector3 markerPosition = transform.position;
        if (markerPosition.x.Equals(positionX))
        {
            if (positionX < 0)
            {
                //Scroll Left
                _target = new Vector3(-15, markerPosition.y, markerPosition.z);
                _goToPosition = true;

            }
            else
            {
                //Scroll Right
                _target = new Vector3(15, markerPosition.y, markerPosition.z);
                _goToPosition = true;

            }

        }

        else if (markerPosition.x > positionX)
        {
            //Scroll Right
            _target = new Vector3(15, markerPosition.y, markerPosition.z);
            _goToPosition = true;

        }
        else
        {
            //Scroll Left
            _target = new Vector3(-15, markerPosition.y, markerPosition.z);
            _goToPosition = true;

        }
    }


}