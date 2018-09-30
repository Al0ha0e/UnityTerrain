using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testFPS : MonoBehaviour
{
    public float speed;
	void Update ()
    {
        if (Input.GetKey(KeyCode.W))//forward
        {
            this.transform.Translate(new Vector3(0.0f, 0.0f, speed * Time.deltaTime));
        }
        if(Input.GetKey(KeyCode.S))//back
        {
            this.transform.Translate(new Vector3(0.0f, 0.0f, -speed * Time.deltaTime));
        }
        if(Input.GetKey(KeyCode.D))
        {
            this.transform.Translate(new Vector3(speed * Time.deltaTime, 0.0f, 0.0f));
            
        }
        if (Input.GetKey(KeyCode.A))
        {
            this.transform.Translate(new Vector3(-speed * Time.deltaTime, 0.0f, 0.0f));
        }
	}
}
