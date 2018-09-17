using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spade
{
    public Vector3 Position;
    public ComputeShader SpadeShader;
    protected List<Chunk> Chunks;
    protected int DigKernel;
    public Spade(Vector3 pos)
    {
        Position = pos;
        Chunks = new List<Chunk>();
        DigKernel = SpadeShader.FindKernel("DigKernel");
    }
    public void Add(Chunk ck)
    {
        Chunks.Add(ck);
    }
	virtual public void Dig(){}
}
