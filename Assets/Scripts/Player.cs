using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 1f;

    private Rigidbody2D rb2D;

    void Start() {
        rb2D = GetComponent<Rigidbody2D>();
    }

    void Update() {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        Vector2 moveDir = new Vector2(inputX, inputY).normalized;
        Vector2 moveVel = new Vector2(moveSpeed, moveSpeed * 0.5f);
        rb2D.position += moveDir * moveVel * Time.deltaTime;
    }
}
