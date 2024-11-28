using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody; 

    float xRotation = 0f; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //temp
    }

    void Update()
    {
        // Get mouse input for horizontal (X) and vertical (Y) movement
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Apply vertical rotation (up and down movement, clamping to avoid flipping)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Clamp between -90 and 90 degrees

        // Rotate the camera on the x-axis (up and down)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player body on the y-axis (left and right movement)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
