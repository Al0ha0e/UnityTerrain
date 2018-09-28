using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkPool : IDisposable// this should have been Singletonlized 
{
    public static int InstanceCnt;//DEBUG
    public int[] IDs;
    public Chunk[] Pool;
    public int InstanceCount;
    ComputeShader Master;
    public ChunkPool(int instancecount)
    {
        InstanceCnt = 0;//DEBUG
        InstanceCount = instancecount;
        IDs = new int[instancecount + 1];
        Pool = new Chunk[instancecount];
        for(int i=0;i<instancecount;i++)
        {
            IDs[i + 1] = i;
            Pool[i] = new Chunk(i);
        }
        IDs[0] = InstanceCount;
    }
    public Chunk GetInstance(Vector3 pos,float size,string customer)
    {
        ////////////////////////////////////////////////////////////////////////
        InstanceCnt++;//DEBIG
        /*
        if(customer== "DFS_UPD")
        {
            TerrainVoxel.print(InstanceCnt);
            TerrainVoxel.print(customer);
            TerrainVoxel.print("------------------------------");
            TerrainVoxel.print(pos);
            TerrainVoxel.print(size);
            TerrainVoxel.print("------------------------------");
        }     
        */
        ///////////////////////////////////////////////////////////////////////////
        if (IDs[0]==0)//MLE
        {
            Chunk ret = new Chunk(-1);
            ret.Activate(pos, size);
            return ret;
        }
        else
        {
            Chunk ret = Pool[IDs[IDs[0]--]].Activate(pos, size);
            return ret;
        }
    }
    public void Release(Chunk obj)
    {
        if (obj.ID == -1)
        {
            obj.Release();
            return;
        }
        Pool[obj.ID].Refresh();
        IDs[++IDs[0]] = obj.ID;
    }
    public void Dispose()
    {
        for (int i = 0; i < InstanceCount; i++) Pool[i].Release();
    }
}
