using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainVoxel : MonoBehaviour
{
    public Vector3 Pos;// TMEP
    public float Radius;//TEMP
    public float Size;
    public Material material;
    public ComputeShader Master;
    private ComputeBuffer Densities;
    private ComputeBuffer Voxels;
    private ComputeBuffer Edges;
    private ComputeBuffer Vertices;
    private ComputeBuffer Normals;
    private ComputeBuffer Counters;
    private int[] CountersIniter = { 0 };
    private Vector3[] vertices;
    private int InitKernel1;//calc and fill the densities buffer
    private int InitKernel2;//calc the voxels buffer
    private int InitKernel3;//calc and fill the edges buffer,clear the vertices buffer
    private int DrawKernel;//calc and fill the vertices buffer,calc and fill the normals buffer
    private int DigKernel;//finally, this kernel will be set in another class especially designed for spades
    private int vertcount;
    void Start ()
    {
        InitKernel1 = Master.FindKernel("InitKernel1");
        InitKernel2 = Master.FindKernel("InitKernel2");
        InitKernel3 = Master.FindKernel("InitKernel3");
        DrawKernel = Master.FindKernel("DrawKernel");
        DigKernel = Master.FindKernel("DigKernel");//
        Densities = new ComputeBuffer(19 * 19 * 19, 8);
        Master.SetBuffer(InitKernel1, "InitKernel1Densities", Densities);
        Master.SetBuffer(InitKernel2, "InitKernel2Densities", Densities);
        Master.SetBuffer(InitKernel3, "InitKernel3Densities", Densities);
        Master.SetBuffer(DigKernel,"DigKernelDensities",Densities);//
        Voxels = new ComputeBuffer(18 * 18 * 18, 4);
        Master.SetBuffer(InitKernel2, "InitKernel2Voxels", Voxels);//DEBUG
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
        Counters.SetData(CountersIniter);
        Master.SetBuffer(DrawKernel, "DrawKernelCounters", Counters);
        Master.Dispatch(InitKernel1, 1, 1, 19);
        Master.Dispatch(InitKernel2, 1, 1, 18);
        Master.Dispatch(InitKernel3, 1, 18, 1);
        Master.Dispatch(DrawKernel, 1, 1, 18);
        int[] ans = new int[1];
        Counters.GetData(ans);
        vertcount =  ans[0];
        //print(ans[0]);
    }
    int GetEdge(Vector3Int pos)
    {
        int dx = pos.x & 1;
        int dz = pos.z & 1;
        int ret = dz * 12996 + dx * 6498;
        dx = 19 - dx;
        dz = 19 - dz;
        return ret + (pos.y >> 1) * dz * dx + (pos.x >> 1) * dz + (pos.z >> 1);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        { Dig(); }
    }
  
    void OnRenderObject()
    {
        material.SetVector("_Position", new Vector4(transform.position.x, transform.position.y, transform.position.z,Size));
        material.SetBuffer("Edges", Edges);
        material.SetBuffer("Vertices",Vertices);
        material.SetBuffer("Normals", Normals);
        material.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points,vertcount);
    }
    void Dig()
    {
        //print("Dig");
        Master.SetVector("SpadePos",Pos);
        Master.SetFloat("Radius", Radius);
        Master.Dispatch(DigKernel,1,19,1);
        Master.Dispatch(InitKernel2, 1, 1, 18);
        Master.Dispatch(InitKernel3, 1, 18, 1);
        Counters.SetData(CountersIniter);
        Master.Dispatch(DrawKernel, 1, 1, 18);
        int[] ans = new int[1];
        Counters.GetData(ans);
        vertcount = ans[0];
        print(vertcount);
    }

    void OnDestroy()
    {
        Densities.Release();
        Voxels.Release();
        Edges.Release();
        Vertices.Release();
        Counters.Release();
        Normals.Release();
    }
}
