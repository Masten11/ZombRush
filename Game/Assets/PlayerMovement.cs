using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Inställningar")]
    public float speed = 10f;
    public float jumpForce = 5f;
    public float laneDistance = 3f; // Hur brett det är mellan filerna
    public float sideSpeed = 10f;   // Hur snabbt gubben glider åt sidan

    private Rigidbody rb;
    private bool isGrounded;
    private int currentLane = 1; // 0 = Vänster, 1 = Mitten, 2 = Höger

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        // --- HOPP-LOGIK ---
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // --- FILBYTE-LOGIK (Subway Surfer-stil) ---
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (currentLane < 2) currentLane++;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (currentLane > 0) currentLane--;
        }

        // Räkna ut målet: Var ska vi vara i X-led?
        // (0-1)*3 = -3 (Vänster) | (1-1)*3 = 0 (Mitten) | (2-1)*3 = 3 (Höger)
        float targetX = (currentLane - 1) * laneDistance;

        // Här sker "svepet": Vi ändrar positionen mjukt mot targetX
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * sideSpeed);
        transform.position = newPos;
    }

    void FixedUpdate() {
        // Konstant fart framåt
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, speed);
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.CompareTag("Ground")) isGrounded = true;
    }
}