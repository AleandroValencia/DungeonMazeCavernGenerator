using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject waypointToDungeon;
    public GameObject waypointToMaze;
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "waypointDungeon")
        {
            this.GetComponent<PathfinderSpawner>().objectiveNumber++;
            this.gameObject.transform.position = waypointToDungeon.transform.position;
        }
        else if (col.tag == "waypointMaze")
        {
            this.GetComponent<PathfinderSpawner>().objectiveNumber++;
            this.gameObject.transform.position = waypointToMaze.transform.position;
        }
    }
}
