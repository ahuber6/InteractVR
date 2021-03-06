﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InstantiateObject : MonoBehaviour
{

	Dictionary<string, GameObject> Objects;
	UnityEngine.WWW www;
	GameObject[] sceneObjects;

	private GameObject newObj { get; set; }

	private GameObject Camera { get; set; }

	private string buildNo;
	public GameObject returnedObj;

	void Awake ()
	{
		Objects = new Dictionary<string, GameObject> ();
		Camera = GameObject.Find ("Main Camera");
	}

	public void loadObject ()
	{
		Debug.Log ("loadObject");
		StartCoroutine (Loading ());
	}

	IEnumerator Loading ()
	{
		string asset;
		Scene newScene;

		//Wait until the cache is ready to be used
		while (!UnityEngine.Caching.ready) {
			yield return null;
		}

		asset = "https://s3.amazonaws.com/immersacad-storage/demos/utk/" + buildNo + "_Android.unity3d";

		//Pull the asset bundle from either the cache or the web.
		www = UnityEngine.WWW.LoadFromCacheOrDownload (asset, 0, 0);

		//Wait for the model to load into the scene before continuing
		while (!www.isDone) {
			yield return null;
		}
        


		//Instantiates the asset bundle that was downloaded
		if (www != null) {
			UnityEngine.AssetBundle bundle = www.assetBundle;
		}

		//Loads the scene using the Build Number
		SceneManager.LoadScene (buildNo, LoadSceneMode.Additive);

        
		//Grabs the Object from the scene and makes the objects in the created scene invisible
		newScene = SceneManager.GetSceneByName (buildNo);
		while (!newScene.isLoaded) {
			yield return null;
		}

        
		sceneObjects = newScene.GetRootGameObjects ();

		/*
        foreach(Transform trans in sceneObjects[0].transform)
        {
            Debug.Log(trans.name);
            trans.localScale = new Vector3(1, 1, 1);
        }
		*/

		sceneObjects [0].SetActive (false);
        

		newObj = Instantiate (sceneObjects [0]);
		newObj.SetActive (false);
        
		//Remove the scene that was created for the object
		SceneManager.UnloadSceneAsync (buildNo);
		newScene = SceneManager.GetSceneByName (buildNo);
		while (newScene.isLoaded) {
			yield return null;
		}

		//Add it to the dictionary for future uses and make the object visible to the user.
		Objects.Add (buildNo, Instantiate (newObj));
		newObj.SetActive (true);
		newObj.transform.position = Camera.transform.position + (5 * Camera.transform.forward);
		attachComponents (newObj);
		returnedObj = newObj;

	}


	public void addObject (string build)
	{
        
		GameObject current;
		GameObject main;
		buildNo = build;
		returnedObj = null;
		main = GameObject.FindGameObjectWithTag ("MainCamera");

		//If the object is already in the dictionary, grab it so it can be instantiated
		if (Objects.ContainsKey (buildNo)) {
			current = (GameObject)Objects [buildNo];
			current = Instantiate (current);
			current.transform.position = Camera.transform.position + (5 * Camera.transform.forward);
			current.SetActive (true);
			attachComponents (current);
			returnedObj = current;
		}

        //Else load the scene with the object in it, remove the object from the scene and add it to the dictionary
        else {
			loadObject ();
		}

	}

	public void attachComponents (GameObject obj)
	{
		AddCollider (obj);
		addShader (obj);
		addBillboard (obj);
		AddRigidBody (obj);
	}

	public void AddRigidBody (GameObject obj)
	{
		Rigidbody rigidBod;

		rigidBod = obj.AddComponent<Rigidbody> ();
		rigidBod.useGravity = false;
	}

	//Removes colliders from the child objects making up the object and adds just one to the parent object
	public void AddCollider (GameObject obj)
	{
		Mesh objMesh;
		MeshFilter[] childrenMesh;
		MeshCollider objMeshCollider;
		MeshCollider[] children;
		children = obj.GetComponentsInChildren<MeshCollider> ();

		//Adds a single mesh collider to the parent object
		objMeshCollider = obj.GetComponent<MeshCollider> ();
		if (objMeshCollider == null) {
			objMeshCollider = obj.AddComponent<MeshCollider> ();
			objMesh = obj.GetComponent<Mesh> ();
			if (objMesh == null) {
				//Search children of object for a mesh if one is not found on the parent
				childrenMesh = obj.GetComponentsInChildren<MeshFilter> ();
				objMeshCollider.sharedMesh = childrenMesh [0].mesh;
               
			}
			obj.AddComponent<MeshRenderer> ();
            
			//Removes mesh colliders from the children of the main object
			foreach (MeshCollider element in children) {
				if (element != null) {
					Destroy (element);
				}
			}
		} 

		//Make the mesh collider convex to decrease computation complexity and to allow rigidbodies to be used
		objMeshCollider.convex = true;
	}

	//Adds a standard shader to all of the components of the Immersafied Object
	public void addShader (GameObject obj)
	{
		Renderer[] objRenderers;
		Material[] objMaterials;

		objRenderers = obj.GetComponentsInChildren<Renderer> ();

		foreach (Renderer element in objRenderers) {
			objMaterials = element.materials;
			foreach (Material mat in objMaterials) {
				mat.shader = Shader.Find ("Standard");
			}
		}
	}

	//Adds the generic billboard as a child of each of the Immersafied Objects
	public void addBillboard (GameObject obj)
	{
		GameObject Billboard;
		GameObject newBillboard;

		//Billboard = (GameObject)Resources.Load("Prefabs/Billboard");
		Billboard = (GameObject)Resources.Load ("Prefabs/NewBillboard");
		newBillboard = Instantiate (Billboard);
		newBillboard.SetActive (false);
		newBillboard.transform.SetParent (obj.transform);
		newBillboard.transform.position = new Vector3 (obj.transform.position.x, obj.transform.position.y + 2, obj.transform.position.z - 1);

		//Add the script for showing the billboard
		obj.AddComponent (typeof(ShowBillboard));

		//Add BasicObject script
		obj.AddComponent (typeof(BasicObject));
		obj.GetComponent<BasicObject> ().buildNo = buildNo;
	}
}
