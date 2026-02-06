using UnityEngine;
using System.Collections; // <--- DENNA RAD SAKNADES!

public class PlayerMovement : MonoBehaviour
{
    [Header("Inställningar")]
    public float speed = 10f;
    public float jumpForce = 10f;
    public float laneDistance = 8f; // Hur brett det är mellan filerna
    public float sideSpeed = 15f;   // Hur snabbt gubben glider åt sidan

    [Header("Roll Inställningar")]
    public float rollDuration = 1.0f; // Hur länge rullningen pågår
    public float rollHeight = 0.5f;   // Hur mycket vi krymper på höjden (0.5 = hälften)
    public float rollAngle = -90f;    // Hur många grader vi lutar bakåt
    public float rollForce = 15f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isRolling = false;
    private int currentLane = 1; // 0 = Vänster, 1 = Mitten, 2 = Höger

    // För att komma ihåg hur gubben såg ut innan rullningen
    private Vector3 originalScale;
    private Quaternion originalRotation;

    void Start() {
        rb = GetComponent<Rigidbody>();

        // Spara originalstorlek och rotation så vi kan återställa dem sen
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }

    void Update() {
        // Ser till att man är på marken och inte rullar
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRolling) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // --- ROLL-LOGIK ---
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isRolling) {
            StartCoroutine(PerformRoll());
        }

        // --- FILBYTE-LOGIK 
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (currentLane < 2) currentLane++;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (currentLane > 0) currentLane--;
        }

        // Räkna ut målet: Var ska vi vara i X-led?
        // (0-1)*3 = -3 (Vänster) , (1-1)*3 = 0 (Mitten) , (2-1)*3 = 3 (Höger)
        float targetX = (currentLane - 1) * laneDistance;

        // Här sker "svepet", Vi ändrar positionen mjukt mot targetX
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * sideSpeed);
        transform.position = newPos;
    }

    // Denna funktion körs "vid sidan av" och kan pausa sig själv
    IEnumerator PerformRoll() {
        isRolling = true;

        // 1. Krymp kroppen (halva höjden) => gör collidern mindre
        transform.localScale = new Vector3(originalScale.x, originalScale.y * rollHeight, originalScale.z);

        // 2. Luta kroppen bakåt (rotera runt X-axeln)
        // Vi använder Rotate för att lägga till rotationen
        transform.Rotate(rollAngle, 0, 0);

        // Om du vill att han ska falla snabbt
        if (!isGrounded) {
            rb.AddForce(Vector3.down * rollForce, ForceMode.Impulse);
        }

        // 3. Vänta i viss antal sekunder
        yield return new WaitForSeconds(rollDuration);

        // 4. Återställ allt till det normala
        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        isRolling = false;
    }

    void FixedUpdate() {
        // Konstant fart framåt
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, speed);
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.CompareTag("Ground")) isGrounded = true;
    }
}