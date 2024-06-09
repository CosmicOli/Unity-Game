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
    // These variables are references to the player and their properties
    PlayerBehaviour playerBehaviour;
    GameObject playerGameObject;
    Rigidbody2D playerRigidBody;

    // This constant refers to the camera component of this game object
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


    // This variable defines whether the player has moved since touching the ground
    private bool hasMovedAfterTouchingTheGround;


    // Start is called before the first frame update
    void Start()
    {
        // Defining the references to the player 
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        playerBehaviour = playerGameObject.GetComponent<PlayerBehaviour>();
        playerRigidBody = playerGameObject.GetComponent<Rigidbody2D>();

        // First assigning whether the player is facing right
        previouslyFacingRight = playerBehaviour.isFacingRight;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // If not currently fixed, trail the player
        if (!currentlyFixed)
        {
            // Creating the camera bounds
            float topBound;
            float bottomBound;
            float leftBound;
            float rightBound;

            // Defining the key object positions
            Vector3 cameraPosition = gameObject.transform.position;
            Vector2 PlayerPosition = playerGameObject.transform.position;

            // If currently moving horizontally, increase the timer
            if (Mathf.Abs(playerRigidBody.velocity.x) > 0)
            {
                horizontalVelocityTimer += Time.deltaTime;

                if (playerBehaviour.isGrounded)
                {
                    hasMovedAfterTouchingTheGround = true;
                }

                // If the timer reaches the limit, cap it
                if (horizontalVelocityTimer > HorizontalVelocityTimerMaximum)
                {
                  horizontalVelocityTimer = HorizontalVelocityTimerMaximum;
                }
            }
            // If not currently moving horizontally
            else
            {
                // If on the floor, reduce the timer
                if (playerBehaviour.isGrounded && hasMovedAfterTouchingTheGround)
                {
                    horizontalVelocityTimer -= Time.deltaTime;
                }
                else
                {
                    hasMovedAfterTouchingTheGround = false;
                }

                // If the timer reaches 0, cap it
                if (horizontalVelocityTimer < 0)
                {
                    horizontalVelocityTimer = 0;
                }
            }

            // Track the player horizontally and assign the camera x position
            cameraPosition.x = TrackObjectHorizontally(PlayerPosition.x, playerRigidBody.velocity.x);
            cameraPosition.y = TrackObjectVertically(PlayerPosition.y, playerRigidBody.velocity.y);

            // Defining the height and width of the camera when orthographic
            //float CameraHeight = Camera.orthographicSize;
            //float CameraWidth = Camera.orthographicSize * Camera.aspect;

            // Defining the height and width of the camera when perspective
            // Unity links orthographic size and FOV by them being an equal viewing at z=-10 with an angle 10*size
            float CameraHeight = Mathf.Tan(Mathf.PI * Camera.fieldOfView / 360) * Mathf.Abs(gameObject.transform.position.z);
            float CameraWidth = CameraHeight * Camera.aspect;

            // Finding the camera bounds
            topBound = CalculateVerticalCameraBound(1, CameraHeight, cameraPosition);
            bottomBound = CalculateVerticalCameraBound(-1, CameraHeight, cameraPosition);
            leftBound = CalculateHorizontalCameraBound(-1, CameraWidth, cameraPosition);
            rightBound = CalculateHorizontalCameraBound(1, CameraWidth, cameraPosition);

            // If the horizontal bounds are closer than the horizontal size of the camera, bound the camera
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

            // If the vertical bounds are closer than the horizontal size of the camera, bound the camera
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

            // Reassign the camera position to be the newly calculated position
            gameObject.transform.position = cameraPosition;
        }
    }

    private float TrackObjectHorizontally(float HorizontalPlayerPosition, float HorizontalPlayerVelocity)
    {
        // Calculate the direction
        float horizontalDirection;
        if (HorizontalPlayerVelocity > 0)
        {
            horizontalDirection = 1;
        }
        else if (HorizontalPlayerVelocity < 0)
        {
            horizontalDirection = -1;
        }
        else
        {
            if (previouslyFacingRight)
            {
                horizontalDirection = 1;
            }
            else
            {
                horizontalDirection = -1;
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
                if (Mathf.Abs(HorizontalPlayerVelocity) > 5)
                {
                    speedFactor = (HorizontalPlayerVelocity - horizontalDirection * 5) / 5;
                }

                timeFactor = (horizontalDirection * CameraLag + speedFactor) * horizontalVelocityTimer / HorizontalVelocityTimerMaximum;

                if (Mathf.Abs(timeFactor) >= CameraLag + horizontalDirection * speedFactor)
                {
                    timeFactor = Mathf.Sign(timeFactor) * (CameraLag + horizontalDirection * speedFactor);
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
                changingDirectionTarget = HorizontalPlayerPosition + horizontalDirection * CameraLag - timeFactor;

                // Smoothly transition between coming from and going to
                // Coming from origin + gradient * time
                horizontalCameraPosition = ChangingDirectionOrigin + changingDirectionTimer * (changingDirectionTarget - ChangingDirectionOrigin) / (ChangingDirectionTimerMaximum);
            }
            // Otherwise if not, position as usual using the time factor offset
            else
            {
                horizontalCameraPosition = HorizontalPlayerPosition + horizontalDirection * CameraLag - timeFactor;
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
            changingDirectionTarget = playerGameObject.transform.position.x + horizontalDirection * CameraLag;

            horizontalCameraPosition = gameObject.transform.position.x;
        }

        return horizontalCameraPosition;
    }

    private float TrackObjectVertically(float VerticalPlayerPosition, float VerticalPlayerVelocity)
    {
        // Calculate the direction
        float verticalDirection = 0;
        if (VerticalPlayerVelocity > 0)
        {
            verticalDirection = 1;
        }
        else if (VerticalPlayerVelocity < 0)
        {
            verticalDirection = -1;
        }

        float verticalMaxSpeed = playerBehaviour.TerminalSpeed - 5;

        float verticalCameraPosition;
        float speedFactor = 0;
        if (Mathf.Abs(VerticalPlayerVelocity) > verticalMaxSpeed)
        {
            speedFactor = (VerticalPlayerVelocity - verticalDirection * verticalMaxSpeed) / verticalMaxSpeed;
        }

        verticalCameraPosition = VerticalPlayerPosition + speedFactor;

        return verticalCameraPosition;
    }

    private float CalculateHorizontalCameraBound(float BoundDirection, float CameraWidth, Vector2 CameraPosition)
    {
        // Find all objects along the camera's axis within it's view
        RaycastHit2D[] HitObjects = Physics2D.RaycastAll(CameraPosition, new Vector2(BoundDirection, 0), CameraWidth, LayerMask.GetMask("Floors and Walls"));

        foreach (RaycastHit2D hitObject in HitObjects)
        {
            GenericEnvironmentBehaviour HitObjectBehaviour = hitObject.collider.gameObject.GetComponent<GenericEnvironmentBehaviour>();

            // If the bound exists, return it
            if (HitObjectBehaviour.HorizontalCameraBounding)
            {
                return (hitObject.transform.position.x + HitObjectBehaviour.RelativeHorizontalCameraBounding);
            }
        }

        return BoundDirection * Mathf.Infinity;
    }

    private float CalculateVerticalCameraBound(float BoundDirection, float CameraHeight, Vector2 CameraPosition)
    {
        RaycastHit2D[] hitObjects = Physics2D.RaycastAll(CameraPosition, new Vector2(0, BoundDirection), CameraHeight, LayerMask.GetMask("Floors and Walls"));

        foreach (RaycastHit2D hitObject in hitObjects)
        {
            GenericEnvironmentBehaviour HitObjectBehaviour = hitObject.collider.gameObject.GetComponent<GenericEnvironmentBehaviour>();

            // If the bound exists, return it
            if (HitObjectBehaviour.VerticalCameraBounding)
            {
                return (hitObject.transform.position.y + HitObjectBehaviour.RelativeVerticalCameraBounding);
            }
        }

        return BoundDirection * Mathf.Infinity;
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
