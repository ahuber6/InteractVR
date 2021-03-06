﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RuntimeGizmos;

//Base class for the Transform Tools (Translator, Rotator, Scaler)
public abstract class TransformTool : MonoBehaviour
{
	public GameObject emptyParentContainer;

	//Transform of object being manipulated
	public Transform obj = null;

	//Reference to the script handling all of the Transformation Gizmo work
	public TransformGizmo gizmoScript;

	//Reference to the BasicObject script associated with the object being transformed
	public BasicObject objScript;

	//The billboard that corresponds to this button
	public GameObject Billboard { get; set; }

	//True if a given tool is active
	private bool _active = false;

	public bool Active {
		get { return _active; }
		set {
			_active = value;

			if (_active) {
				if (Manager.activeTransformGizmo)
					Debug.Log ("Activating a transformation tool, but the Manager says a tool is already active!");
			}

			Manager.activeTransformGizmo = value;
		}
	}



	void Start ()
	{
		setObjectandBillboard ();

		objScript = obj.gameObject.GetComponent<BasicObject> ();
		if (objScript == null)
			Debug.Log ("Could not grab a reference to the BasicObject script for " + transform.name);
		

		gizmoScript = GameObject.Find ("Main Camera").GetComponent<TransformGizmo> ();
		if (gizmoScript == null)
			Debug.Log ("Could not grab a reference to the TransformGizmo script on the Main Camera");
	}

	protected void onClick ()
	{
		//Ensure we have a reference to the object before enabling the tool
		if (obj != null) {
			//Enable the tool, but disable it instead if the user clicks the button again while it is already enabled
			if (!Active)
				enableTool ();
			else
				disableTool ();
		} else {
			Debug.Log ("No reference to the object being transformed. Did not enable the Transform Gizmo.");
		}
	}

	//Enable a particular transformation tool
	protected virtual void enableTool ()
	{
		if (gizmoScript == null)
			return;

		//Ensure no other tool is enabled already (even on a different billboard)
		Manager.disableAllTransformTools ();

		//Ensure the rigidbody is frozen so that physics won't interfere with transformations
		objScript.disableMotion ();

		Active = true;

		//Change the object's layer so that the laser will ignore it while a tool is enabled
		obj.gameObject.layer = LayerMask.NameToLayer ("Ignore Raycast");

		gizmoScript.SetTarget (obj);
	}

	//Disable the current transformation tool
	protected virtual void disableTool ()
	{
		//Don't try to disable an inactive tool
		if (!Active)
			return;
		
		//Set the TrasnformGizmo target back to null (no longer manipulating an object)
		gizmoScript.SetTarget (null);

		//Indicate the current Transform tool is no longer active
		Active = false;

		//Change the object's layer back to Movable
		if (obj != null)
			obj.gameObject.layer = LayerMask.NameToLayer ("Movable");
	}

	//Grab a reference to the GameObject being manipulated, the empty parent, and the object's billboard
	void setObjectandBillboard ()
	{
		//Current hierarchy: this button -> Slot (grid layout) -> Billboard -> Empty parent object wrapper
		//Empty parent object wrapper has 2 children: billboard and model object
		Transform billboard = transform.parent.transform.parent;
		if (billboard == null) {
			Debug.Log ("Could not grab a reference to the Billboard associated with this object.");
			return;
		}

		Billboard = billboard.gameObject;
		emptyParentContainer = billboard.parent.gameObject;
		obj = emptyParentContainer.transform.GetChild (0);
	}

	//Detach the billboard as a child object (so it will not be affected by rotations/scaling)
	protected void detachBillboard ()
	{
		if (Billboard.transform.parent != null) {
			Vector3 temp = Billboard.transform.position;
			Billboard.transform.SetParent (null, true);
			Billboard.transform.position = temp;
		}
	}

	//Reatach the billboard as a child object
	protected void reattachBillboard ()
	{
		if (obj != null && Billboard != null && Billboard.transform.parent == null) {
			Billboard.transform.SetParent (obj, true);
		}
	}
}
