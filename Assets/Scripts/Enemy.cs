
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static Enemy;

public class Enemy : MonoBehaviour
{
    [Header("General")]
    public TerrainGeneration terrainGeneration;
    public LayerMask groundLayer;
    public GameObject bulletPrefab;

    [Header("Pathfinding")]
    public Pathfinding pathfinding;
    private int currentPathIndex;

    [Header("Enemy Settings")]
    public float speed = 2.0f;
    private float variableSpeed = 2.0f;
    public int health = 100;
    public TextMeshProUGUI stateTxt;
    public TextMeshProUGUI healthTxt;

    [Header("Chase Settings")]
    public Transform target;
    public int chaseWeight = 0;
    private float chaseTime = 0f;
    public float chaseLength = 10f;

    [Header("Patrol Settings")]
    public int patrolWeight = 0;
    public float patrolThreshold = 15f;
    public int numberOfWaypoints = 5;
    private int currentWaypointIndex = 0;
    private bool nextWayPoint = false;
    public List<Vector3> waypoints = new List<Vector3>();

    [Header("Attack Settings")]
    public float attackThreshold = 2.0f;
    private bool fightTilldeath = false;

    [Header("Shoot Settings")]
    public float fireRate = 10f;
    private float nextShootTime = 0f;
    public int shotNo = 0;
    private float shootThreshold = 6.0f;

    [Header("Snipe Settings")]
    public float numberOfBullets = 5;
    public int snipeWeight = 0;
    public Vector3 snipePosition;
    private bool snipeing;

    [Header("Heal Settings")]
    private float nextHealTime = 0f;
    private float healRate = 15f;

    [Header("Scavange Settings")]
    private int scavangeWeigth;
    private int scavangeAgainWeigth;

    [Header("Suicide Settings")]
    private bool suicide;

    [Header("Flee Settings")]
    private bool fled = false;
    private Vector3 fleePosition;
    private bool fleeing = false;
    public enum NPCStates
    {
        Patrol,
        Chase,
        Attack,
        Shoot,
        Heal,
        Flee,
        Snipe,
        Suicide,
        Scavange
    }
    public NPCStates currentState = NPCStates.Patrol;
    void Start()
    {
        GenerateRandomWaypoints();
        variableSpeed = speed;
        pathfinding = GameObject.FindObjectOfType<Pathfinding>();
        if (pathfinding == null)
        {
            Debug.LogError("Failed to find an object with Pathfinding component");
        }
        currentPathIndex = 0;
    }
    void Update()
    {
        stateTxt.text = "State: " + currentState;
        healthTxt.text = "Health: " + health.ToString();
        SwitchState();
    }

    private void SwitchState()
    {
        switch (currentState)
        {
            case NPCStates.Patrol:
                Patrol();
                break;
            case NPCStates.Chase:
                Chase();
                break;
            case NPCStates.Attack:
                if (gameObject.tag == "Enemy1")
                {
                    Attack();
                }
                break;
            case NPCStates.Shoot:
                if (gameObject.tag == "Enemy1")
                {
                    Shoot();
                }

                break;
            case NPCStates.Flee:
                if (gameObject.tag == "Enemy1")
                {
                    Flee();
                }
                break;
            case NPCStates.Heal:
                if (gameObject.tag == "Enemy1")
                {
                    Heal();
                }
                break;
            case NPCStates.Snipe:
                if (gameObject.tag == "Enemy2")
                {
                    Snipe();
                }
                break;
            case NPCStates.Scavange:
                if (gameObject.tag == "Enemy2")
                {
                    Scavange();
                }
                break;
            case NPCStates.Suicide:
                if (gameObject.tag == "Enemy2")
                {
                    Suicide();
                }
                break;
            default:
                break;
        }
    }


    void Patrol()
    {
        variableSpeed = speed;

        //look at waypoint
        transform.LookAt(waypoints[currentWaypointIndex]);

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= patrolThreshold)
        {
            int randomDecision = Random.Range(1, 7) + Random.Range(1, 7) + patrolWeight;

            Debug.Log($"[Patrol] Random decision value for state transition: {randomDecision}");

            if (gameObject.tag == "Enemy2")
            {
                if (randomDecision > 10)
                {
                    currentState = NPCStates.Snipe;
                    Debug.Log($"[Patrol] Random decision to switch to Snipe state ({gameObject.tag})");
                    return;
                }
                else
                {
                    currentState = NPCStates.Chase;
                    Debug.Log($"[Patrol] Random decision to switch to Chase state ({gameObject.tag})");
                    patrolWeight = (patrolWeight + 1) % 5;
                    return;
                }

            }
            else
            {
                if (randomDecision < 7)
                {
                    currentState = NPCStates.Shoot;
                    Debug.Log($"[Patrol] Random decision to switch to Shoot state ({gameObject.tag})");
                    return;
                }
                else
                {
                    currentState = NPCStates.Chase;
                    Debug.Log($"[Patrol] Random decision to switch to Chase state ({gameObject.tag})");
                    return;
                }
            }

        }

        if (nextWayPoint)
        {
            nextWayPoint = false;
            Debug.Log($"[Patrol] Moving to waypoint {currentWaypointIndex}: {waypoints[currentWaypointIndex]} ({gameObject.tag})");

            //Increment index keep under count;
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }
        // Move to the next waypoint
        FollowSetup(gameObject.tag, waypoints[currentWaypointIndex], true);
    }


    void Chase()
    {
        //When chasing speed up
        variableSpeed = speed + 2;

        transform.LookAt(target.position);

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Transition to Patrol state if the target is too far
        if (distanceToTarget > patrolThreshold)
        {
            currentState = NPCStates.Patrol;
            Debug.Log($"[Chase] Target is too far. Switching to Patrol state ({gameObject.tag})");
            return;
        }

        // Transition to Attack state if close enough to the target
        if (distanceToTarget <= attackThreshold && gameObject.tag == "Enemy1")
        {
            currentState = NPCStates.Attack;
            Debug.Log($"[Chase] Close enough to target. Switching to Attack state ({gameObject.tag})");
            return;
        }

        // If chase has been too long
        if (Time.time > chaseTime)
        {
            int randomDecision = Random.Range(1, 7) + Random.Range(1, 7) + chaseWeight;
            chaseTime = Time.time + chaseLength;
            Debug.Log($"[Chase] Next decision time updated to: {chaseTime} ({gameObject.tag})");
            if (gameObject.tag == "Enemy2")
            {
                if (randomDecision > 11)
                {
                    currentState = NPCStates.Snipe;
                    Debug.Log($"[Chase] Random decision to switch to Snipe state ({gameObject.tag})");
                   
                }
                else
                {
                    currentState = NPCStates.Patrol;
                    Debug.Log($"[Chase] Random decision to switch to Patrol state ({gameObject.tag})");
                    chaseWeight = (chaseWeight + 1) % 5;
                   
                }

            }
            else
            {
                if (randomDecision < 6)
                {
                    currentState = NPCStates.Attack;
                    Debug.Log($"[Chase] Random decision to switch to Attack state ({gameObject.tag})");
                   
                }
                else
                {
                    currentState = NPCStates.Shoot;
                    Debug.Log($"[Chase] Random decision to switch to Shoot state ({gameObject.tag})");
                   
                }
               
            }
            return;

        }


        // Continue the chase
        FollowSetup(gameObject.tag, target.position);


    }
    void Attack()
    {
       
        
            variableSpeed = speed + 3;
            if (fightTilldeath && gameObject.tag == "Enemy1")
            {
                Debug.Log("[Attack] Enemy1 will fight till death ({gameObject.tag})");
                FollowSetup(gameObject.tag, target.position);
                return;
            }
         
            transform.LookAt(target.position);
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            //help enemy1 with support fire or attack if low on health
            if (health < 30 && gameObject.tag == "Enemy1")
            {
                 GameObject enemy = GameObject.FindGameObjectWithTag("Enemy2");
                if (enemy != null)
                {
                    Enemy enemyHitScript = enemy.GetComponent<Enemy>();
                    if (enemyHitScript != null)
                    {
                    int randomDecision = Random.Range(1, 7) + Random.Range(1, 7);
                        if (randomDecision > 5)
                        {
                            enemyHitScript.currentState = NPCStates.Snipe;

                        }
                        else
                        {
                            enemyHitScript.currentState = NPCStates.Attack;

                        }
                   
                    }

                }
                
                

               
            }

            if (distanceToTarget > attackThreshold)
            {

                int randomDecision = Random.Range(1, 7) + Random.Range(1, 7);

                if (randomDecision < 8 || gameObject.tag == "Enemy2")
                {
                    currentState = NPCStates.Chase;
                    Debug.Log($"[Attack] Random decision to switch to Chase state ({gameObject.tag})");
                
                }
                else
                {
                    currentState = NPCStates.Shoot;
                    Debug.Log($"[Attack] Random decision to switch to Shoot state ({gameObject.tag})");
           
                }
                return;

            }

            FollowSetup(gameObject.tag, target.position);
            Debug.Log("[Attack] Enemy1 is attacking the target ({gameObject.tag})");
        
    }

    void Flee()
    {
        if (gameObject.tag == "Enemy1")
        {
       
            if (!fleeing)
            {
                Debug.Log("[Flee] Enemy1 starts fleeing");
                Vector3? position = GetRandomPoint();
                if (position.HasValue)
                {
                    // Safely access the value
                    fleePosition = position.Value;
                    FollowSetup(gameObject.tag, fleePosition);
                    Debug.Log("[Flee] fleeing");
                }
                else
                {
                    Debug.Log("[Flee] Failed to find a valid position on the NavMesh ({gameObject.tag})");
                    fightTilldeath = true;
                    currentState = NPCStates.Attack;
                    return;
                }

                fleeing = true;
            }
            float distanceToFleePosition = Vector3.Distance(transform.position, fleePosition);
            variableSpeed = speed + 7;
            if (distanceToFleePosition < 2)
            {
                fled = true;
                Debug.Log($"[Flee] Enemy has successfully fled to position: {fleePosition} ({gameObject.tag})");
            }
            else
            {
                fled = false;
            }

            if (fled)
            {
                fleeing = false;
                int randomDecision = Random.Range(1, 7) + Random.Range(1, 7);
                if (randomDecision < 4)
                {
                    currentState = NPCStates.Heal;
                    Debug.Log($"[Flee] Random decision to switch to Heal state ({gameObject.tag})");
                }
                else
                {
                    currentState = NPCStates.Patrol;
                    Debug.Log($"[Flee] Random decision to switch to Patrol state ({gameObject.tag})");
                }
                return;

            }
        }
    }

    void Heal()
    {
        variableSpeed = speed;
        if (Time.time > nextHealTime && gameObject.tag == "Enemy1")
        {
            health += 20;
        }
        nextHealTime = Time.time + healRate;
        currentState = NPCStates.Patrol;
        return;

    }
    void Shoot()
    {
        variableSpeed = speed;
        if (shotNo > 5)
        {
            int randomDecision = Random.Range(1, 7) + Random.Range(1, 7);
            if (randomDecision > 3)
            {
                currentState = NPCStates.Patrol;
                Debug.Log("Changing state to Patrol after 5 shots");
            }
            else
            {
                currentState = NPCStates.Chase;
                Debug.Log("Changing state to Chase after 5 shots");
            }
            return;
        }

        if (Vector3.Distance(transform.position, target.position) > shootThreshold)
        {
            currentState = NPCStates.Chase;
            Debug.Log("Target is too far, changing state to Chase");
            return;
        }

        if (Time.time > nextShootTime)
        {
            // Enemy turns to face the target
            
            GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward, transform.rotation);
            bullet.transform.LookAt(target.transform);
            shotNo++;
            Debug.Log("Shooting bullet. Shot number: " + shotNo);

            EnemyBulletController bulletController = bullet.GetComponent<EnemyBulletController>();
            if (bulletController != null)
            {
                bulletController.speed = 20;
                bulletController.Initialize(transform.forward);
                Debug.Log("Bullet initialized with speed");
            }
            else
            {
                Debug.LogWarning("BulletController component not found on bullet prefab");
            }

            nextShootTime = Time.time + fireRate;
            Debug.Log("Next shoot time set to: " + nextShootTime);
        }
        if (Vector3.Distance(transform.position, target.position) > attackThreshold)
        {
            FollowSetup(gameObject.tag, target.position);
        }

    }
    void Scavange()
    {

        if (gameObject.tag == "Enemy2")
        {
            Vector3? position = GetRandomPoint();
            if (position.HasValue)
            {
                // Safely access the value
                Vector3 scavangePosition = position.Value;
                FollowSetup(gameObject.tag, scavangePosition);
                int randomDecision = Random.Range(1, 7) + Random.Range(1, 7) + scavangeWeigth;

                if (randomDecision > 10)
                {
                    numberOfBullets++;
                    scavangeWeigth = (scavangeWeigth + 1) % 5;
                   
                    int randomDecision2 = Random.Range(1, 7) + Random.Range(1, 7) + scavangeAgainWeigth;
                    if (randomDecision2 > 11)
                    {
                        //Scavenge again
                        scavangeAgainWeigth = (scavangeAgainWeigth + 1) % 5;
                        return;
                    }
                }
            }
            Debug.Log($"[Scavange] Random decision to switch to Patrol state ({gameObject.tag})");
            currentState = NPCStates.Patrol;
            return;
        }
    }
    void Suicide()
    {
        if (gameObject.tag == "Enemy2")
        {
            suicide = true;
            FollowSetup(gameObject.tag, target.position);
        }
        return;

    }
    void Snipe()
    {
        if (!snipeing)
        {
            snipePosition = FindHighestTerrainPoint();
            snipeing = true;
        }
        else
        {
            if (numberOfBullets == 0)
            {
                snipeing = false;
                int randomDecision = Random.Range(1, 7) + Random.Range(1, 7) + snipeWeight;
                if (randomDecision > 8)
                {
                    currentState = NPCStates.Scavange;
                    snipeWeight = (snipeWeight + 1) % 5;
                    Debug.Log($"[Chase] Random decision to switch to Snipe state ({gameObject.tag})");
               
                }
                else
                {
                    currentState = NPCStates.Patrol;
                    Debug.Log($"[Chase] Random decision to switch to Patrol state ({gameObject.tag})");
                   
                }
                return;
            }

            if (Vector3.Distance(transform.position, snipePosition) < 1)
            {
                if (Time.time > nextShootTime)
                {
                  
                    GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward, transform.rotation);
                    bullet.transform.LookAt(target.transform);
                    EnemyBulletController bulletController = bullet.GetComponent<EnemyBulletController>();
                    if (bulletController != null)
                    {
                        bulletController.speed = 20;
                        int randomDecision = Random.Range(1, 7) + Random.Range(1, 7) - (health/10);
                        if (randomDecision > 7)
                        {
                            bulletController.damage = 30;
                        }
                       
                        bulletController.Initialize(transform.forward);
                    }
                    numberOfBullets--;
                    nextShootTime = Time.time + fireRate;
                }
            }
            else
            {
                FollowSetup(gameObject.tag, snipePosition);
            }

        }



    }
    Vector3 FindHighestTerrainPoint()
    {
        float boxWidth = 10f; // Width of the box
        float boxLength = 10f; // Length of the box
        float boxCastHeight = 1000f; // Starting height for the box cast
        float minDistanceFromPlayer = 7f; // Minimum distance from the player

        Vector3 boxCenter = new Vector3(target.position.x, boxCastHeight, target.position.z);
        Vector3 boxSize = new Vector3(boxWidth / 2, 0.01f, boxLength / 2);

        RaycastHit[] hits = Physics.BoxCastAll(boxCenter, boxSize, Vector3.down, Quaternion.identity, boxCastHeight);

        return ProcessHits(hits, minDistanceFromPlayer);
    }
    Vector3 ProcessHits(RaycastHit[] hits, float minDistanceFromPlayer)
    {
        Vector3 highestPoint = Vector3.zero;
        float highestElevation = float.MinValue;

        foreach (RaycastHit hit in hits)
        {
            if (hit.point.y > highestElevation && Vector3.Distance(hit.point, target.position) >= minDistanceFromPlayer)
            {
                highestElevation = hit.point.y;
                highestPoint = hit.point;
            }
        }

        return highestPoint;
    }
    void FollowSetup(string key, Vector3 target, bool iterate = false)
    {
        List<Node> newPath;
        if (key == "Enemy1")
        {

            newPath = pathfinding.FindPathA(transform.position, target, gameObject.tag);
        }
        else
        {

            newPath = pathfinding.FindPathDijkstra(transform.position, target, gameObject.tag);
        }

        pathfinding.grid.paths[key] = newPath;
        bool player = !iterate;
        FollowPath(key, player);
    }
    void FollowPath(string key, bool player)
    {

        List<Node> enemyPath;


        if (pathfinding.grid.paths.TryGetValue(key, out enemyPath) && enemyPath.Count > 0)
        {
            // Target the current node
            Node targetNode = enemyPath[currentPathIndex];
            float terrainHeight = getHeight(targetNode.worldPosition.x, targetNode.worldPosition.z);
            Vector3 targetPosition = new Vector3(targetNode.worldPosition.x, terrainHeight, targetNode.worldPosition.z);

            // Move towards the target position (including height adjustment)
            Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetPosition, variableSpeed * Time.fixedDeltaTime);
            // Move towards the target node

            GetComponent<Rigidbody>().MovePosition(nextPosition);

            // Check if we've reached the target node
            if (Vector3.Distance(transform.position, targetNode.worldPosition) <= 1)
            {
                currentPathIndex++; // Move to the next node
                Debug.Log($"Reached waypoint {currentPathIndex}");

                if (currentPathIndex >= enemyPath.Count)
                {
                    // Path complete
                    Debug.Log("Path complete, resetting path index");
                    currentPathIndex = 0; // Reset for next path
                    pathfinding.grid.paths[key] = null; // Clear path                               
                }
            }
        }
        else
        {
            if (!player)
            {
                nextWayPoint = true;
            }
           
        }

    }

    float getHeight(float x, float z)
    {

        RaycastHit terrainHit;
        if (Physics.Raycast(new Vector3(x, terrainGeneration.maxHeight + 1, z), Vector3.down, out terrainHit, Mathf.Infinity, groundLayer))
        {
            return terrainHit.point.y + 0.3f;
        }
        return terrainGeneration.maxHeight + 1;
    }

    void GenerateRandomWaypoints()
    {
        
        for (int i = 0; i < numberOfWaypoints; i++)
        {
            // Generate waypoints around the center point within the patrol area
            float randomX = Random.Range(-terrainGeneration.Width / 2, terrainGeneration.Width / 2);
            float randomZ = Random.Range(-terrainGeneration.Length / 2, terrainGeneration.Length / 2);
            float terrainHeight = getHeight(randomX, randomZ);

            Vector3 waypoint = new Vector3(randomX, terrainHeight, randomZ);
            waypoints.Add(waypoint);
        }
    }
    private void OnTriggerEnter(Collider other)
    {

        switch (other.gameObject.tag)
        {
            case "Bullet":
                if (gameObject.tag == "Enemy1")
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");

                    if (player != null)
                    {
                        PlayerHit playerHitScript = player.GetComponent<PlayerHit>();
                        if (playerHitScript != null)
                        {
                            if (playerHitScript.pan)
                            {
                                health -= 20;
                            }
                            else
                            {
                                health -= 10;
                            }
                        }

                    }
                   
                    if (health < 30)
                    {
                        if (!fightTilldeath)
                        {
                            int randomDecision = Random.Range(1, 7) + Random.Range(1, 7);
                            if (randomDecision >= 6)
                            {
                                currentState = NPCStates.Flee;
                            }
                            else
                            {
                                fightTilldeath = true;
                                currentState = NPCStates.Attack;
                            }
                        }
                    }
                }
                else
                {
                    health -= 10;
                    if (health < 30)
                    {
                        int randomDecision = Random.Range(1, 7) + Random.Range(1, 7) - (health / 10);
                        if (randomDecision >= 9)
                        {
                            suicide = true;
                            currentState = NPCStates.Suicide;
                        }
                        else if (randomDecision > 5)
                        {
                            currentState = NPCStates.Snipe;
                        }
                        else
                        {
                            currentState = NPCStates.Patrol;
                        }
                    }
                }
                
                break;
            case "Player":
                if (suicide)
                {
                    Destroy(gameObject);
                }
                else{

                     GameObject player = GameObject.FindGameObjectWithTag("Player");

                    if (player != null)
                    {
                        PlayerHit playerHitScript = player.GetComponent<PlayerHit>();
                        if (playerHitScript != null)
                        {
                            if (playerHitScript.pan)
                            {
                                health -= 20;
                            }
                            else
                            {
                                health -= 10;
                            }
                        }
                       
                    }
                   
                }
                
                break;
            default:
                break;

        }
        healthTxt.text = "Health: " + health.ToString();
        if (health < 1)
        {
            Destroy(gameObject);
        }
    }
    private Vector3? GetRandomPoint()
    {
        float randomX = Random.Range(-terrainGeneration.Width / 2, terrainGeneration.Width / 2);
        float randomZ = Random.Range(-terrainGeneration.Length / 2, terrainGeneration.Length / 2);
        float terrainHeight = getHeight(randomX, randomZ);
        var pos = new Vector3(randomX, terrainHeight, randomZ);

        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 20, NavMesh.AllAreas))
        {

            RaycastHit terrainHit;
            if (Physics.Raycast(new Vector3(hit.position.x, terrainGeneration.maxHeight, hit.position.z), Vector3.down, out terrainHit))
            {
                Vector3 finalPosition = new Vector3(hit.position.x, terrainHit.point.y, hit.position.z);
                return finalPosition;
            }

        }
        return null;

    }

}
