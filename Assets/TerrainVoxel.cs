using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainVoxel : MonoBehaviour
{
    public int Visibility; //Real Visibility = pow(16,Visibility)
    public int Resolution;// Real MinSize = pow(16,Resolution)
    public int MaxInstanceCount;
    public Material material;
    public ComputeShader Master;
    public static ComputeShader master;
    public GameObject Player;
    public static ChunkPool Pool;
    private Chunk[,,] ChunkGrid;
    public static List<Chunk> DrawList;
    public struct PrecomputeAttrib
    {
        public Vector4 Pos;
        public Chunk Fa;
        public PrecomputeAttrib(Vector4 pos,Chunk fa) { Pos = pos; Fa = fa; }
    }
    public static List<PrecomputeAttrib> PrecomputeList;
    public static float MinSize;
    private float maxsize;
    void Start()
    {
        master = Master;
        Pool = new ChunkPool(MaxInstanceCount);
        ChunkGrid = new Chunk[3, 3, 3];
        maxsize = Mathf.Pow(16, Visibility);
        MinSize = Mathf.Pow(16, Resolution);
        DrawList = new List<Chunk>();
        PrecomputeList = new List<PrecomputeAttrib>();
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                for (int k = -1; k <= 1; k++)
                {
                    ChunkGrid[(i + 3) % 3, (j + 3) % 3, (k + 3) % 3] = Pool.GetInstance(new Vector3(i, j, k), maxsize, "INIT");
                    ChunkGrid[(i + 3) % 3, (j + 3) % 3, (k + 3) % 3].Dfs(Player.transform.position);
                }              
    }
    void Update()
    {

        //Vector3Int pos = new Vector3Int(Mathf.FloorToInt((Player.transform.position.x + (maxsize)) / maxsize), Mathf.FloorToInt((Player.transform.position.y + (maxsize )) / maxsize), Mathf.FloorToInt((Player.transform.position.z + (maxsize )) / maxsize));
        Vector3Int pos = new Vector3Int(Mathf.FloorToInt((Player.transform.position.x ) / maxsize), Mathf.FloorToInt((Player.transform.position.y ) / maxsize), Mathf.FloorToInt((Player.transform.position.z ) / maxsize));
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                for (int k = -1; k <= 1; k++)
                {
                    Vector3Int ss = new Vector3Int(((pos.x + i) % 3 + 3) % 3, ((pos.y + j) % 3 + 3) % 3, ((pos.z + k) % 3 + 3) % 3);
                    if (ChunkGrid[ss.x, ss.y, ss.z].Position != new Vector3((pos.x + i), (pos.y + j), (pos.z + k))) 
                    {
                        Pool.Release(ChunkGrid[ss.x, ss.y, ss.z]);
                        ChunkGrid[ss.x, ss.y, ss.z] = Pool.GetInstance(new Vector3((pos.x + i), (pos.y + j), (pos.z + k)), maxsize, "MAIN_UPD");
                    }
                }
        foreach(Chunk t in ChunkGrid) { t.Dfs(Player.transform.position); }
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
        foreach (Chunk t in DrawList) { if (t.ID == -1) t.Release(); }
        Pool.Dispose();
    }
    
    static public bool IsNew(Vector3 position)
    {
        return true;
    }
}
