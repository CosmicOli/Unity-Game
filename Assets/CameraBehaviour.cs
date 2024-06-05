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

    public float CameraLag;

    private float horizontalVelocityTimer;
    private float HorizontalVelocityTimerMaximum = 0.2f;

    private bool changingDirection;
    private float changingDirectionTimer;
    private float ChangingDirectionTimerMaximum = 0.2f;
    private float changingDirectionLag;

    private bool changingDirectionOnWrongSide = false;

    private bool currentlyFixed;
    
    private bool previouslyFacingRight;

    private float previousHorizontalPosition = 0;

    float maxTurnAroundOffset = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        playerBehaviour = playerGameObject.GetComponent<PlayerBehaviour>();
        playerRigidBody = playerGameObject.GetComponent<Rigidbody2D>();

        previouslyFacingRight = playerBehaviour.isFacingRight;

        previousHorizontalPosition = gameObject.transform.position.x;
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

            Debug.Log(RelativePosition.x);

            if (Mathf.Abs(playerRigidBody.velocity.x) > 0)
            {
                horizontalVelocityTimer += Time.deltaTime;

                if (horizontalVelocityTimer > HorizontalVelocityTimerMaximum)
                {
                  horizontalVelocityTimer = HorizontalVelocityTimerMaximum;
                }
            }
            else
            {
                horizontalVelocityTimer -= Time.deltaTime;

                if (horizontalVelocityTimer < 0)
                {
                    horizontalVelocityTimer = 0;
                }
            }

            //cameraPosition.x = TrackObjectHorizontally(PlayerPosition.x, horizontalVelocityTimer, playerRigidBody.velocity.x);
            cameraPosition.x = PlayerPosition.x;

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

            bool currentlyFacingRight = playerBehaviour.isFacingRight;

            if (currentlyFacingRight != previouslyFacingRight)
            {
                previouslyFacingRight = currentlyFacingRight;
            }

            float turnAroundOffset = 0;
            cameraPosition.x += turnAroundOffset;


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

    private float TrackObjectHorizontally(float HorizontalPlayerPosition, float HorizontalVelocityTimer, float HorizontalVelocity)
    {
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

            timeFactor = (direction * CameraLag + speedFactor) * HorizontalVelocityTimer / HorizontalVelocityTimerMaximum;

            if (Mathf.Abs(timeFactor) >= CameraLag + direction * speedFactor)
            {
                timeFactor = Mathf.Sign(timeFactor) * (CameraLag + direction * speedFactor);
            }
        }

        float defaultRelativePlayerAxisPosition = direction * CameraLag;

        float horizontalCameraPosition = HorizontalPlayerPosition + defaultRelativePlayerAxisPosition - timeFactor;

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
