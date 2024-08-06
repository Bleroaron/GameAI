using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Grid : MonoBehaviour 
{
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public Node[,] nodeGrid;
	float nodeDiameter;
	public int gridSizeX, gridSizeY;
    public Dictionary<string, List<Node>> paths = new Dictionary<string, List<Node>>();


    public void Run()
	{
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
		CreateGrid();
	}

	void CreateGrid() // creates the grid of nodes
	{
		nodeGrid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius) - Vector3.up * 5;

               bool walkable = !Physics.CheckCapsule(worldPoint, worldPoint + Vector3.up * 10000f, nodeRadius, unwalkableMask);
				nodeGrid[x, y] = new Node(walkable, worldPoint, x, y);
			}
		}
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition) // selects the closest node using a given world position
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return nodeGrid[x, y];
	}

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (nodeGrid != null)
        {
            // First draw all nodes
            foreach (Node n in nodeGrid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
            }

            // Then draw paths if any
            if (paths != null)
            {
                foreach (var path in paths)
                {
                    foreach (Node pathNode in path.Value)
                    {
                        Gizmos.color = Color.black; // You can customize this color for each path if needed
                        Gizmos.DrawCube(pathNode.worldPosition, Vector3.one * (nodeDiameter - .1f));
                    }
                }
            }
        }
    }

}
