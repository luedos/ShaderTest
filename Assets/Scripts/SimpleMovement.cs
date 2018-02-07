using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour {

    public float Speed = 5f;


    private float JumpTimer = 0f;
    private bool IsInJump = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float x = Input.GetAxis("Horizontal") * Time.deltaTime * Speed;
        float z = Input.GetAxis("Vertical") * Time.deltaTime * Speed;

        Vector3 MovingVector = new Vector3(x, 0, z);
        
        transform.Translate(MovingVector);

        if (Input.GetAxis("Jump") > 0 && !IsInJump)
        {
            GetComponent<Rigidbody>().AddForce(MovingVector / 2 + Vector3.up * 500);
            JumpTimer = 1.5f;
            IsInJump = true;
        }


        if(JumpTimer > 0)
        {
            JumpTimer -= Time.deltaTime;
            if(JumpTimer < 0)
            {
                JumpTimer = 0;
                IsInJump = false;
            }
        }
    }
}
