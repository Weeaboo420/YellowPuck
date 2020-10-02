using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    private Teleporter _connectedTeleporter;
    private bool _canActivate = true;    

    private void Start()
    {
        //Find the connected teleporter so we know what the other teleporter is.
        foreach(GameObject teleporter in GameObject.FindGameObjectsWithTag("Teleporter"))
        {
            if(teleporter != this.gameObject)
            {
                _connectedTeleporter = teleporter.GetComponent<Teleporter>();
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_canActivate)
        {
            if (collision.gameObject.CompareTag("Player")) //If the player entered the trigger
            {
                _connectedTeleporter.DisableActivation();

                //Spawn the "explosions"
                Instantiate(Resources.Load("Prefabs/ExplosionPrefab"), transform.position + new Vector3(0, 0, 0.3f), Quaternion.identity);
                Instantiate(Resources.Load("Prefabs/ExplosionPrefab"), _connectedTeleporter.transform.position + new Vector3(0, 0, 0.3f), Quaternion.identity);


                //"Teleport" the player
                Vector3 newPlayerPos = collision.gameObject.transform.position;
                newPlayerPos = new Vector3(_connectedTeleporter.transform.position.x, _connectedTeleporter.transform.position.y, newPlayerPos.z);
                collision.gameObject.transform.position = newPlayerPos;

                //Get the relevant data from the player
                PlayerObject playerObject = collision.gameObject.GetComponent<PlayerObject>();
                Transform[] playerNodes = playerObject.GetNodes();

                for(int i = 0; i < playerNodes.Length; i++) //Loop through the players' nodes and find the node that corresponds to the connected teleporter.
                {
                    if((Vector2)playerNodes[i].position == (Vector2)_connectedTeleporter.transform.position)
                    {
                        playerObject.SetNextNode(i);
                        break;
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player")) //If the player left the trigger
        {
            _canActivate = true;
        }
    }

    public void DisableActivation() //Called by the connected teleporter when the player enters it. This prevents the OnTriggerEnter event from firing once.
    {
        _canActivate = false;
    }
}
