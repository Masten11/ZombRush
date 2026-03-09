using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speed Progression")]
    public float startSpeed = 25f;
    public float maxSpeed = 100f;
    public float accelerationRate = 0.8f;

    public float jumpForce = 26f;
    public float laneDistance = 10f;
    public float sideSpeed = 15f;

    [Header("Roll Inställningar")]
    public float rollDuration = 0.65f;
    public float rollHeightMultiplier = 0.2f;

    [Header("Air Slam & Gravity")]
    public float slamDownForce = 40f;
    public float maxDownSpeed = 30f;
    public float fallMultiplier = 1.8f;
    public float groundCheckBuffer = 0.4f;
    public float apexThreshold = 2f;

    [Header("UI & Timer")]
    public Text timerText;
    private float startTime;

    [Header("Power-Up UI")]
    public Image doubleJumpBar; // The Blue Bar
    public Image smashBar;      // The Green Bar

    [Header("Double jump power up")]
    public float doubleJumpDuration = 10f;
    private bool canDoubleJump = false;
    private bool hasDoubleJumped = false;

    [Header("Smash Power-Up")]
    public float smashDuration = 8f;
    public float smashForce = 40f;
    public float smashSizeMultiplier = 1.5f; 
    private bool isInvincible = false;
    private Vector3 originalScale; // Remembers normal size to shrink back later

    private float speed;
    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private Animator anim;

    // State trackers
    private bool isGrounded;
    private bool jumped = false;
    private bool isRolling = false;
    private int currentLane = 1; // Lanes: 0 (Left), 1 (Middle), 2 (Right)

    private float originalHeight;
    private Vector3 originalCenter;
    private bool isSlammingDown = false;

    private bool isDead = false;
    private bool canDie = false; // Brief invulnerability at start

    private ZombieFollower zombieFollower;
    private PlayerAudio playerAudio;

    void Start()
    {
        // 1. Setup references and save original player sizes
        startTime = Time.time;
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        anim = GetComponentInChildren<Animator>();

        originalHeight = playerCollider.height;
        originalCenter = playerCollider.center;
        originalScale = transform.localScale;
        speed = startSpeed;

        // 2. Make sure UI bars start empty
        if (doubleJumpBar != null) doubleJumpBar.fillAmount = 0f;
        if (smashBar != null) smashBar.fillAmount = 0f;

        playerAudio = GetComponent<PlayerAudio>();
        zombieFollower = FindFirstObjectByType<ZombieFollower>();

        StartCoroutine(EnableDeathAfterDelay());
    }

    // Prevents instant death as soon as the scene loads
    private IEnumerator EnableDeathAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        canDie = true;
    }

    void Update()
    {
        if (isDead) return;

        UpdateGroundedState();

        if (timerText != null)
        {
            float t = Time.time - startTime;
            string minutes = ((int)t / 60).ToString("00");
            string seconds = ((int)t % 60).ToString("00");
            timerText.text = minutes + ":" + seconds;
        }

        // Gradually increase running speed over time
        speed = Mathf.MoveTowards(speed, maxSpeed, accelerationRate * Time.deltaTime);

        // --- JUMP LOGIC ---
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (isGrounded && !isRolling) // Normal jump
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                jumped = true;
                isSlammingDown = false;
                hasDoubleJumped = false;

                if (anim != null) anim.SetTrigger("Jump");
            }
            else if (!isGrounded && canDoubleJump && !hasDoubleJumped && !isRolling) // Air double jump
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                hasDoubleJumped = true;
                isSlammingDown = false;

                if (anim != null) anim.Play("Jump", -1, 0f); // Restart animation
            }
        }

        // --- ROLL & SLAM LOGIC ---
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (isGrounded && !isRolling) // Roll on ground
            {
                StartCoroutine(PerformRoll());
            }
            else if (!isGrounded && !isRolling) // Slam down from air
            {
                isSlammingDown = true;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Min(0f, rb.linearVelocity.y), rb.linearVelocity.z);
                rb.AddForce(Vector3.down * slamDownForce, ForceMode.Impulse);
            }
        }

        // --- LANE SWITCHING LOGIC ---
        if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && currentLane < 2) currentLane++;
        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && currentLane > 0) currentLane--;

        // Smoothly glide character to the target lane
        float targetX = (currentLane - 1) * laneDistance;
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * sideSpeed);
        transform.position = newPos;
    }

    private void UpdateGroundedState()
    {
        bool wasGrounded = isGrounded;

        // Dynamically scale the laser math so it still works when the player is huge
        Vector3 currentCenter = originalCenter * transform.localScale.y;
        float currentHeight = originalHeight * transform.localScale.y;

        // --- FIXED BUG HERE: Replaced 'original' with 'current' ---
        Vector3 rayStart = transform.position + currentCenter; 
        float rayLength = (currentHeight / 2f) + groundCheckBuffer; 

        // Shoot laser down to check for floors/planks
        RaycastHit hit;
        bool hitGround = false;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, rayLength))
        {
            if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Plank") || hit.collider.CompareTag("Obstacle"))
            {
                hitGround = true;
            }
        }

        isGrounded = hitGround;

        // Update animations based on if we just landed or just fell
        if (isGrounded && !wasGrounded)
        {
            if (anim != null) anim.SetBool("isGrounded", true);
            jumped = false;
            isSlammingDown = false;
            hasDoubleJumped = false;
        }
        else if (!isGrounded && wasGrounded)
        {
            if (anim != null) anim.SetBool("isGrounded", false);
        }
    }

    // Shrinks the player's physical collider for a short time so they can slide under things
    IEnumerator PerformRoll()
    {
        isRolling = true;
        if (anim != null) anim.SetTrigger("Roll");

        playerCollider.height = originalHeight * rollHeightMultiplier;
        playerCollider.center = new Vector3(originalCenter.x, originalCenter.y / 2f, originalCenter.z);

        yield return new WaitForSeconds(0.4f);

        playerCollider.height = originalHeight;
        playerCollider.center = originalCenter;

        isRolling = false;
    }

    // Handles actual physics movement
    void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        float y = rb.linearVelocity.y;

        // Cap max falling speed during an air slam
        if (isSlammingDown && y < -maxDownSpeed) y = -maxDownSpeed;

        // Custom heavy gravity for running off a ledge without jumping
        if (!isGrounded && !jumped)
        {
            if (y > 0) y = 0f; // Kills upward ramp momentum to shoot straight forward
            y += Physics.gravity.y * fallMultiplier * Time.fixedDeltaTime;
        }

        // Apply constant forward speed alongside our current vertical speed
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, y, speed);
    }

    // Handles what happens when you hit a wall and die
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        float t = Time.time - startTime;
        if (LeaderboardManager.Instance != null) LeaderboardManager.Instance.SubmitScore("Player", t);
        if (playerAudio != null) playerAudio.MarkDead();

        // Stop all movement
        speed = 0f;
        isRolling = false;
        isSlammingDown = false;
        rb.linearVelocity = Vector3.zero;

        if (anim != null) anim.SetBool("isGrounded", true);

        // Trigger end sequences
        if (zombieFollower != null) zombieFollower.StartKillSequence();
        else if (playerAudio != null) playerAudio.LoadGameOver();
    }

    void OnCollisionEnter(Collision col)
    {
        if (isDead || !canDie) return;

        if (col.gameObject.CompareTag("Obstacle"))
        {
            // --- JUGGERNAUT / SMASH POWER-UP ACTIVE ---
            if (isInvincible)
            {
                // Give the static obstacle a physics body so it can fly away
                Rigidbody obsRb = col.gameObject.GetComponent<Rigidbody>();
                if (obsRb == null) obsRb = col.gameObject.AddComponent<Rigidbody>();
                obsRb.isKinematic = false;

                // Blast it forward and up, adding a random spin (torque)
                Vector3 blastDirection = transform.forward + Vector3.up;
                obsRb.AddForce(blastDirection * smashForce, ForceMode.Impulse);
                obsRb.AddTorque(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)) * smashForce, ForceMode.Impulse);

                col.collider.enabled = false; // Turn off hitboxes so it doesn't bounce back
            }
            // --- NO POWER-UP, NORMAL CRASH ---
            else if (col.contactCount > 0 && col.contacts[0].normal.y <= 0.5f)
            {
                Die(); // Normal.y check ensures we didn't just land on top of the obstacle
            }
        }
    }

    // ==========================================
    // --- POWER-UP COROUTINES & UI BARS ---
    // ==========================================

    public void ActivateDoubleJump()
    {
        StopCoroutine("DoubleJumpTimer"); // Resets timer if player grabs 2 back-to-back
        StartCoroutine("DoubleJumpTimer");
    }

    private IEnumerator DoubleJumpTimer()
    {
        canDoubleJump = true;
        float timeLeft = doubleJumpDuration;

        // Smoothly shrink the blue UI bar over time
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime; 
            if (doubleJumpBar != null) doubleJumpBar.fillAmount = timeLeft / doubleJumpDuration;
            yield return null; 
        }

        canDoubleJump = false;
        if (doubleJumpBar != null) doubleJumpBar.fillAmount = 0f;
    }

    public void ActivateSmashMode()
    {
        StopCoroutine("SmashTimer");
        StartCoroutine("SmashTimer");
    }

    private IEnumerator SmashTimer()
    {
        isInvincible = true;
        transform.localScale = originalScale * smashSizeMultiplier; // Grow big!
        float timeLeft = smashDuration;

        // Smoothly shrink the green UI bar over time
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            if (smashBar != null) smashBar.fillAmount = timeLeft / smashDuration;
            yield return null;
        }
        
        transform.localScale = originalScale; // Shrink back
        isInvincible = false;
        if (smashBar != null) smashBar.fillAmount = 0f;
    }

    public bool IsGrounded => isGrounded;
    public float ForwardSpeed => rb != null ? rb.linearVelocity.z : 0f;
}