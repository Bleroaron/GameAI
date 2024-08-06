
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Seed")]
    [SerializeField] int seed;
    [Header("Terrain Generation")]
    public int Width = 50;
    public int Length = 50;

    [Header("Obstacles")]
    public GameObject obstaclesPrefab;
    public int NoOfObstacles = 10;

    [Header("Perlin Noise")]
    public bool usePerlin;
    public int gridSize = 256;
    [SerializeField] float perlinFrequencyX = 0.1f;
    [SerializeField] float perlinFrequencyZ = 0.1f;
    [SerializeField] float perlinNoiseStrength = 7f;
    public Spawner spawner;

    [Header("Terrain Visualization")]
    public bool VisualizeVertex;

    enum TerrainStyle{
        TerrainColor,
        BlackToWhite,
        WhiteToBlack
    }
    [SerializeField] TerrainStyle terrainStyle;

    Gradient TerrainGradient;
    Gradient BlackToWhiteGradient;
    Gradient WhiteToBlackGradient;

    Vector3[] vertices;
    int[] trianglePoints;
    Vector2[] uvs;
    UnityEngine.Color[] colors;

    Mesh mesh;
    MeshCollider meshCollider;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    NavMeshSurface navMeshSurface;

    public float maxHeight;
    public float minHeight;
    Vector2[] perlinGrid;
    public Grid grid;

    void Start()
    {
        float startTime = Time.realtimeSinceStartup;
        mesh = new Mesh();
        mesh.name = "Procedural Terrain";
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        meshRenderer.material = mat;

        navMeshSurface = GetComponent<NavMeshSurface>();
        meshCollider = GetComponent<MeshCollider>();


        #region TerrainColor Gradient Code

        GradientColorKey[] colorKeyTerrain = new GradientColorKey[8];
        colorKeyTerrain[0].color = new UnityEngine.Color(0, 0.086f, 0.35f, 1);
        colorKeyTerrain[0].time = 0.0f;

        colorKeyTerrain[1].color = new UnityEngine.Color(0, 0.135f, 1, 1);
        colorKeyTerrain[1].time = 0.082f;

        colorKeyTerrain[2].color = new UnityEngine.Color(0, 0.735f, 1, 1);
        colorKeyTerrain[2].time = 0.26f;

        colorKeyTerrain[3].color = new UnityEngine.Color(1, 0.91f, 0.5f, 1);
        colorKeyTerrain[3].time = 0.31f;

        colorKeyTerrain[4].color = new UnityEngine.Color(0.06f, 0.31f, 0, 1);
        colorKeyTerrain[4].time = 0.45f;

        colorKeyTerrain[5].color = new UnityEngine.Color(0.31f, 0.195f, 0.11f, 1);
        colorKeyTerrain[5].time = 0.59f;

        colorKeyTerrain[6].color = new UnityEngine.Color(0.41f, 0.41f, 0.41f, 1);
        colorKeyTerrain[6].time = 0.79f;

        colorKeyTerrain[7].color = new UnityEngine.Color(1, 1, 1, 1);
        colorKeyTerrain[7].time = 1.0f;

        GradientAlphaKey[] alphaKeyTerrain = new GradientAlphaKey[2];
        alphaKeyTerrain[0].alpha = 1.0f;
        alphaKeyTerrain[0].time = 0.0f;

        alphaKeyTerrain[1].alpha = 1.0f;
        alphaKeyTerrain[1].time = 1.0f;

        TerrainGradient = new Gradient();
        TerrainGradient.SetKeys(colorKeyTerrain, alphaKeyTerrain);

        #endregion

        #region BlackToWhite Gradient Code

        GradientColorKey[] colorKeyBTW = new GradientColorKey[2];

        colorKeyBTW[0].color = new UnityEngine.Color(0,0,0,1);
        colorKeyBTW[0].time = 0.0f;

        colorKeyBTW[1].color = new UnityEngine.Color(1,1,1,1);
        colorKeyBTW[1].time = 1;

        

        GradientAlphaKey[] alphaKeyBTW = new GradientAlphaKey[2];

        alphaKeyBTW[0].alpha = 1.0f;
        alphaKeyBTW[0].time = 0.0f;

        alphaKeyBTW[1].alpha = 1.0f;
        alphaKeyBTW[1].time = 1.0f;


        BlackToWhiteGradient = new Gradient();
        BlackToWhiteGradient.SetKeys(colorKeyBTW, alphaKeyBTW);
        #endregion

        #region BlackToWhite Gradient Code

        GradientColorKey[] colorKeyWTB = new GradientColorKey[2];

        colorKeyWTB[0].color = new UnityEngine.Color(1, 1, 1, 1);
        colorKeyWTB[0].time = 0.0f;

        colorKeyWTB[1].color = new UnityEngine.Color(0, 0, 0, 1);
        colorKeyWTB[1].time = 1;



        GradientAlphaKey[] alphaKeyWTB = new GradientAlphaKey[2];

        alphaKeyWTB[0].alpha = 1.0f;
        alphaKeyWTB[0].time = 0.0f;

        alphaKeyWTB[1].alpha = 1.0f;
        alphaKeyWTB[1].time = 1.0f;


        WhiteToBlackGradient = new Gradient();
        WhiteToBlackGradient.SetKeys(colorKeyWTB, alphaKeyWTB);
        #endregion

        InitializePerlinGradients();

        GenerateMeshData();

        GenerateTerrain();
        SpawnObstaclesOnNavMesh();
        navMeshSurface.BuildNavMesh();
        grid.Run();
        spawner.SpawnPlayerAssetsEnemiesOnNavMesh();
        float terrainGenerationTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Terrain Generation Time: " + terrainGenerationTime + " seconds");
    }
    void GenerateMeshData()
    {
        vertices = new Vector3[(Width + 1) * (Length + 1)];
        int i = 0;

        float offsetX = Width / 2.0f;
        float offsetZ = Length / 2.0f;

        for (int z = 0; z <= Length; z++)
        {
            for (int x = 0; x <= Width; x++)
            {
                float y;
                if (usePerlin)
                {
                    y = Mathf.PerlinNoise(x * perlinFrequencyX, z * perlinFrequencyZ) * perlinNoiseStrength;
                }
                else
                {
                    y = CustomPerlin(x * perlinFrequencyX, z * perlinFrequencyZ) * perlinNoiseStrength;
                }


                vertices[i] = new Vector3(x - offsetX, y, z - offsetZ);

                if (y > maxHeight) maxHeight = y;
                if (y < minHeight) minHeight = y;

                i++;
            }
        }

        trianglePoints = new int[Width * Length * 6];

        int currentTrianglePoint = 0;
        int currentVertexPoint = 0;

        for (int z = 0; z < Length; z++)
        {
            for (int x = 0; x < Width; x++)
            {

                trianglePoints[currentTrianglePoint + 0] = currentVertexPoint + 0;
                trianglePoints[currentTrianglePoint + 1] = currentVertexPoint + Width + 1;
                trianglePoints[currentTrianglePoint + 2] = currentVertexPoint + 1;
                trianglePoints[currentTrianglePoint + 3] = currentVertexPoint + 1;
                trianglePoints[currentTrianglePoint + 4] = currentVertexPoint + Width + 1;
                trianglePoints[currentTrianglePoint + 5] = currentVertexPoint + Width + 2;

                currentVertexPoint++;
                currentTrianglePoint += 6;

            }

            currentVertexPoint++;
        }

        uvs = new Vector2[vertices.Length];
        i = 0;

        for (int z = 0; z <= Length; z++)
        {
            for (int x = 0; x <= Width; x++)
            {

                uvs[i] = new Vector2((float)x / Length, (float)z / Width);
                i++;
            }

        }

        colors = new UnityEngine.Color[vertices.Length];
        i = 0;

        for (int z = 0; z <= Length; z++)
        {
            for (int x = 0; x <= Width; x++)
            {

                float Height = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y);

                switch (terrainStyle)
                {
                    case TerrainStyle.TerrainColor:
                        colors[i] = TerrainGradient.Evaluate(Height);
                        break;
                    case TerrainStyle.BlackToWhite:
                        colors[i] = BlackToWhiteGradient.Evaluate(Height);
                        break;
                    case TerrainStyle.WhiteToBlack:
                        colors[i] = WhiteToBlackGradient.Evaluate(Height);
                        break;
                }
                i++;
            }

        }
    }
    void GenerateTerrain()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = trianglePoints;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
        navMeshSurface.BuildNavMesh();
    }

    void InitializePerlinGradients()

        {
        Random.InitState(seed);
        perlinGrid = new Vector2[gridSize* gridSize];
            for (int i = 0; i < perlinGrid.Length; i++)
            {
                
                float Angle = UnityEngine.Random.value * 2 * Mathf.PI;
                perlinGrid[i] = new Vector2(Mathf.Cos(Angle), Mathf.Sin(Angle));
                
            }
            
        }

    float CustomPerlin(float x, float y)
        {
        int x0 = (int)(x);
        int x1 = x0 + 1;
        int y0 = (int)(y);
        int y1 = y0 + 1;

        float sx = x - x0;
        float sy = y - y0;

        float n0 = DotGridGradient(x0, y0, x, y);
        float n1 = DotGridGradient(x1, y0, x, y);
        float ix0 = Mathf.Lerp(n0, n1, sx);

        n0 = DotGridGradient(x0, y1, x, y);
        n1 = DotGridGradient(x1, y1, x, y);
        float ix1 = Mathf.Lerp(n0, n1, sx);

        return Mathf.Lerp(ix0, ix1, sy);
    }

    float DotGridGradient(int ix, int iy, float x, float y)
    {
        Vector2 gradient = perlinGrid[iy * gridSize + ix];

        float dx = x - ix;
        float dy = y - iy;

        return (dx * gradient.x + dy * gradient.y);
    }

    void SpawnObstaclesOnNavMesh()
    {
        for (int i = 0; i < NoOfObstacles; i++)
        {
            Vector3 spawnPosition = spawner.GetRandomPointOnNavMesh();
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            var obstacle = Instantiate(obstaclesPrefab, spawnPosition, randomRotation);

        }
    }

     

}
