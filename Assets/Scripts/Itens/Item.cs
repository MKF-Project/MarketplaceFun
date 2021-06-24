using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private Transform _heldPosition;

    private bool _isHeld;
    // Start is called before the first frame update
    void Start()
    {
        _isHeld = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isHeld)
        {
            transform.position = _heldPosition.position;
        }
    }

    public void BeHeld(Transform holderPosition)
    {
        _heldPosition = holderPosition;
        _isHeld = true;
    }

    public void BeDropped()
    {
        _heldPosition = null;
        _isHeld = false;
    }


}
