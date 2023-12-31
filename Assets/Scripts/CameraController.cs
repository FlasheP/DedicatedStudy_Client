using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerManager player;
    public float xSensitivity = 300f;
    public float ySensitivity = 300f;
    public float clampAngle = 85f;

    private float verticalRotation;
    private float horizontalRotation;

    private void Start()
    {
        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = transform.localEulerAngles.y;
    }
    private void Update()
    {
        Look();
        Debug.DrawRay(transform.position, transform.forward * 2, Color.red);
    }
    private void Look()
    {
        float mouseVertical = -Input.GetAxis("Mouse Y");
        float mouseHorizontal = Input.GetAxis("Mouse X");

        verticalRotation += mouseVertical * ySensitivity * Time.deltaTime;
        horizontalRotation += mouseHorizontal * xSensitivity * Time.deltaTime;

        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        player.transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
    }
}
