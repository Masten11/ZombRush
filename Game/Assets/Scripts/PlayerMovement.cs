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
    public float rollHeightMultiplier = 0.5f; // Hur mycket av originalhöjden som sparas

    private Rigidbody rb;
    private CapsuleCollider playerCollider;
    private Animator anim; // Redo för framtida animationer

    private bool isGrounded;
    private bool isRolling = false;
    private int currentLane = 1;

    // Sparar colliderns originalmått
    private float originalHeight;
    private Vector3 originalCenter;

    void Start() {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>(); // Se till att du har en Animator-komponent på gubben

        // Spara originalstorleken på hitboxen
        originalHeight = playerCollider.height;
        originalCenter = playerCollider.center;
    }

    void Update() {
        // Hopp
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRolling) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            // anim.SetTrigger("Jump"); // Här lägger du sen in hopp-animation
        }

        // Roll / Crouch
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isRolling) {
            StartCoroutine(PerformRoll());
        }

        // Filbyte
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 2) currentLane++;
        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0) currentLane--;

        // Mjuk förflyttning i sidled
        float targetX = (currentLane - 1) * laneDistance;
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * sideSpeed);
        transform.position = newPos;
    }

    IEnumerator PerformRoll() {
        isRolling = true;
        
        // --- ANIMATION START ---
        // anim.SetBool("isRolling", true); 

        // 1. Justera Hitboxen istället för Scale
        // Vi sänker höjden och flyttar ner center-punkten så att gubbens "botten" stannar kvar på marken
        playerCollider.height = originalHeight * rollHeightMultiplier;
        playerCollider.center = new Vector3(originalCenter.x, originalCenter.y / 2f, originalCenter.z);

        if (!isGrounded) {
            rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(rollDuration);

        // 2. Återställ Hitboxen
        playerCollider.height = originalHeight;
        playerCollider.center = originalCenter;

        // --- ANIMATION SLUT ---
        // anim.SetBool("isRolling", false);

        isRolling = false;
    }

    void FixedUpdate() {
        // Konstant fart framåt - använder velocity för bättre fysikkänsla
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, speed);
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.CompareTag("Ground")) isGrounded = true;
        
        if (col.gameObject.CompareTag("Obstacle")) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}