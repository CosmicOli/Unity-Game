using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackBehaviour : MonoBehaviour
{
    public Vector2 currentDirection;
    private Vector2 lastHorizontalDirection;

    public Vector2 GetDirection(InputAction.CallbackContext context)
    {
        Vector2 inputDirection = context.ReadValue<Vector2>();

        if (Mathf.Abs(inputDirection.x) > 0 && inputDirection.y == 0)
        {
            lastHorizontalDirection = inputDirection;
        }

        if (inputDirection != Vector2.zero)
        {
            currentDirection = inputDirection;
        }
        else
        {
            currentDirection = lastHorizontalDirection;
        }

        return currentDirection;
    }
}
