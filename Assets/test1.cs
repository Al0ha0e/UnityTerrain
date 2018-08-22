using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test1 : MonoBehaviour
{
    private float[] A;
    private ComputeBuffer A_;
    public ComputeShader cs;
    private int cnt = 0;
    private int Kernel;
    private void Start()
    {
        A = new float[19];
        A_ = new ComputeBuffer(19, 4);
        Kernel = cs.FindKernel("CSMain");
        cs.SetBuffer(Kernel,"A",A_);
    }
    void Update()
    {
        if(cnt==0)
        {
            cnt++;
            cs.Dispatch(Kernel,1,1,1);
            A_.GetData(A);
            print(transform.name);
            for (int i = 0; i <= 18; i++) print(A[i]);
        }
    }
}