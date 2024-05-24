using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : GenericGravityEntityBehaviour
{
    private Vector2 inputDirection;

    private bool isFacingRight;
    private bool isGrounded = true;

    public AttackBehaviour attackBehaviour;

    // Start is called before the first frame update
    protected override void Start()
    {
        entityRigidBody = gameObject.GetComponent<Rigidbody2D>();
        entityRigidBody.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFacingRight && horizontalAccelerationDirection > 0f)
        {
            FlipCharacter();
        }
        else if (isFacingRight && horizontalAccelerationDirection < 0f)
        {
            FlipCharacter();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        attackBehaviour.GetDirection(inputDirection);
    }

    private void FlipCharacter()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
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
        inputDirection = context.ReadValue<Vector2>();
        horizontalAccelerationDirection = inputDirection.x;
        // Note that this means only horizontal values are added
        // For keyboard controls, w and s could hence be removed
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            entityRigidBody.velocity = new Vector2(entityRigidBody.velocity.x, jumpAccelerationPower);
            isGrounded = false;
        }

        if (context.canceled && entityRigidBody.velocity.y > 0f)
        {
            entityRigidBody.velocity = new Vector2(entityRigidBody.velocity.x, entityRigidBody.velocity.y * 1 / 50);
        }
    }
}
