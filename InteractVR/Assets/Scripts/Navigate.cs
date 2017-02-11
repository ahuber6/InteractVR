﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Navigate : MonoBehaviour {

    public float speed;
    private GvrHead head;

	//Sets the speed of the movement and grabs the head object for referencing the persons gaze direction
	void Start () {
        speed = 0.1f;
        head = FindObjectOfType<GvrHead>();
    }
	
    //Continuously check for the navigation button being pressed
	void FixedUpdate () {

        //Moves the camera forward in the direction you are looking
        if (Input.GetButton("Fire3"))
        {
            this.transform.position += speed * head.Gaze.direction;             
        }

	}
}
