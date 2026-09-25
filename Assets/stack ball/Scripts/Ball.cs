using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState
{
    Prepare,
    Playing,
    Finish
}

public class Ball : MonoBehaviour
{
    public Rigidbody rb;

    public BallState ballState = BallState.Prepare;

    public bool smash = false;
    public bool invincible = false;

    public float currentTime = 0f;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        // PLAYING
        if (ballState == BallState.Playing)
        {
            if (Input.GetMouseButtonDown(0))
                smash = true;

            if (Input.GetMouseButtonUp(0))
                smash = false;

            // Invincibility meter
            if (invincible)
            {
                currentTime -= Time.deltaTime * 0.35f;
            }
            else
            {
                if (smash)
                    currentTime += Time.deltaTime * 0.8f;
                else
                    currentTime -= Time.deltaTime * 0.5f;
            }

            // Become invincible
            if (currentTime >= 1)
            {
                currentTime = 1;
                invincible = true;
            }

            // Lose invincibility
            if (currentTime <= 0)
            {
                currentTime = 0;
                invincible = false;
            }
        }

        // PREPARE
        if (ballState == BallState.Prepare)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ballState = BallState.Playing;
            }
        }

        // FINISH
        if (ballState == BallState.Finish)
        {
            if (Input.GetMouseButtonDown(0))
            {
                LevelSpawner spawner = FindObjectOfType<LevelSpawner>();

                if (spawner != null)
                {
                    spawner.NextLevel();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (ballState == BallState.Playing)
        {
            if (Input.GetMouseButton(0))
            {
                smash = true;

                rb.linearVelocity = new Vector3(
                    0,
                    -100 * Time.fixedDeltaTime * 7,
                    0
                );
            }
        }

        // Limit upward speed
        if (rb.linearVelocity.y > 5)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                5,
                rb.linearVelocity.z
            );
        }
    }

    public void IncreaseBrokenStacks()
    {
        // IMPORTANT:
        // Check if ScoreManager actually exists
        if (ScoreManager.instance == null)
        {
            Debug.LogWarning("ScoreManager.instance is NULL! Add a ScoreManager to the scene.");
            return;
        }

        if (!invincible)
        {
            ScoreManager.instance.AddScore(1);
        }
        else
        {
            ScoreManager.instance.AddScore(2);
        }
    }

    void OnCollisionEnter(Collision target)
    {
        // Normal bounce
        if (!smash)
        {
            rb.linearVelocity = new Vector3(
                0,
                50 * Time.deltaTime * 5,
                0
            );
        }
        else
        {
            // Smash collision
            if (target.gameObject.CompareTag("enemy"))
            {
                BreakStack(target);
            }

            // Invincible ball can also break plane
            if (invincible && target.gameObject.CompareTag("plane"))
            {
                BreakStack(target);
            }

            // If not invincible and hit plane
            if (!invincible && target.gameObject.CompareTag("plane"))
            {
                Debug.Log("Over");

                // Add your Game Over code here later
            }
        }

        // Finish
        if (target.gameObject.CompareTag("Finish") &&
            ballState == BallState.Playing)
        {
            ballState = BallState.Finish;
        }
    }

    void BreakStack(Collision target)
    {
        // Find StackController in parent
        StackController stackController =
            target.gameObject.GetComponentInParent<StackController>();

        if (stackController != null)
        {
            stackController.ShatterAllParts();

            IncreaseBrokenStacks();
        }
        else
        {
            Debug.LogWarning(
                "StackController not found on " +
                target.gameObject.name
            );
        }
    }

    void OnCollisionStay(Collision target)
    {
        if (!smash || target.gameObject.CompareTag("Finish"))
        {
            rb.linearVelocity = new Vector3(
                0,
                50 * Time.deltaTime * 5,
                0
            );
        }
    }
}