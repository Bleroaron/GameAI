using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AssetPathfinding : MonoBehaviour
{
    public List<GameObject> dynamicAssets;
    public LineRenderer lineRenderer;
    public bool showPath;
    public bool on;
    public Button button;
    private NavMeshAgent agent;
    [SerializeField] private LayerMask terrainLayer;
    public TerrainGeneration terrainGeneration;
    string current;
    void Start()
    {
        if (on)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.enabled = true; 
            dynamicAssets = new List<GameObject>();
            button.onClick.AddListener(OnButtonClick);
            agent = GetComponent<NavMeshAgent>();
        }

    }


   public  void OnButtonClick()
    {
        agent.enabled = false;
        agent.enabled = true;
        if (dynamicAssets.Count == 0) return;

        GameObject selectedAsset = dynamicAssets[Random.Range(0, dynamicAssets.Count)];
        while (current == selectedAsset.tag)
        {
            selectedAsset = dynamicAssets[Random.Range(0, dynamicAssets.Count)];
        }
        current = selectedAsset.tag;
        MoveAgentToAsset(selectedAsset);
        
    }

    void MoveAgentToAsset(GameObject asset)
    {

        if (agent)
        {
            if (!agent.isOnNavMesh)
            {
                Debug.LogError("Agent is not on NavMesh");
                return; 
            }

       

            agent.SetDestination(asset.transform.position);
        }
    }


    void Update()
    {
        if (agent.hasPath && showPath)
        {
           
            List<Vector3> pathPoints = new List<Vector3>();

            for (int i = 0; i < agent.path.corners.Length; i++)
            {
                if (i < agent.path.corners.Length - 1)
                {
                    Vector3 start = agent.path.corners[i];
                    Vector3 end = agent.path.corners[i + 1];

                    pathPoints.Add(new Vector3(start.x, getHeight(start.x, start.z), start.z));
                    AddIntermediatePoints(pathPoints, start, end);
                }
                else
                {
                    Vector3 corner = agent.path.corners[i];
                    pathPoints.Add(new Vector3(corner.x, getHeight(corner.x, corner.z), corner.z));
                }
            }

            lineRenderer.positionCount = pathPoints.Count;
          
            lineRenderer.SetPositions(pathPoints.ToArray());
        }
        else
        {
          
            lineRenderer.positionCount = 0;
        }
    }

    void AddIntermediatePoints(List<Vector3> pathPoints, Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        int intermediatePointCount = Mathf.FloorToInt(distance / 1.0f); 

        for (int j = 1; j < intermediatePointCount; j++)
        {
            float lerpFactor = (float)j / intermediatePointCount;
            Vector3 intermediatePoint = Vector3.Lerp(start, end, lerpFactor);
            intermediatePoint.y = getHeight(intermediatePoint.x, intermediatePoint.z);
            pathPoints.Add(intermediatePoint);
        }
    }

    float getHeight(float x, float z)
    {
        RaycastHit terrainHit;
        if (Physics.Raycast(new Vector3(x, terrainGeneration.maxHeight,z), Vector3.down, out terrainHit))
        {
            return terrainHit.point.y + 0.3f;
        }
        return terrainGeneration.maxHeight+1;
    }
}