using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateObject : MonoBehaviour
{
    private LevelManager _levelManager;

    private void Start()
    {
        _levelManager = GameObject.FindObjectOfType<LevelManager>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.transform.CompareTag("Ghost")) //If a ghost just went through this gate
        {
            collision.GetComponent<GhostObject>().SetNodes(_levelManager.GetNodes(false));
        }
    }
}
