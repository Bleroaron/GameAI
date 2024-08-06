using UnityEngine;

public class BulletController : MonoBehaviour
{

    public float speed = 20.0f; // Speed of the bullet
    public float timeToLive = 30.0f; // Time to live for the bullet
    private Camera camera;
    public LayerMask groundLayer;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timeToLive);
        camera = Camera.main;
    
        RotateTowardsMouseCursor();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        
    }
    void RotateTowardsMouseCursor()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundLayer))
        {
            Vector3 target = hitInfo.point;
            target.y = transform.position.y;
            transform.LookAt(target);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag != "Player")
        {
            Destroy(gameObject);
        }
        
    }
}
