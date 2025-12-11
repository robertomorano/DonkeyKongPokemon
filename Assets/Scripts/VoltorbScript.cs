using UnityEngine;

public class VoltorbScript : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 1f;
    
    //public Transform[] endPoint2;
    //private Transform target;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        Vector2 direction = new Vector2(-0.8f, 0.0f);
        rb.linearVelocity = direction * speed;
        //Generar numero aletaorio pa posicion
        //  target = endPoint2;
    }

    private void Update()
    {
        /*if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            //target = null;
        }*/
    }
    private void FixedUpdate()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            rb.AddForce(collision.transform.right * speed, ForceMode2D.Impulse);
        }*/
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("KillZone"))
        {
            Destroy(gameObject);
        } 
        if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.Instance.HandlePlayerHit();

            Destroy(gameObject);

        }
    }
        private void OnTriggerEnter2D(Collider2D other)
    {
      
        if (other.CompareTag("Player"))
        {
      
            GameManager.Instance.HandlePlayerHit();

            Destroy(gameObject);
        }
    }

}


