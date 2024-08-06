using UnityEngine;

public class EnemyBulletController : MonoBehaviour
{

    public float speed = 20.0f; // Speed of the bullet
    public float timeToLive = 30.0f; // Time to live for the bullet
    private Vector3 movementDirection;
    public int damage = 10;

    void Start()
    {
        Destroy(gameObject, timeToLive);
    }
    private void Update()
    {
            move(movementDirection);
    }
    public void Initialize(Vector3 direction)
    {
        movementDirection = direction.normalized;
    }
    // Update is called once per frame
    public void move(Vector3 direction)
    {
        transform.Translate(direction * speed * Time.deltaTime);
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag != "Enemy1" || other.gameObject.tag != "Enemy2")
        {
            Destroy(gameObject);
        }
        
    }
}
