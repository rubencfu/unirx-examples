using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Axes
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Horizontal { get; set; }
    public float Vertical { get; set; }

    public Axes(float x, float y, float horizontal, float vertical)
    {
        X = x;
        Y = y;
        Horizontal = horizontal;
        Vertical = vertical;
    }
}

public class spectatorCameraRX : MonoBehaviour
{
    [Header("Look Sensitivity")]
    public float sensitivity = 6;

    [Header("Clamping")]
    public float minY = -90;
    public float maxY = 90;

    [Header("Spectator")]
    public float speed = 10;

    private float rotationX;
    private float rotationY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Observable.EveryLateUpdate()
        .Select(axes => 
            new Axes(
                Input.GetAxis("Mouse X"), 
                Input.GetAxis("Mouse Y"), 
                Input.GetAxis("Horizontal"), 
                Input.GetAxis("Vertical")
            )
        )
        .Subscribe(HandleSubscription);
    }

    private void HandleSubscription(Axes axes) 
    {
        rotationX += axes.X * sensitivity;
        rotationY += axes.Y * sensitivity;

        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        transform.rotation = Quaternion.Euler(-rotationY, rotationX, 0);

        float x = axes.Horizontal;
        float y = 0;
        float z = axes.Vertical;

        Vector3 direction = transform.right * x + transform.up * y + transform.forward * z;
        transform.position += direction * speed * Time.deltaTime;
    }

}
