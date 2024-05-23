using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : GenericEntityBehaviour
{
    private Vector2 inputDirection;

    private bool isFacingRight;
    private bool isGrounded = true;

    public AttackBehaviour attackBehaviour;

    // Start is called before the first frame update
    void Start()
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

    private void FixedUpdate()
    {
        attackBehaviour.GetDirection(inputDirection);

        Vector2 newVelocity = entityRigidBody.velocity;

        if (Mathf.Abs(horizontalAccelerationDirection) > 0)
        {
            Debug.Log(horizontalAccelerationDirection);
        }

        if (Mathf.Abs(entityRigidBody.velocity.x) <= maximumHorizontalSpeedFromPower)
        {
            newVelocity += new Vector2(horizontalAccelerationDirection * horizontalAccelerationPower, 0);

            // Cap the speed at the limit
            if (Mathf.Abs(newVelocity.x) > maximumHorizontalSpeedFromPower)
            {
                newVelocity.x = Mathf.Sign(entityRigidBody.velocity.x) * maximumHorizontalSpeedFromPower;
            }
        }
        else if (Mathf.Abs(entityRigidBody.velocity.x) > maximumHorizontalSpeedFromPower) // When past the speed limit in a direction a drag is experienced
        {
            TakeDragInAnAxis(ref newVelocity.x, horizontalDrag);
        }

        // When there is no attempt at acceleration a drag should be experienced.
        if ((Mathf.Abs(horizontalAccelerationDirection) == 0 && Mathf.Abs(entityRigidBody.velocity.x) > 0) || (Mathf.Abs(entityRigidBody.velocity.x) > maximumHorizontalSpeedFromPower))
        {
            TakeDragInAnAxis(ref newVelocity.x, horizontalDrag);
        }

        entityRigidBody.velocity = newVelocity;
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
        inputDirection = context.ReadValue<Vector2>();
        horizontalAccelerationDirection = inputDirection.x;
        // Note that this means only horizontal values are added
        // For keyboard controls, w and s could hence be removed
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            entityRigidBody.velocity = new Vector2(entityRigidBody.velocity.x, verticalAccelerationPower);
            isGrounded = false;
        }

        if (context.canceled && entityRigidBody.velocity.y > 0f)
        {
            entityRigidBody.velocity = new Vector2(entityRigidBody.velocity.x, entityRigidBody.velocity.y * 0.5f);
        }
    }
}
