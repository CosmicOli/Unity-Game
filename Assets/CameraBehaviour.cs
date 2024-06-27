using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

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


    // These variables define 
    //private bool wallTransitioning;
    private bool wallTransitioningHorizontally;
    private bool wallTransitioningVertically;
    private float wallTransitioningHorizontalTimer;
    private float wallTransitioningVerticalTimer;

    // This constant defines the cap of wallTransitionTimer
    // This defines how long it takes 
    private float WallTransitionTimerMaximum = 0.5f;

    // This variable defines the start location of the camera when
    private float startingHorizontalCameraOffsetOnWallTransition;
    private float startingVerticalCameraOffsetOnWallTransition;


    // These variables define whether the player just took knockback and how long it has been since taking knockback
    private bool justKnockedBack;
    private float justKnockedBackTimer;

    // These constants define how long of a delay there is before the camera catches up and how long it takes for the camera to catch up
    private float JustKnockedBackDelay = 0.3f;
    private float JustKnockedBackMaximumAfterDelay = 0.2f;

    // This variable defines the start location of the camera when taking knockback
    private Vector3 startingCameraPositionOnKnockback;


    // This variable defines whether the camera is fixed
    private bool currentlyFixed;
    

    // This variable defines whether the player was facing right in the previous frame
    private bool previouslyFacingRight;


    // This variable defines whether the player has moved since touching the ground
    private bool hasMovedAfterTouchingTheGround;


    // These variables store the previous bounds
    float previousTopBound;
    float previousBottomBound;
    float previousLeftBound;
    float previousRightBound;


    // Start is called before the first frame update
    void Start()
    {
        // Defining the references to the player 
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        playerBehaviour = playerGameObject.GetComponent<PlayerBehaviour>();
        playerRigidBody = playerGameObject.GetComponent<Rigidbody2D>();

        // First assigning whether the player is facing right
        previouslyFacingRight = playerBehaviour.isFacingRight;



        Vector3 cameraPosition = gameObject.transform.position;

        // Defining the height and width of the camera when orthographic
        //float CameraHeight = Camera.orthographicSize;
        //float CameraWidth = Camera.orthographicSize * Camera.aspect;

        // Defining the height and width of the camera when perspective
        // Unity links orthographic size and FOV by them being an equal viewing at z=-10 with an angle 10*size
        float CameraHeight = Mathf.Tan(Mathf.PI * Camera.fieldOfView / 360) * Mathf.Abs(gameObject.transform.position.z);
        float CameraWidth = CameraHeight * Camera.aspect;

        previousTopBound = CalculateVerticalCameraBound(1, CameraHeight, cameraPosition);
        previousBottomBound = CalculateVerticalCameraBound(-1, CameraHeight, cameraPosition);
        previousLeftBound = CalculateHorizontalCameraBound(-1, CameraWidth, cameraPosition);
        previousRightBound = CalculateHorizontalCameraBound(1, CameraWidth, cameraPosition);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // If not currently fixed, trail the player
        if (!currentlyFixed)
        {
            // Defining the key object positions
            Vector3 cameraPosition = gameObject.transform.position;
            Vector3 previousCameraPosition = cameraPosition;
            Vector3 PlayerPosition = playerGameObject.transform.position;

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
            float TargetCameraX = TrackObjectHorizontally(PlayerPosition.x, playerRigidBody.velocity.x);
            float TargetCameraY = TrackObjectVertically(PlayerPosition.y, playerRigidBody.velocity.y);

            // If just after taking knockback
            if (justKnockedBack)
            {
                justKnockedBackTimer += Time.deltaTime;
                if (justKnockedBackTimer > JustKnockedBackDelay)
                {
                    cameraPosition = Vector3.Lerp(startingCameraPositionOnKnockback, new Vector3(TargetCameraX, TargetCameraY, cameraPosition.z), (justKnockedBackTimer - JustKnockedBackDelay) / (JustKnockedBackMaximumAfterDelay));

                    if (justKnockedBackTimer > JustKnockedBackDelay + JustKnockedBackMaximumAfterDelay)
                    {
                        justKnockedBack = false;
                        justKnockedBackTimer = 0;
                    }
                }
            }
            else
            {
                cameraPosition.x = TargetCameraX;
                cameraPosition.y = TargetCameraY;
            }

            // Defining the height and width of the camera when orthographic
            //float CameraHeight = Camera.orthographicSize;
            //float CameraWidth = Camera.orthographicSize * Camera.aspect;

            // Defining the height and width of the camera when perspective
            // Unity links orthographic size and FOV by them being an equal viewing at z=-10 with an angle 10*size
            float CameraHeight = Mathf.Tan(Mathf.PI * Camera.fieldOfView / 360) * Mathf.Abs(gameObject.transform.position.z);
            float CameraWidth = CameraHeight * Camera.aspect;

            // Finding the camera bounds
            float topBound = CalculateVerticalCameraBound(1, CameraHeight, cameraPosition);
            float bottomBound = CalculateVerticalCameraBound(-1, CameraHeight, cameraPosition);
            float leftBound = CalculateHorizontalCameraBound(-1, CameraWidth, cameraPosition);
            float rightBound = CalculateHorizontalCameraBound(1, CameraWidth, cameraPosition);

            // If the horizontal bounds are closer than the horizontal size of the camera, bound the camera
            TargetCameraX = CalculateTargetCameraPositionFromBoundaries(leftBound, rightBound, CameraWidth, cameraPosition.x);

            // If the vertical bounds are closer than the vertical size of the camera, bound the camera
            TargetCameraY = CalculateTargetCameraPositionFromBoundaries(bottomBound, topBound, CameraHeight, cameraPosition.y);

            float[] Bounds = { topBound, bottomBound, leftBound, rightBound };
            float[] PreviousBounds = { previousTopBound, previousBottomBound, previousLeftBound, previousRightBound };
            for (int i = 0; i < 3; i += 1)
            {
                if (Bounds[i] != PreviousBounds[i])
                {
                    switch (i)
                    {
                        case 0:
                            if (previousBottomBound != Mathf.NegativeInfinity)
                            {
                                if (previousTopBound < cameraPosition.y + CameraHeight)
                                {
                                    wallTransitioningVertically = true;
                                }
                            }
                            else
                            {
                                if (topBound < cameraPosition.y + CameraHeight)
                                {
                                    wallTransitioningVertically = true;
                                }
                            }

                            previousTopBound = topBound;

                            startingVerticalCameraOffsetOnWallTransition = previousCameraPosition.y;
                            wallTransitioningVerticalTimer = 0;
                            break;
                            
                        case 1:
                            if (previousBottomBound != Mathf.NegativeInfinity)
                            {
                                if (previousBottomBound > cameraPosition.y - CameraHeight)
                                {
                                    wallTransitioningVertically = true;
                                }
                            }
                            else
                            {
                                if (bottomBound > cameraPosition.y - CameraHeight)
                                {
                                    wallTransitioningVertically = true;
                                }
                            }

                            previousBottomBound = bottomBound;

                            startingVerticalCameraOffsetOnWallTransition = previousCameraPosition.y;
                            wallTransitioningVerticalTimer = 0;
                            break;

                        case 2:
                            if (previousLeftBound != Mathf.NegativeInfinity)
                            {
                                if (previousLeftBound > cameraPosition.x - CameraWidth)
                                {
                                    wallTransitioningHorizontally = true;
                                }
                            }
                            else
                            {
                                if (leftBound > cameraPosition.x - CameraWidth)
                                {
                                    wallTransitioningHorizontally = true;
                                }
                            }

                            previousLeftBound = leftBound;

                            startingHorizontalCameraOffsetOnWallTransition = previousCameraPosition.x;
                            wallTransitioningHorizontalTimer = 0;
                            break;

                        case 3:
                            if (previousRightBound != Mathf.NegativeInfinity)
                            {
                                if (previousRightBound < cameraPosition.y + CameraWidth)
                                {
                                    wallTransitioningHorizontally = true;
                                }
                            }
                            else
                            {
                                if (bottomBound < cameraPosition.y + CameraWidth)
                                {
                                    wallTransitioningHorizontally = true;
                                }
                            }

                            previousRightBound = rightBound;

                            startingHorizontalCameraOffsetOnWallTransition = previousCameraPosition.x;
                            wallTransitioningHorizontalTimer = 0;
                            break;
                    }
                }
            }

            if (wallTransitioningHorizontally || wallTransitioningVertically)
            {
                if (wallTransitioningHorizontally)
                {
                    wallTransitioningHorizontalTimer += Time.deltaTime;

                    cameraPosition.x = (startingHorizontalCameraOffsetOnWallTransition) * (1 - wallTransitioningHorizontalTimer / WallTransitionTimerMaximum) + (TargetCameraX) * wallTransitioningHorizontalTimer / WallTransitionTimerMaximum;

                    if (wallTransitioningHorizontalTimer > WallTransitionTimerMaximum)
                    {
                        wallTransitioningHorizontally = false;
                    }
                }
                else
                {
                    cameraPosition.x = TargetCameraX;
                }

                if (wallTransitioningVertically)
                {
                    wallTransitioningVerticalTimer += Time.deltaTime;

                    cameraPosition.y = (startingVerticalCameraOffsetOnWallTransition) * (1 - wallTransitioningVerticalTimer / WallTransitionTimerMaximum) + (TargetCameraY) * wallTransitioningVerticalTimer / WallTransitionTimerMaximum;

                    if (wallTransitioningVerticalTimer > WallTransitionTimerMaximum)
                    {
                        wallTransitioningVertically = false;
                    }
                }
                else
                {
                    cameraPosition.y = TargetCameraY;
                }
            }
            else
            {
                cameraPosition.x = TargetCameraX;
                cameraPosition.y = TargetCameraY;
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

        // If no longer moving in the same direction update which way is being faced and set the timer to the maximum
        if (currentlyFacingRight != previouslyFacingRight)
        {
            previouslyFacingRight = currentlyFacingRight;

            changingDirection = true;
            changingDirectionTimer = 0;
            changingDirectionRelativeOrigin = horizontalCameraPosition - HorizontalPlayerPosition;
            changingDirectionTarget = playerGameObject.transform.position.x + horizontalDirection * CameraLag;
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

    private float CalculateTargetCameraPositionFromBoundaries(float LesserBound, float GreaterBound, float CameraSizeInAxis, float CameraPositionInAxis)
    {
        float targetCameraPosition = CameraPositionInAxis;

        if (Mathf.Abs(LesserBound) != Mathf.Infinity && Mathf.Abs(GreaterBound) != Mathf.Infinity)
        {
            targetCameraPosition = (LesserBound + GreaterBound) / 2;
        }
        else if (Mathf.Abs(LesserBound) != Mathf.Infinity && Mathf.Abs(LesserBound) < Mathf.Abs(CameraPositionInAxis - CameraSizeInAxis))
        {
            targetCameraPosition = LesserBound + CameraSizeInAxis;
        }
        else if (Mathf.Abs(GreaterBound) != Mathf.Infinity && Mathf.Abs(GreaterBound) < Mathf.Abs(CameraPositionInAxis + CameraSizeInAxis))
        {
            targetCameraPosition = GreaterBound - CameraSizeInAxis;
        }

        return targetCameraPosition;
    }

    public void KnockbackCamera(bool KnockbackDelay)
    { 
        if (!KnockbackDelay)
        {
            justKnockedBackTimer = JustKnockedBackDelay;
        }

        justKnockedBack = true;
        startingCameraPositionOnKnockback = gameObject.transform.position;
    }
    
    public void FixCamera(Vector3 position)
    {
        currentlyFixed = true;

        gameObject.transform.position = position;
    }

    public void UnfixCamera()
    {
        currentlyFixed = false;
    }
}
