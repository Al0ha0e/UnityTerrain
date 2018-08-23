using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainVoxel : MonoBehaviour
{
    public ComputeShader Master;
    public Material material;
    public Vector3 Pos;// TEMP
    public float Radius;//TEMP
    public float Size;
    private Chunk ck;
    private Chunk ck2;
    void Start()
    {
        ck = new Chunk(Size,Master);
        ck2 = new Chunk(Size, Master);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            ck.Dig(Pos,Radius);
            ck2.Dig(Pos, Radius);
        }
    }
  
    void OnRenderObject()
    {
        material.SetBuffer("Edges", ck.Edges);
        material.SetBuffer("Vertices", ck.Vertices);
        material.SetBuffer("Normals", ck.Normals);
        material.SetVector("_Position", new Vector4(transform.position.x, transform.position.y, transform.position.z, ck.Size));
        material.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points,ck.vertcount);
        material.SetBuffer("Edges", ck2.Edges);
        material.SetBuffer("Vertices", ck2.Vertices);
        material.SetBuffer("Normals", ck2.Normals);
        material.SetVector("_Position", new Vector4(transform.position.x+20.0f, transform.position.y, transform.position.z, ck2.Size));
        material.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, ck2.vertcount);
    }
    

    void OnDestroy()
    {
        ck.Release();
        ck2.Release();
    }
}
