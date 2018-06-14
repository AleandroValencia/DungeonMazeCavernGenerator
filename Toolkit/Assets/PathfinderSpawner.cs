using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfinderSpawner : MonoBehaviour
{

    public GameObject PathFinder;
    public Transform[] Objective;
	public Transform Player;
    public int objectiveNumber = 0;

	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (GameObject.FindGameObjectWithTag("PathFinder") == null)
            {
				//print (Player.position);
				GameObject pathfinder = Instantiate(PathFinder, this.transform.position + this.transform.forward,  this.transform.rotation);
				print (pathfinder.transform.position);
                pathfinder.GetComponent<PathFinderScript>().target = Objective[objectiveNumber];
                Destroy(pathfinder, 5);
            }
        }
    }
}
