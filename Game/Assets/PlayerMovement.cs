using UnityEngine; // Detta säger till Unity att vi vill använda deras inbyggda verktyg

public class PlayerMovement : MonoBehaviour
{
    // Dessa variabler syns i Inspector-fönstret i Unity
    public float speed = 10f;       // Hur snabbt vi springer framåt
    public float jumpForce = 5f;    // Hur kraftfullt hoppet är

    // Dessa är interna variabler som scriptet använder för att hålla koll på saker
    private Rigidbody rb;           // En referens till fysik-komponenten (Rigidbody)
    private bool isGrounded;        // En "strömbrytare" (Ja/Nej) för att veta om vi står på marken

    // Start körs en gång precis när man trycker på Play
    void Start() {
        // Vi letar upp Rigidbody-komponenten på vår Cylinder och sparar den i variabeln 'rb'
        rb = GetComponent<Rigidbody>();
    }

    // Update körs hela tiden, så snabbt datorn hinner (bra för knapptryck)
    void Update() {
        // Om vi trycker ner Space-tangenten OCH är på marken...
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
            // ...skicka iväg oss uppåt med en impuls-kraft (som ett hopp!)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            // Nu är vi i luften, så vi sätter isGrounded till false (nej)
            isGrounded = false;
        }
    }

    // FixedUpdate används för fysik-beräkningar (körs i jämn takt)
    void FixedUpdate() {
        // Här tvingar vi cylinderns fart framåt (Z-axeln) att vara lika med vår 'speed'
        // Vi behåller gubbens nuvarande fart i X och Y (så att hopp och fall fungerar)
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, speed);
    }

    // Denna körs automatiskt när cylindern krockar med något
    void OnCollisionEnter(Collision col) {
        // Om det vi krockade med har etiketten (Tag) "Ground"...
        if (col.gameObject.CompareTag("Ground")) {
            // ...då vet vi att vi står på marken igen och får hoppa!
            isGrounded = true;
        }
    }
}
}