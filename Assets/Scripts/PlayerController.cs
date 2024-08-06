
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public LayerMask groundLayer;
    public GameObject bullet;
    bool pathFinding;

    void Start()
    {
        pathFinding = !GetComponent<AssetPathfinding>().on;
        if (!pathFinding)
        {
            GetComponent<NavMeshAgent>().enabled = false;
        }

    }

    void FixedUpdate()
    {
        if (!pathFinding)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);
            GetComponent<Rigidbody>().MovePosition(transform.position + movement * speed * Time.fixedDeltaTime);
        }
    }
    void Update()
    {
        
        RotateTowardsMouseCursor();
        if (!pathFinding)
        {
            Shoot();
        }
    }
    
    void RotateTowardsMouseCursor()
    {
        Ray ray = GetComponent<CameraController>().camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundLayer))
        {
            Vector3 target = hitInfo.point;
            target.y = transform.position.y; 
            transform.LookAt(target);
        }
    }
    void Shoot()
    {
        if (Input.GetKeyDown(KeyCode.Space) ||  Input.GetMouseButtonDown(0))
        {
            Instantiate(bullet, transform.position,  Quaternion.identity);
            
        }
            
    }

}


