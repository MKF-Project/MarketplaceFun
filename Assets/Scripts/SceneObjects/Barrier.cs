using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    public float DescendSpeed;
    public float MoveDistance;

    private bool _openGate = false;
    private float _targetHeight;

    private void Awake()
    {
        _targetHeight = transform.position.y - MoveDistance;

        SpawnController.OnSpawnOpened += OpenBarrier;
    }

    private void Update()
    {
        if(_openGate && transform.position.y > _targetHeight)
        {
            transform.position += Vector3.down * DescendSpeed * Time.deltaTime;
        }
    }

    private void OpenBarrier()
    {
        SpawnController.OnSpawnOpened -= OpenBarrier;

        _openGate = true;
    }
}
