using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public partial class TestSingtonSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (CameraController.instance == null)
            this.Enabled = false;

        {
            if (Input.GetKey(KeyCode.W))
            {
            CameraController.instance.transform.position 
                += Vector3.forward * SystemAPI.Time.DeltaTime * 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
            CameraController.instance.transform.position 
                += Vector3.back * SystemAPI.Time.DeltaTime * 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
            CameraController.instance.transform.position 
                += Vector3.left * SystemAPI.Time.DeltaTime * 1;
            }

            if (Input.GetKey(KeyCode.D))
            {
            CameraController.instance.transform.position 
                += Vector3.right * SystemAPI.Time.DeltaTime * 1;
            }
        


            if (Input.GetKey(KeyCode.Q))
            {
            CameraController.instance.transform.position += Vector3.up * SystemAPI.Time.DeltaTime * 1;
            }

            if (Input.GetKey(KeyCode.E))
            {
            CameraController.instance.transform.position += Vector3.down * SystemAPI.Time.DeltaTime * 1;
            }
        }//이동   

        CameraController.instance.transform.rotation 
            *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * 90 * SystemAPI.Time.DeltaTime * -1, Vector3.right);

        CameraController.instance.transform.rotation 
            *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * 90 * SystemAPI.Time.DeltaTime, Vector3.up);
    }
}
