using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class MouseAxes
{
    public float X { get; set; }
    public float Y { get; set; }

    public MouseAxes(float x, float y)
    {
        X = x;
        Y = y;
    }
}

public class PlayerViewRx : MonoBehaviour
{
    public float sensitivity = 100;

    public Transform playerBody;

    private float rotationX;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Observable.EveryLateUpdate()
        .Select(mouseAxes => 
            new MouseAxes(
                Input.GetAxis("Mouse X"), 
                Input.GetAxis("Mouse Y")
            )
        )
        .Subscribe(mouseAxes => HandleSubscription(mouseAxes));
    }
    
    private void HandleSubscription(MouseAxes mouseAxes)
    {
        float mouseX = mouseAxes.X * sensitivity * Time.deltaTime;
        float mouseY = mouseAxes.Y * sensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90);

        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
