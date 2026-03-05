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

    // --- NEW: Power-Up Variables ---
    [Header("Double jump power uo")]
    public float doubleJumpDuration = 10f; // How many seconds the power-up lasts
    private bool canDoubleJump = false;    // Is the powerup currently active?
    private bool hasDoubleJumped = false;  // Has the player already used their second jump in the air?


    // --- Juggernaut Power-Up ---
    [Header("Smash Power-Up")]
    public float smashDuration = 8f;     // How long you are invincible
    public float smashForce = 40f;       // How hard the obstacles get hit
    private bool isInvincible = false;   // Are we in Smash Mode?
    // --------------------------------
    // -------------------------------

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

    void Start()
    {
        startTime = Time.time;
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        anim = GetComponentInChildren<Animator>();

        originalHeight = playerCollider.height;
        originalCenter = playerCollider.center;

        speed = startSpeed;
    }

    void Update()
    {
        UpdateGroundedState();

        if (timerText != null)
        {
            float t = Time.time - startTime;
            string minutes = ((int)t / 60).ToString("00");
            string seconds = (t % 60).ToString("00");
            timerText.text = minutes + ":" + seconds;
        }

        speed = Mathf.MoveTowards(speed, maxSpeed, accelerationRate * Time.deltaTime);

        // --- UPDATED: Jump Logic to handle Double Jumps ---
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            // NORMAL JUMP (From the ground)
            if (isGrounded && !isRolling)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                
                jumped = true; 
                isSlammingDown = false;
                hasDoubleJumped = false; // Reset the double jump token

                anim.SetTrigger("Jump");
            }
            // DOUBLE JUMP (From the air)
            else if (!isGrounded && canDoubleJump && !hasDoubleJumped && !isRolling)
            {
                // Reset the Y velocity to 0 first so the second jump is always exactly as high as the first
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                
                hasDoubleJumped = true; // Mark that we used the air jump
                isSlammingDown = false;
                
                // Play the jump animation again in the air!
                anim.Play("Jump", -1, 0f); 
            }
        }
        // ---------------------------------------------------

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

        if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && currentLane < 2) currentLane++;
        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && currentLane > 0) currentLane--;

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
            anim.SetBool("isGrounded", true);
            jumped = false; 
            isSlammingDown = false; 
            hasDoubleJumped = false; // --- NEW: Reset double jump when we hit the floor ---
        }
        else if (!isGrounded && wasGrounded)
        {
            anim.SetBool("isGrounded", false);
        }
    }

    IEnumerator PerformRoll()
    {
        isRolling = true;
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
    
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Obstacle"))
        {
            // --- SCENARIO 1: We have the Super Power! SMASH IT! ---
            if (isInvincible)
            {
                // 1. Get the Rigidbody of the obstacle, or add one instantly if it doesn't have one
                Rigidbody obsRb = col.gameObject.GetComponent<Rigidbody>();
                if (obsRb == null) 
                {
                    obsRb = col.gameObject.AddComponent<Rigidbody>();
                }

                // 2. Turn on physics
                obsRb.isKinematic = false;

                // 3. Blast it away and slightly upwards so it flies over our head!
                Vector3 blastDirection = transform.forward + Vector3.up; 
                obsRb.AddForce(blastDirection * smashForce, ForceMode.Impulse);
                obsRb.AddTorque(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)) * smashForce, ForceMode.Impulse);

                // 4. Turn off the obstacle's collider so it doesn't bounce back and hit us
                col.collider.enabled = false; 
            }
            // --- SCENARIO 2: No Super Power. Game Over. ---
            else if (col.contacts[0].normal.y <= 0.5f)
            {
                float t = Time.time - startTime;

                if (LeaderboardManager.Instance != null)
                    LeaderboardManager.Instance.SubmitScore("Player", t);

                GetComponent<PlayerAudio>()?.DieWithSounds();
            }
        }
    }

    // --- NEW: Power-Up Activation Methods ---
    public void ActivateDoubleJump()
    {
        // Stop any existing timer so grabbing two powerups back-to-back resets the clock
        StopCoroutine("DoubleJumpTimer"); 
        StartCoroutine("DoubleJumpTimer");
    }

    private IEnumerator DoubleJumpTimer()
    {
        canDoubleJump = true;
        
        // Wait for X seconds
        yield return new WaitForSeconds(doubleJumpDuration);
        
        canDoubleJump = false;
    }

    // --- NEW: Activate the Smash Power! ---
    public void ActivateSmashMode()
    {
        StopCoroutine("SmashTimer"); 
        StartCoroutine("SmashTimer");
    }

    private IEnumerator SmashTimer()
    {
        isInvincible = true;
        
        // Wait for X seconds
        yield return new WaitForSeconds(smashDuration);
        
        isInvincible = false;
    }


    public bool IsGrounded => isGrounded;
    public float ForwardSpeed => rb != null ? rb.linearVelocity.z : 0f;
}