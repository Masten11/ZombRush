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
    public float fallMultiplier = 1.8f; // NEW: Pulls you down fast when you fall off ledges
    public float groundCheckBuffer = 0.4f; // NEW: Prevents ramp "quicksand" stutter

    public float apexThreshold = 2f; // NEW: Gravity kicks in when upward speed drops below this
    [Header("UI & Timer")]
    public Text timerText;
    private float startTime;

    private float speed;

    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private Animator anim;

    private bool isGrounded;
    private bool jumped = false; // Tracks if we intentionally jumped
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
        // 1. UPDATE GROUND STATE FIRST
        UpdateGroundedState();

        if (timerText != null)
        {
            float t = Time.time - startTime;
            string minutes = ((int)t / 60).ToString("00");
            string seconds = (t % 60).ToString("00");
            timerText.text = minutes + ":" + seconds;
        }

        // Progressiv fartökning
        speed = Mathf.MoveTowards(speed, maxSpeed, accelerationRate * Time.deltaTime);

        // Hopp (Space / UpArrow / W)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && isGrounded && !isRolling)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            jumped = true; // We pressed jump!
            isSlammingDown = false;

            anim.SetTrigger("Jump");
        }

        // Roll (DownArrow / S) 
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

        // Filbyte (RightArrow/D, LeftArrow/A)
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
        
        // 1. Start the laser at the exact center of the Capsule, no matter where the pivot is.
        Vector3 rayStart = transform.position + originalCenter;
        
        // 2. Length is half the capsule height + your buffer to reach into the floor
        float rayLength = (originalHeight / 2f) + groundCheckBuffer;
        
        RaycastHit hit;
        bool hitGround = false;

        // 3. Shoot the laser and ONLY count it if it hits your specific tags
        if (Physics.Raycast(rayStart, Vector3.down, out hit, rayLength))
        {
            if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Plank") || hit.collider.CompareTag("Obstacle"))
            {
                hitGround = true;
            }
        }

        isGrounded = hitGround;

        // Update Animator and States if our grounding changed
        if (isGrounded && !wasGrounded)
        {
            anim.SetBool("isGrounded", true);
            jumped = false; // Reset intentional jump when we land
            isSlammingDown = false; // Reset the air slam!
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

        // User's Air Slam limit
        if (isSlammingDown && y < -maxDownSpeed)
        {
            y = -maxDownSpeed;
        }

        // IF WE RAN OFF A LEDGE OR RAMP (No intentional jump)
        if (!isGrounded && !jumped)
        {
            // 1. KILL THE UPWARD LAUNCH
            // If the ramp threw us up into the air, instantly flatten our trajectory.
            if (y > 0)
            {
                y = 0f; 
            }

            // 2. THE SMOOTH FALL
            // Apply the extra gravity to pull us down. 
            y += Physics.gravity.y * fallMultiplier * Time.fixedDeltaTime;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, y, speed);
    }
    void OnCollisionEnter(Collision col)
    {
        // Grounding is handled completely by UpdateGroundedState now. 
        // This is ONLY for hitting a wall and Game Over.
        if (col.gameObject.CompareTag("Obstacle"))
        {
            if (col.contacts[0].normal.y <= 0.5f)
            {
                // We hit the side or the front. Game over.
                float t = Time.time - startTime;

                if (LeaderboardManager.Instance != null)
                    LeaderboardManager.Instance.SubmitScore("Player", t);

                GetComponent<PlayerAudio>()?.DieWithSounds();
            }
        }
    }

    public bool IsGrounded => isGrounded;
    public float ForwardSpeed => rb != null ? rb.linearVelocity.z : 0f;
}