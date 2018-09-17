using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpSpade : Spade
{
    public float Radius;
    public ExpSpade(Vector3 pos) : base(pos)
    {
        SpadeShader.SetFloat("Radius", Radius);
    }
    override public void Dig()
    {
        for(int i = 0;i<Chunks.Count;i++)
        {
            SpadeShader.SetVector("SpadePos", Position - Chunks[i].Position);
            SpadeShader.SetBuffer(DigKernel, "Densities", Chunks[i].Densities);
            SpadeShader.Dispatch(DigKernel, 1, 19, 1);
        }
        Chunks.Clear();
    }
}
