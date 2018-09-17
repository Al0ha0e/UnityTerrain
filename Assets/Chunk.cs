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
    public int vertcount;
    public Chunk(int id)
    //public Chunk(float size,ComputeShader master)
    {
        //this.Size = size;
        ID = id;
        this.Master = TerrainVoxel.master;
        InitKernel1 = Master.FindKernel("InitKernel1");
        InitKernel2 = Master.FindKernel("InitKernel2");
        InitKernel3 = Master.FindKernel("InitKernel3");
        DrawKernel = Master.FindKernel("DrawKernel");
        DigKernel = Master.FindKernel("DigKernel");//
        Densities = new ComputeBuffer(19 * 19 * 19, 8);
        Master.SetBuffer(InitKernel1, "InitKernel1Densities", Densities);
        Master.SetBuffer(InitKernel2, "InitKernel2Densities", Densities);
        Master.SetBuffer(InitKernel3, "InitKernel3Densities", Densities);
        Master.SetBuffer(DigKernel, "DigKernelDensities", Densities);//
        Voxels = new ComputeBuffer(18 * 18 * 18, 4);
        Master.SetBuffer(InitKernel2, "InitKernel2Voxels", Voxels);
        Master.SetBuffer(DrawKernel, "DrawKernelVoxels", Voxels);
        Edges = new ComputeBuffer(18 * 19 * 19 * 3, 12);
        Master.SetBuffer(InitKernel3, "InitKernel3Edges", Edges);
        Master.SetBuffer(DrawKernel, "DrawKernelEdges", Edges);
        Vertices = new ComputeBuffer(18 * 18 * 18 * 5, 12);
        Master.SetBuffer(InitKernel3, "InitKernel3Vertices", Vertices);
        Master.SetBuffer(DrawKernel, "DrawKernelVertices", Vertices);
        Normals = new ComputeBuffer(18 * 19 * 19 * 3 * 4, 12);
        Master.SetBuffer(InitKernel3, "InitKernel3Normals", Normals);
        Master.SetBuffer(DrawKernel, "DrawKernelNormals", Normals);
        Counters = new ComputeBuffer(1, 4);
        Master.SetBuffer(DrawKernel, "DrawKernelCounters", Counters);
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
        /*
         if(IsNew(pos))
         {Master.Dispatch(InitKernel1, 1, 1, 19);}
         else
         {ReadFromDisk();}
         */
        return this;
    }

    public void Refresh()
    {

    }

    public void Dfs()
    {
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
