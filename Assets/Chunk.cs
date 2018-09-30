using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public int ID;
    public float Size;
    public Vector3 Position;
    public ComputeShader Master;
    public ComputeBuffer Densities;
    private ComputeBuffer Voxels;
    public ComputeBuffer Edges;
    public ComputeBuffer Vertices;
    public ComputeBuffer Normals;
    private ComputeBuffer Counters;
    private bool ChangedFlag;
    private int[] CountersIniter = { 0 };
    private Vector3[] vertices;
    private int InitKernel0;//Clear the Voxel Buffer when activated
    private int InitKernel1;//calc and fill the densities buffer
    private int InitKernel2;//calc the voxels buffer
    private int InitKernel3;//calc and fill the edges buffer,clear the vertices buffer
    private int DrawKernel;//calc and fill the vertices buffer,calc and fill the normals buffer
    private int DigKernel;//finally, this kernel will be set in another class especially designed for spades
    private int MaskClearKernel;
    private int MaskSetKernel;
    public int vertcount;
    private Vector3 ClearStartPos;
    private Vector3Int PreSize;
    private Chunk[,,] ChunkGrid;
    public Chunk(int id)
    {
        ID = id;
        this.Master = TerrainVoxel.master;
        InitKernel0 = Master.FindKernel("InitKernel0");
        InitKernel1 = Master.FindKernel("InitKernel1");
        InitKernel2 = Master.FindKernel("InitKernel2");
        InitKernel3 = Master.FindKernel("InitKernel3");
        DrawKernel = Master.FindKernel("DrawKernel");
        DigKernel = Master.FindKernel("DigKernel");//
        MaskClearKernel = Master.FindKernel("MaskClearKernel");
        MaskSetKernel = Master.FindKernel("MaskSetKernel");
        Densities = new ComputeBuffer(19 * 19 * 19, 8);
        Voxels = new ComputeBuffer(18 * 18 * 18, 4);
        Edges = new ComputeBuffer(18 * 19 * 19 * 3, 12);
        Vertices = new ComputeBuffer(18 * 18 * 18 * 5, 12);
        Normals = new ComputeBuffer(18 * 19 * 19 * 3 * 4, 12);
        Counters = new ComputeBuffer(1, 4);
        ClearStartPos = new Vector3(0.0f, 0.0f, 0.0f);
        PreSize = new Vector3Int(1, 1, 1);
        ChunkGrid = new Chunk[5, 5, 5];
    }
    public Chunk Activate(Vector3 pos, float size)
    {
        Position = pos;
        Master.SetVector("Position", Position);
        Size = size;
        Master.SetFloat("Size", Size);
        Master.SetBuffer(InitKernel1, "InitKernel1Densities", Densities);
        Master.Dispatch(InitKernel1, 1, 1, 19);
        Master.SetBuffer(InitKernel0, "InitKernel0Voxels", Voxels);
        Master.Dispatch(InitKernel0, 1, 1, 18);
        ChangedFlag = true;
        if (TerrainVoxel.IsNew(Position * Size))
         {
            Master.Dispatch(InitKernel1, 1, 1, 19);
         }
         else
         {
            //ReadFromDisk();
         }
        PreSize = new Vector3Int(1, 1, 1);
        ClearStartPos = new Vector3(1, 1, 1);
        return this;
    }

    public void Refresh()
    {

    }

    public void Dfs(Vector3 PlayerPos)
    {
        if (this.Size > TerrainVoxel.MinSize)
        {
            float unit = Size / 16.0f;
            if (PreSize.x != 0)
            {
                Master.SetBuffer(MaskClearKernel, "MaskClearKernelVoxels", Voxels);
                Master.SetVector("ClearStartPos", ClearStartPos);
                Master.Dispatch(MaskClearKernel, PreSize.x, PreSize.y, PreSize.z);
                ChangedFlag = true;
            }
            Vector3 PL = PlayerPos / unit, CK = Position * 16.0f;
            PL.x = Mathf.FloorToInt(PL.x); PL.y = Mathf.FloorToInt(PL.y); PL.z = Mathf.FloorToInt(PL.z);
            Vector3 T = PL - CK;
            Vector3Int LD = new Vector3Int(100, 100, 100), RT = new Vector3Int(-100, -100, -100), T1 = new Vector3Int();
            int i, j, k, x, y, z;
            for (i = -2; i <= 2; i++)
                for (j = -2; j <= 2; j++)
                    for (k = -2; k <= 2; k++)
                    {
                        T1.x = (int)T.x + i; T1.y = (int)T.y + j; T1.z = (int)T.z + k;
                        x = ((int)(T1.x) % 5 + 5) % 5;
                        y = ((int)(T1.y) % 5 + 5) % 5;
                        z = ((int)(T1.z) % 5 + 5) % 5;
                        if ((T1.x < 0) || (T1.x > 15) || (T1.y < 0) || (T1.y > 15) || (T1.z < 0) || (T1.z > 15))
                        {
                            if (ChunkGrid[x, y, z] != null) TerrainVoxel.Pool.Release(ChunkGrid[x, y, z]);
                            ChunkGrid[x, y, z] = null;
                            continue;
                        }
                        if (((i > -2) && (i < 2)) && ((j > -2) && (j < 2)) && ((k > -2) && (k < 2)))
                        {
                            if (ChunkGrid[x, y, z] == null)
                            {
                                ChunkGrid[x, y, z] = TerrainVoxel.Pool.GetInstance(new Vector3(PL.x + i, PL.y + j, PL.z + k), unit, "DFS_UPD");
                            }
                            if (T1.x <= LD.x && T1.y <= LD.y && T1.z <= LD.z) { LD = T1; }
                            if (T1.x >= LD.x && T1.y >= LD.y && T1.z >= LD.z) { RT = T1; }
                            continue;
                        }
                        if (ChunkGrid[x, y, z] == null) { TerrainVoxel.PrecomputeList.Add(new TerrainVoxel.PrecomputeAttrib(new Vector4(PL.x + i, PL.y + j, PL.z + k, unit), this)); }
                        else if (ChunkGrid[x, y, z].Position != new Vector3(PL.x + i, PL.y + j, PL.z + k))
                        {
                            TerrainVoxel.Pool.Release(ChunkGrid[x, y, z]);
                            ChunkGrid[x, y, z] = null;
                            TerrainVoxel.PrecomputeList.Add(new TerrainVoxel.PrecomputeAttrib(new Vector4(PL.x + i, PL.y + j, PL.z + k, unit), this));
                        }
                    }
            if (LD.x != 100)
            {
                ChangedFlag = true;
                PreSize = RT - LD + new Vector3Int(1, 1, 1);
                ClearStartPos = LD + new Vector3(1, 1, 1);
                //Master.SetVector("SetClearStartPos", new Vector3(1.0f, 1.0f, 1.0f)); //DEBUG
                Master.SetVector("SetClearStartPos", ClearStartPos);
                Master.SetBuffer(MaskSetKernel, "MaskSetKernelVoxels", Voxels);
                //Vector3Int DEBUG_VEC = new Vector3Int(15, 15, 15) - LD + new Vector3Int(1, 1, 1); //DEBUG
                //Master.Dispatch(MaskSetKernel, 10, 10, 10);//DEBUG
                Master.Dispatch(MaskSetKernel, PreSize.x, PreSize.y, PreSize.z);
                for (i = 0; i < PreSize.x; i++)
                    for (j = 0; j < PreSize.y; j++)
                        for (k = 0; k < PreSize.z; k++)
                        {
                            ChunkGrid[((int)LD.x + i) % 5, ((int)LD.y + j) % 5, ((int)LD.z + k) % 5].Dfs(PlayerPos);
                        }

            }
            else { PreSize.x = 0; }
        }
        Draw();
        TerrainVoxel.DrawList.Add(this);
    }

    public void Dig(Vector3 Pos,float Radius/*Spade sp*/)
    {
        Master.SetVector("SpadePos", Pos);
        Master.SetFloat("Radius", Radius);
        Master.SetBuffer(DigKernel, "DigKernelDensities", Densities);//
        Master.Dispatch(DigKernel, 1, 19, 1);
        ChangedFlag = true;
        //Draw();
        /*
        sp.Add(this);
        ChangedFlag = true;
        */
    }
    public void Draw()
    {
        if(ChangedFlag)
        {
            int[] DEBUG_VOXEL = new int[18 * 18 * 18];
            Master.SetBuffer(InitKernel2, "InitKernel2Densities", Densities);
            Master.SetBuffer(InitKernel2, "InitKernel2Voxels", Voxels);
            Master.Dispatch(InitKernel2, 1, 1, 18);
            Master.SetBuffer(InitKernel3, "InitKernel3Densities", Densities);
            Master.SetBuffer(InitKernel3, "InitKernel3Edges", Edges);
            Master.SetBuffer(InitKernel3, "InitKernel3Vertices", Vertices);
            Master.SetBuffer(InitKernel3, "InitKernel3Normals", Normals);
            Master.Dispatch(InitKernel3, 1, 18, 1);
            Master.SetBuffer(DrawKernel, "DrawKernelVoxels", Voxels);
            Master.SetBuffer(DrawKernel, "DrawKernelEdges", Edges);
            Master.SetBuffer(DrawKernel, "DrawKernelVertices", Vertices);
            Master.SetBuffer(DrawKernel, "DrawKernelNormals", Normals);
            Master.SetBuffer(DrawKernel, "DrawKernelCounters", Counters);
            Counters.SetData(CountersIniter);
            Master.Dispatch(DrawKernel, 1, 1, 18);
            int[] ans = new int[1];
            Counters.GetData(ans);
            vertcount = ans[0];
            ChangedFlag = false;
        }      
    }
    public void Release()
    {
        Densities.Release();
        Voxels.Release();
        Edges.Release();
        Vertices.Release();
        Counters.Release();
        Normals.Release();
    }
}
