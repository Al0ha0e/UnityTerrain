using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Vector3[] TEMP; //DEBUG
    public float Size;
    public ComputeShader Master;
    private ComputeBuffer Densities;
    private ComputeBuffer Voxels;
    public ComputeBuffer Edges;
    public ComputeBuffer Vertices;
    public ComputeBuffer Normals;
    private ComputeBuffer Counters;
    private int[] CountersIniter = { 0 };
    private Vector3[] vertices;
    private int InitKernel1;//calc and fill the densities buffer
    private int InitKernel2;//calc the voxels buffer
    private int InitKernel3;//calc and fill the edges buffer,clear the vertices buffer
    private int DrawKernel;//calc and fill the vertices buffer,calc and fill the normals buffer
    private int DigKernel;//finally, this kernel will be set in another class especially designed for spades
    public int vertcount;
    public Chunk(float size,ComputeShader master)
    {
        this.Size = size;
        this.Master = master;
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
        Master.Dispatch(InitKernel1, 1, 1, 19);
        Draw();
        //print(ans[0]);
    }
    public void Dig(Vector3 Pos,float Radius)
    {
        Master.SetVector("SpadePos", Pos);
        Master.SetFloat("Radius", Radius);
        Master.SetBuffer(DigKernel, "DigKernelDensities", Densities);//
        Master.Dispatch(DigKernel, 1, 19, 1);
        Draw();
    }
    private void Draw()
    {
        Master.SetBuffer(InitKernel2, "InitKernel2Densities", Densities);
        Master.SetBuffer(InitKernel2, "InitKernel2Voxels", Voxels);
        Master.Dispatch(InitKernel2, 1, 1, 18);
        Master.SetBuffer(InitKernel3, "InitKernel3Densities", Densities);
        Master.SetBuffer(InitKernel3, "InitKernel3Edges", Edges);
        Master.SetBuffer(InitKernel3, "InitKernel3Vertices", Vertices);
        Master.SetBuffer(InitKernel3, "InitKernel3Normals", Normals);
        Master.Dispatch(InitKernel3, 1, 18, 1);
        //
        TEMP = new Vector3[300];
        Vertices.GetData(TEMP);
        //
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
