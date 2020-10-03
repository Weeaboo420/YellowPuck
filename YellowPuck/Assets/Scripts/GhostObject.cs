using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using static Utilities;


public class GhostObject : MonoBehaviour
{
    public Transform[] _nodes, _playerNodes, _combinedNodes;
    private GhostEntity _ghostEntity;
    private LevelManager _levelManager;
    private PlayerObject _playerObject;

    private List<GameObject> _cursors;    
    private int _tries;
    private const int _maxTries = 30;

    private Vector2 _nextNode, _target, _myNode;
    private List<Vector2> _path;
    private GameObject _cursor, _bestMatchCursor;
    private float _speed;
    private bool _shouldCalculatePath = false;

    private byte _currentDirection;
    /*
     * 0 = up 
     * 1 = right
     * 2 = down
     * 3 = left
     */

    private void Awake()
    {
        this.gameObject.transform.name = "Ghost";
        _ghostEntity = new GhostEntity("Ghost", 1, this.gameObject);
    }

    public void ToggleCursors()
    {
        _cursor.GetComponent<SpriteRenderer>().enabled = !_cursor.GetComponent<SpriteRenderer>().enabled;
        _bestMatchCursor.GetComponent<SpriteRenderer>().enabled = !_bestMatchCursor.GetComponent<SpriteRenderer>().enabled;
    }

    public void SetNodes(Transform[] newNodes)
    {
        _nodes = newNodes;
    }

    private void Start()
    {     
        _levelManager = GameObject.Find("LEVEL MANAGER").GetComponent<LevelManager>();        
        _playerObject = GameObject.Find("Player").GetComponent<PlayerObject>();

        _cursors = new List<GameObject>();

        _nodes = _levelManager.GetNodes(true);
        _playerNodes = _levelManager.GetNodes(false, true);
        _combinedNodes = _levelManager.GetNodes(true, true); //Used for pathfinding
        _path = new List<Vector2>();
    }

    private Transform FindMyNode(bool lookInCombinedNodes = false)
    {
        Transform myNode = null;

        if (!lookInCombinedNodes)
        {
            for (int i = 0; i < _nodes.Length; i++)
            {
                if (InRange(transform.position.x, _nodes[i].position.x, 0.05f) && InRange(transform.position.y, _nodes[i].position.y, 0.05f))
                {
                    myNode = _nodes[i];
                    break;
                }
            }
        }

        else
        {
            for (int i = 0; i < _combinedNodes.Length; i++)
            {
                if (InRange(transform.position.x, _combinedNodes[i].position.x, 0.05f) && InRange(transform.position.y, _combinedNodes[i].position.y, 0.05f))
                {
                    myNode = _nodes[i];
                    break;
                }
            }
        }

        return myNode;
    }


    private Vector2 FindClosestNode(List<Vector2> path, Vector2 currentNode)
    {
        Vector2 closestNode = Vector2.negativeInfinity; 
        List<Vector2> candidates = new List<Vector2>(); //A list of candidates for the next node to be added to the path
        float margin = 0.05f;

        foreach (Transform node in _combinedNodes)
        {
            if (!path.Contains((Vector2)node.position) && (Vector2)node.position != currentNode) //Make sure we are looking at nodes that are not inside the current path
            {
                if (InRange(node.position.x, currentNode.x + 0.8f, margin) && node.position.y == currentNode.y ||
                    InRange(node.position.x, currentNode.x - 0.8f, margin) && node.position.y == currentNode.y ||
                    InRange(node.position.y, currentNode.y + 0.8f, margin) && node.position.x == currentNode.x ||
                    InRange(node.position.y, currentNode.y - 0.8f, margin) && node.position.x == currentNode.x)
                {
                    candidates.Add((Vector2)node.position);
                }
            }
        }

        float closestDistanceSqr = Mathf.Infinity;
        foreach (Vector2 node in candidates)
        {
            if((_target - node).sqrMagnitude < closestDistanceSqr)
            {
                closestNode = node;
                closestDistanceSqr = (_target - node).sqrMagnitude;
            }
        }

        if (closestNode != Vector2.negativeInfinity)
        {
            /*closestNode.x = Mathf.Clamp(closestNode.x, 0.8f, _levelManager.CurrentLevel.Width * 0.8f);
            closestNode.y = Mathf.Clamp(closestNode.y, _levelManager.CurrentLevel.Height * -0.8f, 0.8f);*/
            return closestNode;
        }

        return Vector2.negativeInfinity;
    }
  
    private void CalculatePath()
    {
        if (_tries < _maxTries)
        {
            if (!_path.Contains(_target)) //As long as the "target" is not in "newPath" then keep looking for nodes that are closer and closer
            {
                if (_path.Count == 0)
                {
                    Vector2 firstNode = FindClosestNode(_path, _myNode);
                    if(firstNode != Vector2.negativeInfinity)
                    {
                        _path.Add(firstNode);
                        Debug.Log("a");
                    } else
                    {
                        Debug.Log("b");
                    }
                    
                }
                else
                {
                    Vector2 newNode = FindClosestNode(_path, _path[_path.Count - 1]);
                    if(newNode != Vector2.negativeInfinity)
                    {
                        _path.Add(newNode);
                        Debug.Log("C, target: " + _target + ", latest in path: " + _path[_path.Count-1]);
                    } else
                    {
                        Debug.Log("d");
                    }
                }

                _tries++;
            }

            else //If the target is in the calculated path, then draw the path (debug for now)
            {
                List<Vector2> nodesToRemove = new List<Vector2>();

                /*foreach(Vector2 node in _path)
                {
                    if(node.x <= Vector2.negativeInfinity.x && node.y <= Vector2.negativeInfinity.y)
                    {
                        nodesToRemove.Add(node);
                    }
                }*/

                foreach (Vector2 node in _path)
                {
                    GameObject newNode = new GameObject("node");
                    SpriteRenderer sr = newNode.AddComponent<SpriteRenderer>();
                    sr.sprite = LoadSprite("Sprites/cursor");
                    newNode.transform.position = new Vector3(node.x, node.y, -2);
                    _cursors.Add(newNode);
                }

                _shouldCalculatePath = false;
            }

        } else
        {
            _tries = 0;
            _shouldCalculatePath = false;
        }

    }

    private Vector2 FindPlayerInCombinedNodes()
    {
        Vector2 playerPos = Vector2.negativeInfinity;

        for(int i = 0; i < _combinedNodes.Length; i++)
        {
            Vector2 node = _combinedNodes[i].position;
            if(InRange(node.x, _playerObject.transform.position.x, 0.4f) && InRange(node.y, _playerObject.transform.position.y, 0.4f))
            {
                playerPos = new Vector2(node.x, node.y);
                break;
            }
        }

        if (playerPos != Vector2.negativeInfinity)
        {
            return playerPos;
        }

        return (Vector2)_combinedNodes[0].position;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R) && !_shouldCalculatePath)
        {
            if (FindPlayerInCombinedNodes() != Vector2.negativeInfinity)
            {
                foreach (GameObject cursor in _cursors)
                {
                    Destroy(cursor);
                }

                _cursors.Clear();

                //_target = _playerNodes[_playerObject.GetNodeIndex()].position;
                _target = FindPlayerInCombinedNodes();
                _myNode = FindMyNode(true).position;

                _path.Clear();
                _shouldCalculatePath = true;
            }
        }

        if(_shouldCalculatePath)
        {
            CalculatePath();
        }

        Vector2 myPos = transform.position; 

        if(_nextNode != (Vector2)myPos)
        {
            if(myPos.x > _nextNode.x)
            {
                _currentDirection = 3;
                myPos.x -= Time.deltaTime * _speed;
                myPos.x = Mathf.Clamp(myPos.x, _nextNode.x, myPos.x);
            }                
            else if(myPos.x < _nextNode.x)
            {
                _currentDirection = 1;
                myPos.x += Time.deltaTime * _speed;
                myPos.x = Mathf.Clamp(myPos.x, myPos.x, _nextNode.x);
            }

            if (myPos.y > _nextNode.y)
            {
                _currentDirection = 2;
                myPos.y -= Time.deltaTime * _speed;
                myPos.y = Mathf.Clamp(myPos.y, _nextNode.y, myPos.y);
            }
            else if(myPos.y < _nextNode.y)
            {
                _currentDirection = 0;
                myPos.y += Time.deltaTime * _speed;
                myPos.y = Mathf.Clamp(myPos.y, myPos.y, _nextNode.y);
            }
        }

        transform.position = Vector2.Lerp(transform.position, myPos, Time.deltaTime * 29f);        
    }
}