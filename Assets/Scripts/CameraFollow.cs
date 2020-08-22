using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("The max number of seconds it takes for the camera to reach the player.")]
    public float followTime = 1f;
    [Tooltip("The transform the camera is following.")]
    public Transform target;

    private Vector2 currentVelocity;

    void FixedUpdate() {
        Vector3 newPosition = Vector2.SmoothDamp(transform.position, target.transform.position, ref currentVelocity, followTime);
        transform.position = newPosition + Vector3.back;
    }
}
