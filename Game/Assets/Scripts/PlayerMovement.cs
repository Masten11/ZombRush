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
    public float rollHeightMultiplier = 0.5f;

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

        // Hopp
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRolling)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;

            anim.SetTrigger("Jump");
            anim.SetBool("isGrounded", false);
        }

        // Roll (Trigger istället för Bool)
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isRolling)
        {
            StartCoroutine(PerformRoll());
        }

        // Filbyte
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 2) currentLane++;
        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0) currentLane--;

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

    
    yield return new WaitForSeconds(0.6f);

    // Återställ collider
    playerCollider.height = originalHeight;
    playerCollider.center = originalCenter;

    isRolling = false;
}

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, speed);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            anim.SetBool("isGrounded", true);
        }

        if (col.gameObject.CompareTag("Obstacle"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}