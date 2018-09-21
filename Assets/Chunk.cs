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
        //Master.Dispatch(InitKernel1, 1, 1, 19);
        //ChangedFlag = true;
        //Draw();
    }

    public Chunk Activate(Vector3 pos, float size)
    {
        Position = pos;
        Master.SetVector("Position", Position);
        Size = size;
        Master.SetFloat("Size", Size);
        Master.SetBuffer(InitKernel1, "InitKernel1Densities", Densities);
        Master.Dispatch(InitKernel1, 1, 1, 19);
        ChangedFlag = true;
         if(TerrainVoxel.IsNew(pos*Size))
         {
            Master.Dispatch(InitKernel1, 1, 1, 19);
         }
         else
         {
            //ReadFromDisk();
         }
        return this;
    }

    public void Refresh()
    {

    }

    public void Dfs(Vector3 PlayerPos)
    {
        Draw();
        TerrainVoxel.DrawList.Add(this);
        if (this.Size == TerrainVoxel.MinSize) return;
        if(PreSize.x!=0)
        {
            Master.SetBuffer(MaskClearKernel, "MaskClearKernelVoxels", Voxels);
            Master.SetVector("ClearStartPos", ClearStartPos);
            Master.Dispatch(MaskClearKernel, PreSize.x, PreSize.y, PreSize.z);
        }
        Vector3 PL = PlayerPos * 16 / Size, CK = Position * 16 / Size;
        Vector3 T = PL - CK;
        Vector3Int LD = new Vector3Int(100, 100, 100), RT = new Vector3Int(-1, -1, -1), T1 = new Vector3Int();
        int i, j, k, x, y, z;
        for(i=-2;i<=2;i++)
            for(j=-2;j<=2;j++)
                for(k=-2;k<=2;k++)
                {
                    x = ((int)(PL.x + i) % 5 + 5) % 5;
                    y = ((int)(PL.y + j) % 5 + 5) % 5;
                    z = ((int)(PL.z + k) % 5 + 5) % 5;
                    if (T.x + i < 0 || T.x + i > 17 || T.y + j < 0 || T.y + j > 17 || T.z + k < 0 || T.z + k > 17)
                    {
                        if (ChunkGrid[x, y, z] != null) TerrainVoxel.Pool.Release(ChunkGrid[x, y, z].ID);
                        ChunkGrid[x, y, z] = null;
                        continue;
                    }
                    if (i > -2 && i < 2 && j > -2 && j < 2 && k > -2 && k < 2)
                    {
                        if (ChunkGrid[x, y, z] == null)
                        {
                            TerrainVoxel.print("INIT");
                            ChunkGrid[x, y, z] = TerrainVoxel.Pool.GetInstance(new Vector3(PL.x + i, PL.y + j, PL.z + k), Size / 16.0f);
                        }
                        T1.x = (int)T.x + i; T1.y = (int)T.y + j; T1.z = (int)T.z + k;
                        if (T1.x <= LD.x && T1.y <= LD.y && T1.z <= LD.z) { LD = T1; }
                        else if (T1.x >= LD.x && T1.y >= LD.y && T1.z >= LD.z) { RT = T1; }
                        continue;
                    }
                    if (ChunkGrid[x, y, z] == null) { TerrainVoxel.PrecomputeList.Add(new TerrainVoxel.PrecomputeAttrib(new Vector4(PL.x + i, PL.y + j, PL.z + k, Size / 16.0f),this)); }
                    else if (ChunkGrid[x, y, z].Position != new Vector3(PL.x + i, PL.y + y, PL.z + z))
                    {
                        TerrainVoxel.Pool.Release(ChunkGrid[x, y, z].ID);
                        ChunkGrid[x, y, z] = null;
                        TerrainVoxel.PrecomputeList.Add(new TerrainVoxel.PrecomputeAttrib(new Vector4(PL.x + i, PL.y + j, PL.z + k, Size / 16.0f), this));
                    }
                }
        if(LD.x!=100)
        {
            PreSize = RT - LD + new Vector3Int(1,1,1);
            ClearStartPos.x = LD.x; ClearStartPos.y = LD.y; ClearStartPos.z = LD.z;
            Master.SetVector("SetClearStartPos", ClearStartPos);
            Master.SetBuffer(MaskSetKernel, "MaskSetKernelVoxels", Voxels);
            Master.Dispatch(MaskSetKernel, PreSize.x, PreSize.y, PreSize.z);
            for (i = 0; i < PreSize.x; i++)
                for (j = 0; j < PreSize.y; j++)
                    for (k = 0; k < PreSize.z; k++)
                        ChunkGrid[((int)ClearStartPos.x + i) % 5, ((int)ClearStartPos.y + j) % 5, ((int)ClearStartPos.z + k) % 5].Dfs(PlayerPos);
        }
        else { PreSize.x = 0; }
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
