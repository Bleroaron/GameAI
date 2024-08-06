using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera camera;
    public float cameraHeight = 50f;
    public float cameraAngle = 90.0f; 
    void Start()
    {
        camera = Camera.main;
    }

   
    void FixedUpdate()
    {
       
        if (camera != null)
        {
            PositionCamera();
        }

    }
    void PositionCamera()
    {
        camera.transform.position = transform.position + new Vector3(0, cameraHeight, 0);
        camera.transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
    }

}
