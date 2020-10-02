using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using static Utilities;

public class Node
{
    private int _timeToLive = 8;
    private float _x, _y;

    public Node(float x, float y) 
    {
        _x = x;
        _y = y;
    }
    
    public Vector2 Position()
    {
        return new Vector2(_x, _y);
    }

    public void Advance()
    {
        _timeToLive -= 1;        
    }

    public int TimeToLive()
    {
        return _timeToLive;
    }

}

public class GhostObject : MonoBehaviour
{
    private Transform[] _nodes, _playerNodes;
    private GhostEntity _ghostEntity;
    private LevelManager _levelManager;
    private PlayerObject _playerObject;

    private List<Node> _recentNodes;

    private Vector3 _target, _nextNode, _previousNode;
    private Vector2 _randomOffset;
    private GameObject _cursor, _bestMatchCursor;
    private float _speed;

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
        _speed = Random.Range(19.5f, 22.5f);
        _randomOffset.x = Random.Range(-2f, 2f);
        _randomOffset.y = Random.Range(-2f, 2f);

        _recentNodes = new List<Node>();

        _levelManager = GameObject.Find("LEVEL MANAGER").GetComponent<LevelManager>();        
        _playerObject = GameObject.Find("Player").GetComponent<PlayerObject>();

        _cursor = new GameObject("CURSOR");
        SpriteRenderer cursorSpriteRenderer = _cursor.AddComponent<SpriteRenderer>();
        cursorSpriteRenderer.sprite = LoadSprite("Sprites/cursor");

        _bestMatchCursor = new GameObject("BEST MATCH");
        SpriteRenderer bestMatchCursorSpriteRenderer = _bestMatchCursor.AddComponent<SpriteRenderer>();
        bestMatchCursorSpriteRenderer.sprite = LoadSprite("Sprites/cursor");
        bestMatchCursorSpriteRenderer.color = new Color32(66, 245, 96, 255);

        if(!Globals.ShowAITargeting)
        {
            cursorSpriteRenderer.enabled = false;
            bestMatchCursorSpriteRenderer.enabled = false;
        }

        _nodes = _levelManager.GetNodes(true);
        _playerNodes = _levelManager.GetNodes(false, true);
        _nextNode = transform.position;        



        //Subscribe to the OnTick event
        TickSystem.OnTick += delegate (object sender, TickSystem.OnTickEventArgs args)
        {            
            if (_playerNodes != null && _playerObject != null) //Fixes bug that says that some "Transform" has been destroyed when reloading a level.
            {
                Vector3 newTargetPos = _playerNodes[_playerObject.GetNodeIndex()].position;
                //+ new Vector3(_randomOffset.x, _randomOffset.y, 0);

                /*newTargetPos.x = Mathf.Clamp(newTargetPos.x, 0, _levelManager.CurrentLevel.Width);
                newTargetPos.y = Mathf.Clamp(newTargetPos.y, -_levelManager.CurrentLevel.Height, 0);*/

                _target = new Vector3(newTargetPos.x, newTargetPos.y, transform.position.z);

                _cursor.transform.position = _target + new Vector3(0, 0, -2);

                if (FindBestNode() != null)
                {
                    if (!IsRecentNode((Vector2)FindBestNode().position))
                    {                        
                        Vector3 newBestMatchPos = FindBestNode().position;
                        _recentNodes.Add(new Node(newBestMatchPos.x, newBestMatchPos.y));
                        _bestMatchCursor.transform.position = new Vector3(newBestMatchPos.x, newBestMatchPos.y, -2);
                        _nextNode = newBestMatchPos;                        
                    }
                }
            }

            UpdateRecentNodes();

        };
    }

    private void UpdateRecentNodes()
    {
        List<Node> nodesToRemove = new List<Node>();

        foreach(Node node in _recentNodes) //Advance or "age" every node.
        {
            node.Advance();
            if(node.TimeToLive() <= 0)
            {
                nodesToRemove.Add(node);
                //Debug.Log("removal queued");
            }
        }

        if (nodesToRemove.Count > 0)
        {
            foreach (Node node in nodesToRemove) //Remove nodes if there are any
            {
                _recentNodes.Remove(node);
            }
        }
    }

    private bool IsRecentNode(Vector2 nodeToCheck)
    {
        foreach(Node recentNode in _recentNodes)
        {
            if(recentNode.Position() == nodeToCheck)
            {
                return true;           
            }
        }

        return false;
    }

    private Transform FindMyNode()
    {
        Transform myNode = null;

        for(int i = 0; i < _nodes.Length; i++)
        {
            if(InRange(transform.position.x, _nodes[i].position.x, 0.05f) && InRange(transform.position.y, _nodes[i].position.y, 0.05f))
            {
                myNode = _nodes[i];
                break;
            }
        }

        return myNode;
    }

    private Transform FindLastNode() //Find the node that the ghost just visited
    {
        Transform lastNode = null;

        foreach(Transform node in _nodes)
        {
            switch(_currentDirection)
            {
                case 0:
                    if(InRange(node.position.y, _nextNode.y - 0.8f, 0.05f) && InRange(node.position.x, _nextNode.x, 0.05f))
                    {
                        lastNode = node;
                    }
                    break;

                case 1:
                    if (InRange(node.position.x, _nextNode.x - 0.8f, 0.05f) && InRange(node.position.y, _nextNode.y, 0.05f))
                    {
                        lastNode = node;
                    }
                    break;

                case 2:
                    if (InRange(node.position.y, _nextNode.y + 0.8f, 0.05f) && InRange(node.position.x, _nextNode.x, 0.05f))
                    {
                        lastNode = node;
                    }
                    break;

                case 3:
                    if (InRange(node.position.x, _nextNode.x + 0.8f, 0.05f) && InRange(node.position.y, _nextNode.y, 0.05f))
                    {
                        lastNode = node;
                    }
                    break;
            }
        }

        return lastNode;
    }

    private Transform FindBestNode() //Find the best node to go to based on the "target"
    {
        Transform myNode = FindMyNode();
        Transform bestMatch = null;

        if (myNode != null)
        {

            List<Transform> availableNodes = new List<Transform>(); //A list of nodes that the Ghost can move to, meaning those that are directly next to it.
            foreach (Transform node in _nodes)
            {                
                if (!IsRecentNode((Vector2)node.position))
                {
                    if ((Vector2)node.position == (Vector2)myNode.position + new Vector2(-0.8f, 0)) //Node directly to the left
                    {
                        availableNodes.Add(node);
                    }

                    else if ((Vector2)node.position == (Vector2)myNode.position + new Vector2(0.8f, 0)) //Node directly to the right
                    {
                        availableNodes.Add(node);
                    }

                    else if ((Vector2)node.position == (Vector2)myNode.position + new Vector2(0, 0.8f)) //Node directly above
                    {
                        availableNodes.Add(node);
                    }

                    else if ((Vector2)node.position == (Vector2)myNode.position + new Vector2(0, -0.8f)) //Node directly below
                    {
                        availableNodes.Add(node);
                    }
                }
            }

            /*if(_nextNode == FindMyNode().position) //If we are for some reason trying to move towards our own node then we look for new nodes.
            {                
                availableNodes.Clear();

                foreach(Transform node in _nodes)
                {
                    if ((Vector2)node.position == (Vector2)myNode.position + new Vector2(-0.8f, 0)) //Node directly to the left
                    {
                        availableNodes.Add(node);
                    }

                    else if ((Vector2)node.position == (Vector2)myNode.position + new Vector2(0.8f, 0)) //Node directly to the right
                    {
                        availableNodes.Add(node);
                    }

                    else if ((Vector2)node.position == (Vector2)myNode.position + new Vector2(0, 0.8f)) //Node directly above
                    {
                        availableNodes.Add(node);
                    }

                    else if ((Vector2)node.position == (Vector2)myNode.position + new Vector2(0, -0.8f)) //Node directly below
                    {
                        availableNodes.Add(node);
                    }
                }
            }*/

            float closestDistanceSquared = Mathf.Infinity;
            foreach (Transform node in availableNodes)
            {
                if (!IsRecentNode((Vector2)node.position))
                {
                    float distanceSquared = ((Vector2)_target - (Vector2)node.position).sqrMagnitude;
                    if (distanceSquared < closestDistanceSquared)
                    {
                        closestDistanceSquared = distanceSquared;
                        bestMatch = node;
                    }
                }
            }
        }

        return bestMatch;
    }

    private void Update()
    {
        if (_nextNode != null)
        {
            Vector2 myPos = transform.position; 

            if((Vector2)_nextNode != (Vector2)myPos)
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
}