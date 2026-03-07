using UnityEngine;
using System.Collections;
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

    [Header("Double jump power up")]
    public float doubleJumpDuration = 10f;
    private bool canDoubleJump = false;
    private bool hasDoubleJumped = false;

    [Header("Smash Power-Up")]
    public float smashDuration = 8f;
    public float smashForce = 40f;
    private bool isInvincible = false;

    private float speed;

    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private Animator anim;

    private bool isGrounded;
    private bool jumped = false;
    private bool isRolling = false;
    private int currentLane = 1;

    private float originalHeight;
    private Vector3 originalCenter;

    private bool isSlammingDown = false;

    private bool isDead = false;
    private bool canDie = false;

    private ZombieFollower zombieFollower;
    private PlayerAudio playerAudio;

    void Start()
    {
        startTime = Time.time;
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        anim = GetComponentInChildren<Animator>();

        originalHeight = playerCollider.height;
        originalCenter = playerCollider.center;

        speed = startSpeed;

        playerAudio = GetComponent<PlayerAudio>();
        zombieFollower = FindFirstObjectByType<ZombieFollower>();

        StartCoroutine(EnableDeathAfterDelay());
    }

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

        speed = Mathf.MoveTowards(speed, maxSpeed, accelerationRate * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (isGrounded && !isRolling)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                jumped = true;
                isSlammingDown = false;
                hasDoubleJumped = false;

                if (anim != null)
                    anim.SetTrigger("Jump");
            }
            else if (!isGrounded && canDoubleJump && !hasDoubleJumped && !isRolling)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                hasDoubleJumped = true;
                isSlammingDown = false;

                if (anim != null)
                    anim.Play("Jump", -1, 0f);
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (isGrounded && !isRolling)
            {
                StartCoroutine(PerformRoll());
            }
            else if (!isGrounded && !isRolling)
            {
                isSlammingDown = true;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Min(0f, rb.linearVelocity.y), rb.linearVelocity.z);
                rb.AddForce(Vector3.down * slamDownForce, ForceMode.Impulse);
            }
        }

        if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && currentLane < 2)
            currentLane++;

        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && currentLane > 0)
            currentLane--;

        float targetX = (currentLane - 1) * laneDistance;
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * sideSpeed);
        transform.position = newPos;
    }

    private void UpdateGroundedState()
    {
        bool wasGrounded = isGrounded;

        Vector3 rayStart = transform.position + originalCenter;
        float rayLength = (originalHeight / 2f) + groundCheckBuffer;

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

        if (isGrounded && !wasGrounded)
        {
            if (anim != null)
                anim.SetBool("isGrounded", true);

            jumped = false;
            isSlammingDown = false;
            hasDoubleJumped = false;
        }
        else if (!isGrounded && wasGrounded)
        {
            if (anim != null)
                anim.SetBool("isGrounded", false);
        }
    }

    IEnumerator PerformRoll()
    {
        isRolling = true;

        if (anim != null)
            anim.SetTrigger("Roll");

        playerCollider.height = originalHeight * rollHeightMultiplier;
        playerCollider.center = new Vector3(originalCenter.x, originalCenter.y / 2f, originalCenter.z);

        yield return new WaitForSeconds(0.4f);

        playerCollider.height = originalHeight;
        playerCollider.center = originalCenter;

        isRolling = false;
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        float y = rb.linearVelocity.y;

        if (isSlammingDown && y < -maxDownSpeed)
        {
            y = -maxDownSpeed;
        }

        if (!isGrounded && !jumped)
        {
            if (y > 0)
            {
                y = 0f;
            }

            y += Physics.gravity.y * fallMultiplier * Time.fixedDeltaTime;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, y, speed);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        float t = Time.time - startTime;

        if (LeaderboardManager.Instance != null)
            LeaderboardManager.Instance.SubmitScore("Player", t);

        if (playerAudio != null)
            playerAudio.MarkDead();

        speed = 0f;
        isRolling = false;
        isSlammingDown = false;
        rb.linearVelocity = Vector3.zero;

        if (anim != null)
            anim.SetBool("isGrounded", true);

        if (zombieFollower != null)
            zombieFollower.StartKillSequence();
        else if (playerAudio != null)
            playerAudio.LoadGameOver();
    }

    void OnCollisionEnter(Collision col)
    {
        if (isDead || !canDie) return;

        if (col.gameObject.CompareTag("Obstacle"))
        {
            if (isInvincible)
            {
                Rigidbody obsRb = col.gameObject.GetComponent<Rigidbody>();
                if (obsRb == null)
                {
                    obsRb = col.gameObject.AddComponent<Rigidbody>();
                }

                obsRb.isKinematic = false;

                Vector3 blastDirection = transform.forward + Vector3.up;
                obsRb.AddForce(blastDirection * smashForce, ForceMode.Impulse);
                obsRb.AddTorque(
                    new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)) * smashForce,
                    ForceMode.Impulse
                );

                col.collider.enabled = false;
            }
            else if (col.contactCount > 0 && col.contacts[0].normal.y <= 0.5f)
            {
                Die();
            }
        }
    }

    public void ActivateDoubleJump()
    {
        StopCoroutine("DoubleJumpTimer");
        StartCoroutine("DoubleJumpTimer");
    }

    private IEnumerator DoubleJumpTimer()
    {
        canDoubleJump = true;
        yield return new WaitForSeconds(doubleJumpDuration);
        canDoubleJump = false;
    }

    public void ActivateSmashMode()
    {
        StopCoroutine("SmashTimer");
        StartCoroutine("SmashTimer");
    }

    private IEnumerator SmashTimer()
    {
        isInvincible = true;
        yield return new WaitForSeconds(smashDuration);
        isInvincible = false;
    }

    public bool IsGrounded => isGrounded;
    public float ForwardSpeed => rb != null ? rb.linearVelocity.z : 0f;
}