using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class CameraBehaviour : MonoBehaviour
{
    PlayerBehaviour playerBehaviour;
    GameObject playerGameObject;
    Rigidbody2D playerRigidBody;

    public Camera Camera;

    // This constant defines the offset the player should have from the camera when not moving
    public float CameraLag;


    // This variable times how long since horizontal input is detected
    // It doesn't reset to 0 immediately but goes down at the same rate as it goes up
    private float horizontalVelocityTimer;
    
    // This constant defines the cap of horizontalVelocityTimer
    // This defines how long it takes to go from 0 to maximum offset
    private float HorizontalVelocityTimerMaximum = 0.2f;


    // These variables define whether the player is changing direction, how long it has been starting to change, and where the camera is coming from and aiming to get to while turning around
    private bool changingDirection;
    private float changingDirectionTimer;
    private float changingDirectionRelativeOrigin;
    private float changingDirectionTarget;

    // This constant defines the cap of changingDirectionTimer
    // This defines how long it takes to change the camera position
    private float ChangingDirectionTimerMaximum = 0.2f;


    // This variable defines whether the camera is fixed
    private bool currentlyFixed;
    

    // This variable defines whether the player was facing right in the previous frame
    private bool previouslyFacingRight;


    // Start is called before the first frame update
    void Start()
    {
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        playerBehaviour = playerGameObject.GetComponent<PlayerBehaviour>();
        playerRigidBody = playerGameObject.GetComponent<Rigidbody2D>();

        previouslyFacingRight = playerBehaviour.isFacingRight;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // If not currently fixed, trail the player
        if (!currentlyFixed)
        {
            float topBound;
            float bottomBound;
            float leftBound;
            float rightBound;

            Vector3 cameraPosition = gameObject.transform.position;
            Vector2 PlayerPosition = playerGameObject.transform.position;
            Vector2 RelativePosition = playerGameObject.transform.position - cameraPosition;

            // If currently moving horizontally, increase the timer
            if (Mathf.Abs(playerRigidBody.velocity.x) > 0)
            {
                horizontalVelocityTimer += Time.deltaTime;

                // If the timer reaches the limit, cap it
                if (horizontalVelocityTimer > HorizontalVelocityTimerMaximum)
                {
                  horizontalVelocityTimer = HorizontalVelocityTimerMaximum;
                }
            }
            // If not currently moving horizontally, reduce the timer
            else
            {
                horizontalVelocityTimer -= Time.deltaTime;

                // If the timer reaches 0, cap it
                if (horizontalVelocityTimer < 0)
                {
                    horizontalVelocityTimer = 0;
                }
            }

            cameraPosition.x = TrackObjectHorizontally(PlayerPosition.x, playerRigidBody.velocity.x);

            float CameraHeight = Camera.orthographicSize;
            float CameraWidth = Camera.orthographicSize * 16 / 9;

            topBound = CalculateVerticalCameraBound(Vector2.up, CameraHeight, cameraPosition);
            bottomBound = CalculateVerticalCameraBound(Vector2.down, CameraHeight, cameraPosition);
            leftBound = CalculateHorizontalCameraBound(Vector2.left, CameraWidth, cameraPosition);
            rightBound = CalculateHorizontalCameraBound(Vector2.right, CameraWidth, cameraPosition);

            if (Mathf.Abs(leftBound) != Mathf.Infinity && Mathf.Abs(rightBound) != Mathf.Infinity)
            {
                cameraPosition.x = (leftBound + rightBound) / 2;
            }
            else if (Mathf.Abs(leftBound) != Mathf.Infinity && Mathf.Abs(leftBound) < Mathf.Abs(cameraPosition.x - CameraWidth))
            {
                cameraPosition.x = leftBound + CameraWidth;
            }
            else if (Mathf.Abs(rightBound) != Mathf.Infinity && Mathf.Abs(rightBound) < Mathf.Abs(cameraPosition.x + CameraWidth))
            {
                cameraPosition.x = rightBound - CameraWidth;
            }

            if (Mathf.Abs(bottomBound) != Mathf.Infinity && Mathf.Abs(topBound) != Mathf.Infinity)
            {
                cameraPosition.y = (bottomBound + topBound) / 2;
            }
            else if (Mathf.Abs(bottomBound) != Mathf.Infinity && Mathf.Abs(bottomBound) < Mathf.Abs(cameraPosition.y - CameraHeight))
            {
                cameraPosition.y = bottomBound + CameraHeight;
            }
            else if (Mathf.Abs(topBound) != Mathf.Infinity && Mathf.Abs(topBound) < Mathf.Abs(cameraPosition.y + CameraHeight))
            {
                cameraPosition.y = topBound - CameraHeight;
            }

            gameObject.transform.position = cameraPosition;
        }
    }

    private float CalculateHorizontalCameraBound(Vector2 BoundDirection, float CameraWidth, Vector2 CameraPosition)
    {
        RaycastHit2D[] hitObjects = Physics2D.RaycastAll(CameraPosition, BoundDirection, CameraWidth, LayerMask.GetMask("Floors and Walls"));

        foreach (RaycastHit2D hitObject in hitObjects)
        {
            GenericEnvironmentBehaviour HitObjectBehaviour = hitObject.collider.gameObject.GetComponent<GenericEnvironmentBehaviour>();

            if (HitObjectBehaviour.HorizontalCameraBounding)
            {
                return (hitObject.transform.position.x + HitObjectBehaviour.RelativeHorizontalCameraBounding);
            }
        }

        return BoundDirection.x * Mathf.Infinity;
    }

    private float CalculateVerticalCameraBound(Vector2 BoundDirection, float CameraHeight, Vector2 CameraPosition)
    {
        RaycastHit2D[] hitObjects = Physics2D.RaycastAll(CameraPosition, BoundDirection, CameraHeight, LayerMask.GetMask("Floors and Walls"));

        foreach (RaycastHit2D hitObject in hitObjects)
        {
            GenericEnvironmentBehaviour HitObjectBehaviour = hitObject.collider.gameObject.GetComponent<GenericEnvironmentBehaviour>();

            if (HitObjectBehaviour.VerticalCameraBounding)
            {
                return (hitObject.transform.position.y + HitObjectBehaviour.RelativeVerticalCameraBounding);
            }
        }

        return BoundDirection.y * Mathf.Infinity;
    }

    private float TrackObjectHorizontally(float HorizontalPlayerPosition, float HorizontalVelocity)
    {
        // Calculate the direction
        float direction;
        if (playerRigidBody.velocity.x > 0)
        {
            direction = 1;
        }
        else if (playerRigidBody.velocity.x < 0)
        {
            direction = -1;
        }
        else
        {
            if (previouslyFacingRight)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }
        }

        bool currentlyFacingRight = playerBehaviour.isFacingRight;
        float horizontalCameraPosition;

        // If still moving in the same direction
        if (currentlyFacingRight == previouslyFacingRight)
        {
            float timeFactor = 0;
            if (horizontalVelocityTimer > 0)
            {
                // I want to change speedFactor to be non linear
                // Or I may want to return to return to 0 slower than usual
                float speedFactor = 0;
                if (Mathf.Abs(HorizontalVelocity) > 5)
                {
                    speedFactor = (HorizontalVelocity - direction * 5) / 5;
                }

                timeFactor = (direction * CameraLag + speedFactor) * horizontalVelocityTimer / HorizontalVelocityTimerMaximum;

                if (Mathf.Abs(timeFactor) >= CameraLag + direction * speedFactor)
                {
                    timeFactor = Mathf.Sign(timeFactor) * (CameraLag + direction * speedFactor);
                }
            }

            // If changing direction
            if (changingDirection)
            {
                // Update the changing direction timer
                changingDirectionTimer += Time.deltaTime;

                // If reached the maximum, cap the timer
                if (changingDirectionTimer > ChangingDirectionTimerMaximum)
                {
                    changingDirection = false;
                    changingDirectionTimer = ChangingDirectionTimerMaximum;
                }

                // Coming from: The player position plus the starting offset
                float ChangingDirectionOrigin = HorizontalPlayerPosition + changingDirectionRelativeOrigin;

                // Going to: The player position plus the ending offset
                changingDirectionTarget = HorizontalPlayerPosition + direction * CameraLag - timeFactor;

                // Smoothly transition between coming from and going to
                // Coming from origin + gradient * time
                horizontalCameraPosition = ChangingDirectionOrigin + changingDirectionTimer * (changingDirectionTarget - ChangingDirectionOrigin) / (ChangingDirectionTimerMaximum);
            }   
            // Otherwise if not, position as usual using the time factor offset
            else
            {
                horizontalCameraPosition = HorizontalPlayerPosition + direction * CameraLag - timeFactor;
            }
        }
        // Otherwise update which way is being faced and set the timer to the maximum
        // Note that the timer is actually set negative and counts up; this is as the offset should return to 0 instead of start at 0
        else
        {
            previouslyFacingRight = currentlyFacingRight;

            changingDirection = true;
            changingDirectionTimer = 0;
            changingDirectionRelativeOrigin = gameObject.transform.position.x - HorizontalPlayerPosition;
            changingDirectionTarget = playerGameObject.transform.position.x + direction * CameraLag;

            horizontalCameraPosition = gameObject.transform.position.x;
        }

        return horizontalCameraPosition;                                                                                         
    }

    public void FixCamera(Vector2 position)
    {
        currentlyFixed = true;

        gameObject.transform.position = position;
    }

    public void UnfixCamera()
    {
        currentlyFixed = false;
    }
}
