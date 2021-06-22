using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class Throw : NetworkBehaviour
{
    public GameObject BombPrefab;
    private GameObject _bomb;
    private Rigidbody _bombRigidbody;
    public float Strength;

    public float ArcThrow;
    
    public Vector3 targetFake;
    //public Camera Camera;

    //public GameObject InstantiatedBomb;
    // Start is called before the first frame update
    private void Awake()
    {
        if (IsServer)
        {
            _bomb = Instantiate(BombPrefab, Vector3.zero, Quaternion.identity);
            _bomb.GetComponent<NetworkObject>().Spawn();
            _bombRigidbody = _bomb.GetComponent<Rigidbody>();
            _bomb.SetActive(false);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsOwner)
        {
            Vector3 screenMiddle = new Vector3();
            screenMiddle.x = Screen.width / 2;
            screenMiddle.y = Screen.height / 2;
            Ray ray = Camera.main.ScreenPointToRay(screenMiddle);

            Vector3 target = ray.direction * Strength;
            target += Vector3.up * ArcThrow;

            if (InputManager.PressFireButton())
            {
                _bombRigidbody.velocity = Vector3.zero;
                _bomb.transform.position = new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
                _bomb.SetActive(true);
                _bombRigidbody.AddForce(target, ForceMode.Impulse);
                targetFake = target;
            }
        }
    }

}
