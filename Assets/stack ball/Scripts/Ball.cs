using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallState
{
    Prepare,
    Playing,
    Died,
    Finish
}

public class Ball : MonoBehaviour
{
    public Rigidbody rb;
    public BallState ballState = BallState.Prepare;

    public bool smash = false;
    public bool invincible = false;
    public float currentTime = 0f;

    [Header("Effects & Visuals")]
    public GameObject shatterParticle; // Particle System.prefab
    public GameObject splashPrefab;    // Splash.prefab
    public GameObject winParticle;     // Win Particle.prefab
    public GameObject fireEffect;      // Fire particle effect GameObject
    public MeshRenderer ballMesh;
    public TrailRenderer trailRenderer;

    [Header("Color Palette per Level")]
    public Color[] levelColors = new Color[]
    {
        new Color(1f, 0.6f, 0f),     // Vibrant Orange
        new Color(0.1f, 0.8f, 0.3f),  // Bright Green
        new Color(0.9f, 0.1f, 0.5f),  // Pink / Magenta
        new Color(0f, 0.75f, 1f),     // Cyan Blue
        new Color(1f, 0.85f, 0.1f),   // Gold / Yellow
        new Color(0.6f, 0.2f, 0.9f)   // Purple
    };

    [HideInInspector]
    public Color currentLevelColor;
    public Color invincibleBallColor = Color.red;

    [Header("Audio Sounds")]
    public AudioSource audioSource;
    public AudioClip bounceClip;      // Balljump.mp3
    public AudioClip breakClip;       // BallBreakStack.wav
    public AudioClip deathClip;       // BallDied.mp3
    public AudioClip winClip;         // BallLevel.mp3
    public AudioClip fireIgniteClip;

    [Header("Audio Pitch Controls (Inspector Sliders)")]
    [Range(0.2f, 1.0f)] public float minPitch = 0.85f; // Minimum pitch slider
    [Range(1.0f, 2.5f)] public float maxPitch = 1.25f; // Maximum pitch slider

    private bool winEffectSpawned = false;
    private Material ballMat;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (ballMesh == null) ballMesh = GetComponent<MeshRenderer>();
        if (trailRenderer == null) trailRenderer = GetComponent<TrailRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (ballMesh != null)
        {
            ballMat = ballMesh.material;
        }

        // Pick color based on current level
        int level = PlayerPrefs.GetInt("Level", 1);
        if (levelColors != null && levelColors.Length > 0)
        {
            currentLevelColor = levelColors[(level - 1) % levelColors.Length];
        }
        else
        {
            currentLevelColor = new Color(1f, 0.6f, 0f);
        }

        ApplyBallColor(currentLevelColor);
    }

    void Start()
    {
        if (fireEffect != null)
        {
            fireEffect.SetActive(false);
        }
    }

    void ApplyBallColor(Color color)
    {
        if (ballMat != null)
        {
            ballMat.color = color;
        }

        if (trailRenderer != null)
        {
            trailRenderer.startColor = color;
            Color solidTrail = color;
            solidTrail.a = 0.85f;
            trailRenderer.endColor = solidTrail;
        }
    }

    void Update()
    {
        // 1. PREPARE
        if (ballState == BallState.Prepare)
        {
            if (Input.GetMouseButtonDown(1))
            {
                ballState = BallState.Playing;
                smash = true;
            }
        }

        // 2. PLAYING
        if (ballState == BallState.Playing)
        {
            if (Input.GetMouseButtonDown(1))
            {
                smash = true;
            }

            if (Input.GetMouseButtonUp(1))
            {
                smash = false;
            }

            // Charge meter when holding smash
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

            // Invincibility triggers
            if (currentTime >= 1f)
            {
                currentTime = 1f;
                if (!invincible) ActivateFireMode(true);
            }
            else if (currentTime <= 0f)
            {
                currentTime = 0f;
                if (invincible) ActivateFireMode(false);
            }
        }

        // 3. FINISH
        if (ballState == BallState.Finish)
        {
            if (fireEffect != null) fireEffect.SetActive(false);

            if (Input.GetMouseButtonDown(1))
            {
                LevelSpawner spawner = Object.FindFirstObjectByType<LevelSpawner>();
                if (spawner != null)
                {
                    spawner.NextLevel();
                }
            }
        }
    }

    void ActivateFireMode(bool active)
    {
        invincible = active;

        if (fireEffect != null)
        {
            fireEffect.SetActive(active);

            if (active)
            {
                ParticleSystem[] ps = fireEffect.GetComponentsInChildren<ParticleSystem>();
                foreach (var p in ps)
                {
                    p.Clear();
                    p.Play();
                }
            }
        }

        ApplyBallColor(active ? invincibleBallColor : currentLevelColor);

        if (active && fireIgniteClip != null)
        {
            PlaySound(fireIgniteClip);
        }
    }

    void FixedUpdate()
    {
        if (ballState == BallState.Playing && smash)
        {
            rb.linearVelocity = new Vector3(0, -700f * Time.fixedDeltaTime, 0);
        }

        if (rb.linearVelocity.y > 5f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 5f, rb.linearVelocity.z);
        }
    }

    void OnCollisionEnter(Collision target)
    {
        // Finish platform check
        if (target.gameObject.CompareTag("Finish") || LayerMask.LayerToName(target.gameObject.layer) == "Finish")
        {
            ballState = BallState.Finish;
            smash = false;
            rb.linearVelocity = new Vector3(0, 250f * Time.fixedDeltaTime, 0);

            ActivateFireMode(false);

            if (!winEffectSpawned)
            {
                winEffectSpawned = true;
                PlaySound(winClip);

                if (winParticle != null)
                {
                    Instantiate(winParticle, transform.position, Quaternion.identity);
                }

                if (GameUI.instance != null)
                {
                    GameUI.instance.ShowWin();
                }
            }
            return;
        }

        // Prepare bounce
        if (ballState == BallState.Prepare)
        {
            rb.linearVelocity = new Vector3(0, 250f * Time.fixedDeltaTime, 0);
            PlaySound(bounceClip);
            SpawnSplash(target);
            return;
        }

        if (ballState != BallState.Playing) return;

        // Check types via Layer and Tag
        bool isBreakable = target.gameObject.CompareTag("enemy") || LayerMask.LayerToName(target.gameObject.layer) == "Breakable";
        bool isObstacle = target.gameObject.CompareTag("plane") || LayerMask.LayerToName(target.gameObject.layer) == "Obstacle";

        if (!smash)
        {
            rb.linearVelocity = new Vector3(0, 250f * Time.fixedDeltaTime, 0);
            PlaySound(bounceClip);
            SpawnSplash(target);
        }
        else
        {
            if (isBreakable)
            {
                BreakStack(target);
                PlayBreakSoundWithRandomPitch();
            }

            if (invincible && isObstacle)
            {
                BreakStack(target);
                PlayBreakSoundWithRandomPitch();
            }

            if (!invincible && isObstacle)
            {
                Die();
            }
        }
    }

    void OnCollisionStay(Collision target)
    {
        if (ballState == BallState.Prepare || ballState == BallState.Finish || (ballState == BallState.Playing && !smash))
        {
            rb.linearVelocity = new Vector3(0, 250f * Time.fixedDeltaTime, 0);
        }
    }

    void SpawnSplash(Collision target)
    {
        if (splashPrefab == null) return;
        if (target.gameObject.CompareTag("Finish")) return;

        Vector3 spawnPosition;

        if (target.contacts != null && target.contacts.Length > 0)
        {
            spawnPosition = target.contacts[0].point;
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
            {
                spawnPosition = hit.point;
            }
            else
            {
                spawnPosition = new Vector3(transform.position.x, target.transform.position.y + 0.25f, transform.position.z);
            }
        }

        spawnPosition.y += 0.015f;

        GameObject splash = Instantiate(
            splashPrefab,
            spawnPosition,
            Quaternion.Euler(90f, 0f, Random.Range(0f, 360f))
        );

        // Tint splash to match current level ball color
        SpriteRenderer sr = splash.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = currentLevelColor;
        }
        else
        {
            MeshRenderer mr = splash.GetComponent<MeshRenderer>();
            if (mr != null) mr.material.color = currentLevelColor;
        }

        splash.transform.SetParent(target.transform);
    }

    void BreakStack(Collision target)
    {
        StackController stackController = target.gameObject.GetComponentInParent<StackController>();
        if (stackController != null)
        {
            stackController.ShatterAllParts();
        }
    }

    public void IncreaseBrokenStacks()
    {
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(invincible ? 2 : 1);
        }
    }

    void Die()
    {
        ballState = BallState.Died;
        smash = false;
        rb.isKinematic = true;

        ActivateFireMode(false);

        if (ballMesh != null)
        {
            ballMesh.enabled = false;
        }

        PlaySound(deathClip);

        if (shatterParticle != null)
        {
            Instantiate(shatterParticle, transform.position, Quaternion.identity);
        }

        if (GameUI.instance != null)
        {
            GameUI.instance.ShowGameOver();
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = 1.0f; // Fixed pitch for normal sounds
            audioSource.PlayOneShot(clip);
        }
    }

    // Random pitch variation based on minPitch and maxPitch sliders
    void PlayBreakSoundWithRandomPitch()
    {
        if (audioSource != null && breakClip != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(breakClip);
        }
    }
}