// Old BadGuy spawner by Joe. Replaced by EnemySpawner

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class BadGuyManager : MonoBehaviour {
    public List<GameObject> locations;

    public GameObject badguy;

    private GameObject curBadGuy = null;

    void OnEnable()
    {
        EventManager.StartListening("Destroy", PlaceBadGuy);
    }

    void OnDisable()
    {
        EventManager.StopListening("Destroy", PlaceBadGuy);
    }

    // Use this for initialization
    void Start () {
        PlaceBadGuy();
	}
	
	//// Update is called once per frame
	//void Update () {
	//
	//}

    void PlaceBadGuy()
    {
        if (curBadGuy != null)
        {
            Destroy(curBadGuy);
        }
        // Respawn Bad Guy at random location
        var t = locations[Random.Range(0, locations.Count)];
        curBadGuy = Instantiate<GameObject>(badguy);
        curBadGuy.transform.position = t.transform.position;
    }
}
