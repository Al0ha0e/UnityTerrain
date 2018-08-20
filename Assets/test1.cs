using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test1 : MonoBehaviour
{
    public ComputeShader cs;
    private int Kernel;
    private ComputeBuffer cb;
	void Start ()
    {
        cb = new ComputeBuffer(4, 12);
        Kernel = cs.FindKernel("CSMain");
        cs.SetBuffer(Kernel, "cb", cb);
        Vector3 test = new Vector3(2.0f, 5.0f, 4.0f);
        cs.SetVector("TEST", test);
        cs.Dispatch(Kernel, 1, 1, 4);
        Vector3[] ans = new Vector3[4];
        cb.GetData(ans);
        foreach (Vector3 a in ans) print(a);
	}
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                print("up arrow key is held down");
            }
        }

}
