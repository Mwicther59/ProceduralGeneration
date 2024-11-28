using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraRotate : MonoBehaviour
{
    [SerializeField] private Transform terrain;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(terrain);
    }
}
