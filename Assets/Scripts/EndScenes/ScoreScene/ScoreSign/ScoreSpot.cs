using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSpot : MonoBehaviour
{
    //Score Sign
    public Text Text;
    private int _currentPoints;

    //Score Color
    public Material PlayerColor;

    public MeshRenderer PipeUp;
    public MeshRenderer Tube;
    public MeshRenderer ScoreBase;
    public MeshRenderer ScoreSign;


    public void AddPoints(int points)
    {
        StartCoroutine(nameof(AddPointsCoroutine), points);
    }

    private IEnumerator AddPointsCoroutine(int points)
    {
        for (int amount = points; amount > 0; amount--)
        {
            _currentPoints++;
            SetRightPoint(_currentPoints);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void StartPoints(int points)
    {
        _currentPoints = points;
        SetRightPoint(_currentPoints);
    }

    public void SetRightPoint(int points)
    {
        if (points < 10)
        {
            Text.text = "0" + points;
            return;
        }
        Text.text = "" + points;
        
    }


    public void TurnSpotOn()
    {
        EditMaterialOfMesh(PipeUp, 1);
        EditMaterialOfMesh(Tube, 1);
        EditMaterialOfMesh(ScoreBase, 0);
        EditMaterialOfMesh(ScoreSign, 0);

    }

    private void EditMaterialOfMesh(MeshRenderer mesh, int materialIndex)
    {
        Material[] materials = mesh.materials;
        materials[materialIndex] = PlayerColor;
        mesh.materials = materials;
    }

}
