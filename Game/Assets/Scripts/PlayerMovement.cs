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

    [Header("Air Slam")]
    public float slamDownForce = 60f;   // higher = faster drop
    public float maxDownSpeed = 50f;    // cap fall speed while slamming

    [Header("UI & Timer")]
    public Text timerText;
    private float startTime;

    private float speed;

    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private Animator anim;

    private bool isGrounded;
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
            // FIX: Reset the Y (vertical) velocity to 0 before jumping.
            // We keep the X and Z velocities exactly as they are so we don't lose forward speed.
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Now apply the jump force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            isSlammingDown = false;

            anim.SetTrigger("Jump");
            anim.SetBool("isGrounded", false);
        }

        // Roll (DownArrow / S)  -> Ground roll, Air slam only
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (isGrounded && !isRolling)
            {
                StartCoroutine(PerformRoll());
            }
            else if (!isGrounded && !isRolling)
            {
                // In air: just drop fast (no roll after)
                isSlammingDown = true;

                // Kill upward velocity so we start falling immediately
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    Mathf.Min(0f, rb.linearVelocity.y),
                    rb.linearVelocity.z
                );

                // Add a strong downward impulse
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

    IEnumerator PerformRoll()
    {
        isRolling = true;

        anim.SetTrigger("Roll");

        // Sänk collider direkt
        playerCollider.height = originalHeight * rollHeightMultiplier;
        playerCollider.center = new Vector3(originalCenter.x, originalCenter.y / 2f, originalCenter.z);

        yield return new WaitForSeconds(0.4f);

        // Återställ collider
        playerCollider.height = originalHeight;
        playerCollider.center = originalCenter;

        isRolling = false;
    }

    void FixedUpdate()
    {
        // keep forward speed, optionally cap fall speed when slamming
        float y = rb.linearVelocity.y;

        if (isSlammingDown && y < -maxDownSpeed)
            y = -maxDownSpeed;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, y, speed);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isSlammingDown = false;
            anim.SetBool("isGrounded", true);
        }

        if (col.gameObject.CompareTag("Obstacle"))
        {
            // Check the direction of the surface we hit.
            // A normal.y of 1 means perfectly flat ground. 
            // We use > 0.5f to allow landing on slightly angled car roofs/hoods.
            if (col.contacts[0].normal.y > 0.5f)
            {
                // We landed on top! Treat the obstacle like ground.
                isGrounded = true;
                isSlammingDown = false;
                anim.SetBool("isGrounded", true);
            }
            else
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