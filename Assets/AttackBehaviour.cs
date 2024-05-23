using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AttackBehaviour : MonoBehaviour
{
    public Vector2 currentDirection;

    public Vector2 GetDirection(InputAction.CallbackContext context)
    {
        Vector2 inputDirection = context.ReadValue<Vector2>();

        if (inputDirection != Vector2.zero)
        {
            currentDirection = inputDirection;
        }

        return currentDirection;
    }
}
