using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    PlayerBehaviour playerBehaviour;
    GameObject playerGameObject;

    public Camera Camera;

    public float CameraLag;

    bool currentlyFixed;

    // Start is called before the first frame update
    void Start()
    {
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        playerBehaviour = playerGameObject.GetComponent<PlayerBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        // If not currently fixed, trail the player
        if (!currentlyFixed)
        {
            float topBound = Mathf.Infinity;
            float bottomBound = Mathf.NegativeInfinity;
            float leftBound = Mathf.NegativeInfinity;
            float rightBound = Mathf.Infinity;

            Vector3 CameraPosition = gameObject.transform.position;
            Vector2 relativePosition = playerGameObject.transform.position - CameraPosition;

            if (Mathf.Abs(relativePosition.x) > CameraLag || Mathf.Abs(relativePosition.y) > CameraLag)
            {
                // Follow the player
                // 

                if (Mathf.Abs(relativePosition.x) > CameraLag)
                {
                    CameraPosition.x = playerGameObject.transform.position.x - Mathf.Sign(relativePosition.x) * CameraLag;
                }

                if (Mathf.Abs(relativePosition.y) > CameraLag)
                {
                    CameraPosition.y = playerGameObject.transform.position.y - Mathf.Sign(relativePosition.y) * CameraLag;
                }

                float CameraHeight = Camera.orthographicSize;
                float CameraWidth = Camera.orthographicSize * 16 / 9;

                topBound = CalculateVerticalCameraBound(Vector2.up, CameraHeight);
                bottomBound = CalculateVerticalCameraBound(Vector2.down, CameraHeight);
                leftBound = CalculateHorizontalCameraBound(Vector2.left, CameraWidth);
                rightBound = CalculateHorizontalCameraBound(Vector2.right, CameraWidth);

                if (Mathf.Abs(leftBound) != Mathf.Infinity && Mathf.Abs(rightBound) != Mathf.Infinity)
                {
                    CameraPosition.x = (leftBound + rightBound) / 2;
                }
                else if (Mathf.Abs(leftBound) != Mathf.Infinity && Mathf.Abs(leftBound) < Mathf.Abs(CameraPosition.x - CameraWidth))
                {
                    CameraPosition.x = leftBound + CameraWidth;
                }
                else if (Mathf.Abs(rightBound) != Mathf.Infinity && Mathf.Abs(rightBound) < Mathf.Abs(CameraPosition.x + CameraWidth))
                {
                    CameraPosition.x = rightBound - CameraWidth;
                }

                if (Mathf.Abs(bottomBound) != Mathf.Infinity && Mathf.Abs(topBound) != Mathf.Infinity)
                {
                    CameraPosition.y = (bottomBound + topBound) / 2;
                }
                else if (Mathf.Abs(bottomBound) != Mathf.Infinity && Mathf.Abs(bottomBound) < Mathf.Abs(CameraPosition.y - CameraHeight))
                {
                    CameraPosition.y = bottomBound + CameraHeight;
                }
                else if (Mathf.Abs(topBound) != Mathf.Infinity && Mathf.Abs(topBound) < Mathf.Abs(CameraPosition.y + CameraHeight))
                {
                    CameraPosition.y = topBound - CameraHeight;
                }

                gameObject.transform.position = CameraPosition;
            }

            // Let the player move briefly to convey movement 
            // Then lock the camera relative to the player
        }
    }

    public float CalculateHorizontalCameraBound(Vector2 BoundDirection, float CameraWidth)
    {
        RaycastHit2D[] hitObjects = Physics2D.RaycastAll(playerGameObject.transform.position, BoundDirection, CameraWidth, LayerMask.GetMask("Floors and Walls"));

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

    public float CalculateVerticalCameraBound(Vector2 BoundDirection, float CameraHeight)
    {
        RaycastHit2D[] hitObjects = Physics2D.RaycastAll(playerGameObject.transform.position, BoundDirection, CameraHeight, LayerMask.GetMask("Floors and Walls"));

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
