using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test1 : MonoBehaviour
{
    public ComputeShader cs;
    private ComputeBuffer terrain;
    private int kernel;
    private float[] ans;
    public void Start()
    {
        kernel = cs.FindKernel("CSMain");
        terrain = new ComputeBuffer(16, 4);
        cs.SetBuffer(kernel, "terrain", terrain);
        cs.Dispatch(kernel, 4, 4, 1);
        ans = new float[16];
        terrain.GetData(ans);
        for(int i=0;i<4;i++)
        {
            for(int j = 0;j<4;j++)
            {
                print(new Vector2(i, j));
                print(ans[i * 4 + j]);
            }
        }
    }
}