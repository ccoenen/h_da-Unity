using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallController : MonoBehaviour
{
    public Text pointsText;
    public float speed;
    public MQTTClient mqtt;

    private Matrix4x4 mqttToWorldCoordinates = new Matrix4x4(
        new Vector4(0,     0,     -1.0f,  0),
        new Vector4(1.0f,  0,     0,     0),
        new Vector4(0,     0,     0,     0),
        new Vector4(0,     0,     0,     0)
    );

    private int points = 0;
    private Rigidbody rigidbody;

    // Use this for initialization
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        Vector3 movement = new Vector3(horizontal, 0, vertical);
        Vector3 mqttMovement = mqttToWorldCoordinates.MultiplyVector(mqtt.direction);
        rigidbody.AddForce(mqttMovement);
        rigidbody.AddForce(movement * speed);
    }

    // Called by Unity when a trigger collider enters
    void OnTriggerEnter(Collider other)
    {
        other.gameObject.SetActive(false);
        points++;

        pointsText.text = "Punkte: " + points;

        if (points == 10)
        {
            pointsText.text = "Gewonnen!";
        }
    }
}
