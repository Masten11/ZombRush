using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Inställningar")]
    public float speed = 10f;
    public float jumpForce = 12f;
    public float laneDistance = 3f; 
    public float sideSpeed = 15f;

    [Header("Roll Inställningar")]
    public float rollDuration = 1.0f;
    public float rollHeightMultiplier = 0.5f;

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
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        // Hämta Animator även om den sitter på child
        anim = GetComponentInChildren<Animator>();

        originalHeight = playerCollider.height;
        originalCenter = playerCollider.center;
    }

    void Update() 
    {
        // Hopp
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRolling) 
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;

            anim.SetTrigger("Jump");
            anim.SetBool("isGrounded", false);
        }

        // Roll
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isRolling) 
        {
            StartCoroutine(PerformRoll());
        }

        // Filbyte
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 2) currentLane++;
        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0) currentLane--;

        // Sidled
        float targetX = (currentLane - 1) * laneDistance;
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * sideSpeed);
        transform.position = newPos;
    }

    IEnumerator PerformRoll() 
    {
        isRolling = true;
        anim.SetBool("isRolling", true);

        playerCollider.height = originalHeight * rollHeightMultiplier;
        playerCollider.center = new Vector3(originalCenter.x, originalCenter.y / 2f, originalCenter.z);

        if (!isGrounded) 
        {
            rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(rollDuration);

        playerCollider.height = originalHeight;
        playerCollider.center = originalCenter;

        anim.SetBool("isRolling", false);
        isRolling = false;
    }

    void FixedUpdate() 
    {
        // Använd rätt velocity
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
