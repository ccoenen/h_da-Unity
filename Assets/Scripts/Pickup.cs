using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public Vector3 rotation;
    public GameObject player;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotation);
        transform.LookAt(player.transform);
    }
}
