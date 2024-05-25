using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : GenericGravityEntityBehaviour
{
    // This variable is a normalised vector that points in the direction of movement input
    private Vector2 inputDirection;

    // This variable is used to mark the direction the player is facing
    private bool isFacingRight;

    // Variables used to allow a jump to be queued slightly before hitting the floor
    private bool jumpPreEntered = false;
    private float jumpPreEnteredTimer = 0;
    private InputAction.CallbackContext preEnteredContext;

    // A variable that corresponds to whether the floor currently in contact
    // Used to determine whether the player can jump
    private bool isGrounded = false;

    // A variable that corresponds to whether the player is currently in a jump
    // This is used to make a distinction between upward velocity and jumping
    private bool currentlyJumping = false;

    // This variable is used to determine vertical acceleration to see if any other force than gravity has been applied
    private float? previousVerticalVelocity;

    // This is a constant that links to the attack behaviour script
    [SerializeField]
    private AttackBehaviour AttackBehaviour;

    // Update is called once per frame
    void Update()
    {
        // If jump has been pre-entered, update the timer
        if (jumpPreEntered)
        {
            jumpPreEnteredTimer += Time.deltaTime;

            // If the ellapsed time exceeds the cutoff time, the jump is marked as no longer having been pre-entered
            if (jumpPreEnteredTimer > 0.05)
            {
                jumpPreEntered = false;
                jumpPreEnteredTimer = 0;
            }
        }

        // Change the character direction when changing input direction
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

        // Update the current direction stored in the AttackBehaviour script
        AttackBehaviour.GetDirection(inputDirection);

        // Update whether the player is currently jumping
        if (currentlyJumping)
        {
            currentlyJumping = isJumping();
        }
    }

    // Flips the character horizontally
    private void FlipCharacter()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If on the floor, update isGrounded
        if (collision.gameObject.layer == 6)
        {
            isGrounded = true;

            // If the player has pre-entered jump, jump
            if (jumpPreEntered)
            {
                Jump(preEnteredContext);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // If no longer on the floor, update isGrounded
        if (collision.gameObject.layer == 6)
        {
            isGrounded = false;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        // When moving, update the input directions
        inputDirection = context.ReadValue<Vector2>();
        horizontalAccelerationDirection = inputDirection.x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        // If on the ground then accelerate upwards
        if (isGrounded)
        {
            if (context.performed)
            {
                entityRigidBody.velocity = new Vector2(entityRigidBody.velocity.x, JumpAccelerationPower);
                currentlyJumping = true;
            }
        }
        // If not on the ground, preregister the jump so it activates when hitting the floor
        else
        {
            jumpPreEntered = true;
            preEnteredContext = context;
        }

        // If the jump input is cancelled
        if (context.canceled)
        {
            // If the jump is pre-entered, cancel the pre-enter
            if (jumpPreEntered)
            {
                jumpPreEntered = false;
                jumpPreEnteredTimer = 0;
            }

            // If the jump is currently happening, cancel the current jump
            if (entityRigidBody.velocity.y > 0f && currentlyJumping)
            {
                // Reduces upwards velocity to a 20th
                // This keeps responsiveness to stop the player quickly but doesn't fully stop the player
                entityRigidBody.velocity = new Vector2(entityRigidBody.velocity.x, entityRigidBody.velocity.y * 1 / 20);
            }
        }
    }

    private bool isJumping()
    {
        float currentVerticalVelocity = entityRigidBody.velocity.y;

        // From rest, initual acceleration is equal to the jump power
        // Hence, this first acceleration is skipped by being marked as null
        if (previousVerticalVelocity != null)
        {
            // If gravity and the current vertical acceleration match, no change in vertical acceleration has occured
            // Note the multiples by 10000 and the conversion to nullable integers; this is to round the measured gravity
            if ((int?)((currentVerticalVelocity - previousVerticalVelocity) * 10000) == (int?)(-9.81 * gravityScale * 10000 / 50))
            {
                previousVerticalVelocity = currentVerticalVelocity;
                return true;
            }
            // If gravity and the current vertical acceleration do not match, another acceleration has occured and the jump is cancelled
            else
            {
                previousVerticalVelocity = null;
                return false;
            }
        }
        // If this is the first frame of the jump, the jump is currently in progress
        else
        {
            previousVerticalVelocity = currentVerticalVelocity;
            return true;
        }
    }
}
