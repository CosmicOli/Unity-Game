using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : GenericGravityEntityBehaviour
{
    private Vector2 inputDirection;

    private bool isFacingRight;

    private bool jumpPreEntered = false;
    private float jumpPreEnteredTimer = 0;
    private InputAction.CallbackContext preEnteredContext;

    private bool isGrounded = false;

    public AttackBehaviour attackBehaviour;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        entityRigidBody.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (jumpPreEntered)
        {
            jumpPreEnteredTimer += Time.deltaTime;

            if (jumpPreEnteredTimer > 0.05)
            {
                jumpPreEntered = false;
                jumpPreEnteredTimer = 0;
            }
        }

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

            if (jumpPreEntered)
            {
                Jump(preEnteredContext);
            }
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
        // If on the ground then accelerate upwards
        if (isGrounded)
        {
            if (context.performed)
            {
                entityRigidBody.velocity = new Vector2(entityRigidBody.velocity.x, jumpAccelerationPower);
                isGrounded = false;
            }
        }
        // If not on the ground, preregister the jump so it activates when hitting the floor
        else
        {
            jumpPreEntered = true;
            preEnteredContext = context;
        }

        if (context.canceled)
        {
            if (jumpPreEntered)
            {
                jumpPreEntered = false;
                jumpPreEnteredTimer = 0;
            }
        }

        if (context.canceled && entityRigidBody.velocity.y > 0f)
        {
            // Reduces upwards velocity to a 20th
            // This keeps responsiveness to stop the player quickly but doesn't fully stop the player.
            // Currently a glitch where if pogoing the user can release space to cancel their upwards momentum. (would apply if any other source gave upwards velocity too though)
            entityRigidBody.velocity = new Vector2(entityRigidBody.velocity.x, entityRigidBody.velocity.y * 1 / 20);
        }
    }
}
