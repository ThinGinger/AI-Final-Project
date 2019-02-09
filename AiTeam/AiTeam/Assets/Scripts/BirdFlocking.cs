using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFlocking : MonoBehaviour {

    public int maxBirds;
    public GameObject bird;

    List<GameObject> birds = new List<GameObject>();


    //Obstacles
    GameObject[] obstacles;
    List<GameObject> obstacleList = new List<GameObject>();

    // Use this for initialization
    void Start () {

        for (int i = 0; i < maxBirds; i++)
        {
            birds.Add(Instantiate(bird, Vector3.zero, transform.rotation));
        }

        foreach (GameObject bird in birds)
        {
            bird.SetActive(true);
            bird.GetComponent<Bird>().flock = this;
        }


        // obstacles
        obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        obstacleList = new List<GameObject>(obstacles);
        Debug.Log(obstacles.Length);


    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public List<GameObject> GetBirds()
    {
        return birds;
    }

    public List<GameObject> GetObstacles()
    {
        return obstacleList;
    }
}
