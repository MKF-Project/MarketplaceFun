using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsManager : MonoBehaviour
{
    public static GameObject OverviewCamera = null;
    [SerializeField]
    private GameObject _overviewCamera = null;

    private void Start()
    {
        if(OverviewCamera == null) {
            OverviewCamera = _overviewCamera;
        }
    }
}
