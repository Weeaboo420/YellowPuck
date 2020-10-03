using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utilities;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerObject : MonoBehaviour
{
    private PlayerEntity _playerEntity;    
    private SpriteRenderer _spriteRenderer;    

    private Transform[] _nodes;
    private Transform _nextNode, _currentNode;

    private LevelManager _levelManager;
    private int _currentNodeIndex;
    private byte _currentDirection = 1;
    /*  
     *  ^ = 0, up
     *  > = 1, right
     *  v = 2, down
     *  < = 3, left
     */    

    private void Awake()
    {
        this.gameObject.name = "Player";
        _playerEntity = new PlayerEntity("Player", 3, this.gameObject);
    }

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _levelManager = GameObject.Find("LEVEL MANAGER").GetComponent<LevelManager>();
        _nodes = _levelManager.GetNodes(false, true);

        int tempIndex = 0;
        Vector2 tempStartingNodePosition = Vector2.zero;
        
        for (int y = 0; y < _levelManager.CurrentLevel.Height; y++)
        {
            for (int x = 0; x < _levelManager.CurrentLevel.Width; x++)
            {
                tempIndex++;

                if (tempIndex == _playerEntity.StartingNodeIndex)
                {                    
                    tempStartingNodePosition = new Vector2((x * 0.8f) + 0.8f, y * -0.8f);
                    break;
                }                

            }
        }

        foreach (Transform node in _nodes)
        {
            if ((Vector2)node.position == tempStartingNodePosition)
            {
                _currentNode = node;
                _nextNode = _currentNode;
                break;
            }
        }               

        //Subscribe to the OnTick event
        TickSystem.OnTick += delegate (object sender, TickSystem.OnTickEventArgs args)
        {
            if (CanMove(_currentDirection))
            {
                ChangeDirection(_currentDirection);
                if (FindNode(_currentDirection))
                {
                    _nextNode = FindNode(_currentDirection);
                }
            }
        };
    }

    public PlayerEntity Entity
    {
        get
        {
            return _playerEntity;
        }
    }

    public Transform[] GetNodes()
    {
        return _nodes;
    }

    private void ChangeDirection(byte newDirection)
    {
        if(newDirection != _currentDirection)
        {
            _currentDirection = newDirection;
            switch(_currentDirection)
            {
                case 0:
                    SetSprite(LoadSprite("Sprites/puck_u"));
                    break;
                case 1:
                    _spriteRenderer.flipX = false;
                    SetSprite(LoadSprite("Sprites/puck_lr"));
                    break;
                case 2:
                    SetSprite(LoadSprite("Sprites/puck_d"));
                    break;
                case 3:
                    _spriteRenderer.flipX = true;
                    SetSprite(LoadSprite("Sprites/puck_lr"));                    
                    break;

            }
        }
    }

    private void SetSprite(Sprite newSprite)
    {
        if (newSprite != null)
        {
            _spriteRenderer.sprite = newSprite;
        } else
        {
            Debug.LogWarning("New sprite was null");
        }
    }

    public void SetNextNode(int newNextNodeIndex)
    {
        _nextNode = _nodes[newNextNodeIndex];
    }

    private Transform FindNode(byte wantedDirection)
    {
        Transform wantedNode = null;
        Vector2 wantedPosition = (Vector2)_currentNode.position;

        switch(wantedDirection)
        {
            case 0:
                wantedPosition.y += 0.8f;
                break;
            case 1:
                wantedPosition.x += 0.8f;
                break;
            case 2:
                wantedPosition.y -= 0.8f;
                break;
            case 3:
                wantedPosition.x -= 0.8f;
                break;
        }
        
        for(int i = 0; i < _nodes.Length; i++)
        {
            if(InRange(_nodes[i].position.x, wantedPosition.x, 0.001f) && InRange(_nodes[i].position.y, wantedPosition.y, 0.001f))
            {
                wantedNode = _nodes[i];
                _currentNodeIndex = i;
                break;
            }
        }

        return wantedNode;
    }

    public int GetNodeIndex() //Get the index of the current node the player is on.
    {
        for(int i = 0; i < _nodes.Length; i++)
        {
            if((Vector2)_nodes[i].transform.position == (Vector2)transform.position)
            {
                _currentNodeIndex = i;
                break;
            }
        }
        return _currentNodeIndex;
    }

    private bool CanMove(byte wantedDirection)
    {
        if (_nextNode != null && InRange(transform.position.x, _nextNode.position.x, 0.35f) && InRange(transform.position.y, _nextNode.position.y, 0.35f))
        {
            if (FindNode(wantedDirection) != null)
            {
                return true;
            }
        }

        return false;
    }

    private void Update()
    {
        Vector2 myPos = transform.position;

        if (Input.GetKey(KeyCode.D)) //Right
        {
            if (CanMove(1))
            {                    
                ChangeDirection(1);                
            }
        }

        else if (Input.GetKey(KeyCode.A)) //Left
        {
            if (CanMove(3))
            {                    
                ChangeDirection(3);                
            }
        }
        
        else if (Input.GetKey(KeyCode.W)) //Up
        {
            if (CanMove(0))
            {                    
                ChangeDirection(0);                
            }
        }

        else if (Input.GetKey(KeyCode.S)) //Down
        {
            if (CanMove(2))
            {                    
                ChangeDirection(2);                
            }
        }

        if (_nextNode)
        {
            _currentNode = _nextNode;

            if ((Vector2)myPos != (Vector2)_nextNode.position)
            {
                if (myPos.x > _nextNode.position.x)
                {
                    myPos.x -= Time.deltaTime * 4f;
                    myPos.x = Mathf.Clamp(myPos.x, _nextNode.position.x, myPos.x);
                }
                else
                {
                    myPos.x += Time.deltaTime * 4f;
                    myPos.x = Mathf.Clamp(myPos.x, myPos.x, _nextNode.position.x);
                }

                if (myPos.y > _nextNode.position.y)
                {
                    myPos.y -= Time.deltaTime * 4f;
                    myPos.y = Mathf.Clamp(myPos.y, _nextNode.position.y, myPos.y);
                }
                else
                {
                    myPos.y += Time.deltaTime * 4f;
                    myPos.y = Mathf.Clamp(myPos.y, myPos.y, _nextNode.position.y);
                }
                
                transform.position = Vector2.Lerp(transform.position, myPos, Time.deltaTime * 3f * 200f);
            }

        }

    }

}
