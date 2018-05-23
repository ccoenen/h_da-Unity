using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LidarDisplay : MonoBehaviour
{
	public LidarReceiver lidarReceiver;

    static private int CUBE_NUMBER = 360;
    private GameObject[] cubes;


    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < CUBE_NUMBER; i++)
        {
            float percent = ((float)i / (float)CUBE_NUMBER);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.tag = "LIDAR";
            go.name = string.Format("LIDAR box {0}", i);
            go.transform.SetParent(gameObject.transform);
            go.transform.Rotate(Vector3.up * i, Space.World);
        }

        cubes = GameObject.FindGameObjectsWithTag("LIDAR");
    }

    // Update is called once per frame
    void Update()
    {
		for (int i = 0; i < cubes.Length; i++) {
            float percent = ((float)i / (float)CUBE_NUMBER);
            LidarReceiver.Measurement measurement = lidarReceiver.measurements[i];

			GameObject cube = cubes[i];
			//cube.transform.localScale = new Vector3(0.2f, 0.5f * Mathf.Abs(Mathf.Sin((float)i/(float)CUBE_NUMBER * 2 * Mathf.PI + Time.realtimeSinceStartup / 5)), 3.0f);
			Color newColor = Color.HSVToRGB((float)measurement.ss / 3000f, 1.0f, 0.5f);
			cube.GetComponent<MeshRenderer>().material.color = newColor;
			// cube.GetComponent<MeshRenderer>() = newColor;

            float distance = (float)measurement.distance / 100;
            cube.transform.localScale = new Vector3(
                0.0001f * measurement.distance,
                measurement.error ? 0.0f : 0.5f,
                0.1f
            );

            Vector3 position = new Vector3(
                distance / 2 * Mathf.Sin(Mathf.PI * 2 * percent),
                0,
                distance / 2 * Mathf.Cos(Mathf.PI * 2 * percent)
            );
            cube.transform.position = position;
		}
    }
}
