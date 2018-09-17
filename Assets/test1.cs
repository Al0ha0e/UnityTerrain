using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test1 : MonoBehaviour
{
    /*
    public GameObject player;
    public GameObject obj;
    public int Visibility;
    private GameObject[,,] Grid;
    private int sum;
    public void Start()
    {
        sum = (Visibility << 1) | 1;
        Grid = new GameObject[sum, sum,sum];
        for(int i = -Visibility;i<=Visibility;i++)
            for(int j  = -Visibility;j<=Visibility;j++)
                for(int k = -Visibility; k<=Visibility;k++)
                    Grid[i+Visibility, j+Visibility,k+Visibility] = GameObject.Instantiate(obj, new Vector3(i*4,k*4,j*4), Quaternion.identity);
    }
    public void Update()
    {
        Vector3Int pos = new Vector3Int(Mathf.FloorToInt(player.transform.position.x / 4.0f), Mathf.FloorToInt(player.transform.position.y / 4.0f), Mathf.FloorToInt(player.transform.position.z / 4.0f));
        
        for (int i = -Visibility; i <= Visibility; i++)
            for (int j = -Visibility; j <= Visibility; j++)
                for(int k = -Visibility;k<=Visibility;k++)
                {
                    Vector3Int ss = new Vector3Int(((pos.x + i) % sum + sum) % sum, ((pos.y + k) % sum + sum) % sum,((pos.z + j) % sum + sum) % sum);
                    if (Grid[ss.x, ss.y, ss.z].transform.position != new Vector3((pos.x + i) * 4, (pos.y + k) * 4, (pos.z + j) * 4)) 
                    {
                        GameObject.Destroy(Grid[ss.x, ss.y,ss.z], 0.0f);
                        Grid[ss.x, ss.y,ss.z] = GameObject.Instantiate(obj, new Vector3((pos.x + i) * 4, (pos.y + k) * 4, (pos.z + j) * 4), Quaternion.identity);
                    }
                }  
    }
    */
}