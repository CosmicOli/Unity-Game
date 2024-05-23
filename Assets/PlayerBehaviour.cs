using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : GenericEntityBehaviour
{
    public Rigidbody2D rigidBody;

    private float horizontal;
    public float runningPower;
    public float jumpingPower;

    private bool isFacingRight;
    private bool isGrounded = true;

    public AttackBehaviour attackBehaviour;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFacingRight && horizontal > 0f)
        {
            FlipCharacter();
        }
        else if (isFacingRight && horizontal < 0f)
        {
            FlipCharacter();
        }
    }

    private void FixedUpdate()
    {
        rigidBody.velocity = new Vector2(horizontal * runningPower, rigidBody.velocity.y);

    }

    private void FlipCharacter()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            isGrounded = true;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
        // Note that this means only horizontal values are added
        // For keyboard controls, w and s could hence be removed

        attackBehaviour.GetDirection(context);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpingPower);
            isGrounded = false;
        }

        if (context.canceled && rigidBody.velocity.y > 0f)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * 0.5f);
        }
    }
}
