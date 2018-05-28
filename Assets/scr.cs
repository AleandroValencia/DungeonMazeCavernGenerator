using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class scr : MonoBehaviour 
{

	public Transform Target;
	// Use this for initialization
	void Start () 
	{
		this.GetComponent<NavMeshAgent> ().SetDestination (Target.position);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
