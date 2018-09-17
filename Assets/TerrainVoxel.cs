using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainVoxel : MonoBehaviour
{
    public int Visibility; //Real Visibility = pow(16,Visibility)
    public int MaxInstanceCount;
    public Material material;
    public ComputeShader Master;
    public static ComputeShader master;
    public GameObject Player;
    private float maxsize;
    public static ChunkPool Pool;
    private Chunk[,,] ChunkGrid;
    public static List<Chunk> DrawList;
    public static List<Chunk> PrecomputeList;
    void Start()
    {
        master = Master;
        Pool = new ChunkPool(MaxInstanceCount);
        ChunkGrid = new Chunk[3, 3, 3];
        maxsize = Mathf.Pow(16, Visibility);
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                for (int k = -1; k <= 1; k++)
                    ChunkGrid[i + 1, j + 1, k + 1] = Pool.GetInstance(new Vector3(i * maxsize, j * maxsize, k * maxsize), maxsize);
        DrawList = new List<Chunk>();
    }
    void Update()
    {
        
        Vector3Int pos = new Vector3Int(Mathf.FloorToInt(Player.transform.position.x / maxsize), Mathf.FloorToInt(Player.transform.position.y / maxsize), Mathf.FloorToInt(Player.transform.position.z / maxsize));
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                for (int k = -1; k <= 1; k++)
                {
                    Vector3Int ss = new Vector3Int(((pos.x + i) % 3 + 3) % 3, ((pos.y + j) % 3 + 3) % 3, ((pos.z + k) % 3 + 3) % 3);
                    if (ChunkGrid[ss.x, ss.y, ss.z].Position != new Vector3((pos.x + i) * maxsize, (pos.y + j) * maxsize, (pos.z + k) * maxsize))
                    {
                        Pool.Release(ChunkGrid[ss.x, ss.y, ss.z].ID);
                        ChunkGrid[ss.x, ss.y, ss.z] = Pool.GetInstance(new Vector3((pos.x + i) * maxsize, (pos.y + j) * maxsize, (pos.z + k) * maxsize), maxsize);
                    }
                }
        foreach(Chunk t in ChunkGrid) { t.Dfs(); }
    }
    /*
    private void Dig()
    {
        Spade sp = new Spade();
    }
    */
    void OnRenderObject()
    {
        foreach (Chunk t in DrawList)
        {
            material.SetBuffer("Edges", t.Edges);
            material.SetBuffer("Vertices", t.Vertices);
            material.SetBuffer("Normals", t.Normals);
            material.SetVector("_Position", new Vector4(t.Position.x, t.Position.y, t.Position.z, t.Size));
            material.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Points, t.vertcount);
        }
        DrawList.Clear();
    }
    

    void OnDestroy()
    {
        Pool.Dispose();
    }
}
