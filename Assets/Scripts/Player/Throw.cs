using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    public GameObject Bomb;
    private Rigidbody _bombRigidbody;
    public float Strength;
    //public Camera Camera;

    //public GameObject InstantiatedBomb;
    // Start is called before the first frame update
    private void Awake()
    {
        _bombRigidbody = Bomb.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 screenMiddle = new Vector3();
        screenMiddle.x = Screen.width / 2;
        screenMiddle.y = Screen.height / 2;
        Ray ray = Camera.main.ScreenPointToRay(screenMiddle);
            
            
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
        _bombRigidbody.useGravity = true;

        if (InputManager.FireButton())
        {
            Bomb.transform.position = new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
            Bomb.SetActive(true);
            _bombRigidbody.useGravity = false;
        }

        if(InputManager.ReleaseFireButton())
        {
            _bombRigidbody.useGravity = true;
            _bombRigidbody.AddForce(ray.direction  * Strength, ForceMode.Force);
            
        }
    }
}
