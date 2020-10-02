using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using static Utilities;
using static Globals;
using UnityEngine.UI;

public class Level
{
    private int _width, _height;
    private float _cameraFov;
    private byte[] _data;

    /*
     * 0 = empty
     * 1 = wall
     * 2 = purple wall
     * 3 = player
     */

    public Level(int Width, int Height)
    {
        if(Width >= 10 && Height >= 10)
        {
            _width = Width;
            _height = Height;
        } else
        {
            Debug.LogError("Level width and height must be greater than or equal to 10!");
        }
    }

    public Level(int Width, int Height, byte[] Data, float CameraFov)
    {
        if (Width >= 10 && Height >= 10)
        {
            _width = Width;
            _height = Height;
        }
        else
        {
            Debug.LogError("Level width and height must be greater than or equal to 10!");
        }

        if(Data.Length == (_width * _height))
        {
            _data = Data;
        } else
        {
            Debug.LogError("Level data array does not have the appropriate length. It must be " + _width.ToString() + " * " + _height.ToString() + " (" + (_width * _height).ToString() + ")");
        }

        if(CameraFov >= 0.1f)
        {
            _cameraFov = CameraFov;
        }

    }

    public byte[] Data
    {
        get
        {
            return _data;
        }

        set
        {
            if(value.Length == (_width * _height))
            {
                _data = value;
            } else
            {
                Debug.LogError("Level data array does not have the appropriate length. It must be " + _width.ToString() + " * " + _height.ToString() + " (" + (_width * _height).ToString() + ")");
            }
        }

    }

    public float CameraFov
    {
        get
        {
            return _cameraFov;
        }

        set
        {
            if(value >= 0.1f)
            {
                _cameraFov = value;
            }
        }
    }

    public int Width
    {
        get
        {
            return _width;
        }
    }

    public int Height
    {
        get
        {
            return _height;
        }
    }

}

public class LevelManager : MonoBehaviour
{
    private Level _level;
    private byte[] _data;
    private Transform _levelParent;    

    private byte _ghostCount = 0;
    private const byte _maxGhostCount = 3;
    private List<string> _usedGhostColors;
    private List<Vector2> _usedGhostSpawns;

    private int _currentScore, _maxScore;
    private Text _scoreText;

    private static readonly Dictionary<Color32, byte> MapColors = new Dictionary<Color32, byte>() 
    {        
        {new Color32(255, 255, 255, 255), 1 }, //Wall
        {new Color32(102, 9, 143, 255), 2 }, //Purple Wall
        {new Color32(0, 0, 0, 255), 0 }, //Floor
        {new Color32(138, 138, 138, 255), 5 }, //False floor
        {new Color32(143, 9, 9, 255), 4 }, //Gate (horizontal)
        {new Color32(191, 104, 13, 255), 6 }, //Gate (vertical)
        {new Color32(255, 224, 0, 255), 3 }, //Player
        {new Color32(40, 79, 157, 255), 7 } //Teleporter
    };

    /*
     * 0 = empty / floor
     * 1 = wall
     * 2 = purple wall
     * 3 = player
     * 4 = gate (horizontal)
     * 5 = false floor (not added as a walkable node, no orb spawns)
     * 6 = gate (vertical)
     * 7 = teleporter
     */

    private byte[] MakeMapFromImage(string mapPath)
    {
        Texture2D rawImageData = Resources.Load<Texture2D>(mapPath);
        byte[] mapData = new byte[rawImageData.width * rawImageData.height];

        int i = 0;
        for(int y = rawImageData.height; y > 0; y--)
        {
            for(int x = 0; x < rawImageData.width; x++)
            {                
                mapData[i] = MapColors[(Color32)rawImageData.GetPixel(x, y-1)];                
                i++;
            }
        }

        return mapData;
    }

    private Vector2 GetMapSize(string mapPath)
    {
        Texture2D rawImageData = Resources.Load<Texture2D>(mapPath);
        Vector2 mapSize = Vector2.zero;

        if(rawImageData != null)
        {
            mapSize = new Vector2(rawImageData.width, rawImageData.height);
        }
        
        return mapSize;
    }

    private void Awake()
    {        
        _levelParent = GameObject.Find("Level").transform;
        _scoreText = GameObject.Find("scoreText").GetComponent<Text>();

        _data = MakeMapFromImage("Maps/map01");
        Vector2 _mapSize = GetMapSize("Maps/map01");
        _level = new Level((int)_mapSize.x, (int)_mapSize.y, _data, 9f);

        _usedGhostColors = new List<string>();
        _usedGhostSpawns = new List<Vector2>();

        GameObject mainCamera = GameObject.Find("Main Camera");
        mainCamera.GetComponent<Camera>().orthographicSize = _level.CameraFov;
        GameObject.Find("Background").transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 20);

        Vector3 newCameraPos = mainCamera.transform.position;
        newCameraPos = _levelParent.transform.position + new Vector3((_level.Width / 2f) * 0.8f, -(_level.Height / 2f) * 0.75f, -10f);
        mainCamera.transform.position = newCameraPos;


        GenerateLevel();        

    }

    public Level CurrentLevel
    {
        get
        {
            return _level;
        }
    }

    public void RemoveTeleporters() //Removes all teleporters if there are more than two teleporters present.
    {
        foreach(GameObject teleporter in GameObject.FindGameObjectsWithTag("Teleporter"))
        {
            Destroy(teleporter);
        }

        Debug.LogError("More than two teleporters found! Removing all teleporters!");
    }

    private void SpawnSingleNode(string name, string tag, string spritePath, float x, float y, int layer, int depth, Color32 color)
    {
        GameObject newNode = new GameObject(name);
        newNode.transform.parent = _levelParent;

        if (tag.Length > 0)
        {
            newNode.transform.tag = tag;
        }

        SpriteRenderer spriteRenderer = newNode.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = LoadSprite(spritePath);
        spriteRenderer.color = color;
        newNode.transform.position = new Vector3(x * 0.8f, y * -0.8f, depth);
        newNode.transform.gameObject.layer = layer;
    }

    public Transform[] GetNodes(bool includeFalseFloors = false, bool includeSpecialFloors = false)
    {                   
        List<Transform> nodes = new List<Transform>();

        List<string> tags = new List<string>();
        tags.Add("Floor");

        if(includeFalseFloors)
        {
            tags.Add("False Floor");
        }

        if(includeSpecialFloors)
        {
            tags.Add("Special Floor");
        }

        foreach(Transform child in _levelParent)
        {
            foreach(string tag in tags)
            {
                if(child.transform.CompareTag(tag) && !nodes.Contains(child))
                {
                    nodes.Add(child);
                }
            }
        }

        return nodes.ToArray();        
    }

    public void AddScore()
    {
        _currentScore++;
        _scoreText.text = "score: " + _currentScore.ToString() + "/" + _maxScore.ToString();
    }

    private void GenerateLevel()
    {
        int i = 0;
        int startingNodeIndex = 0;

        //Find all the keys in the Ordered dicitonary called GhostColors.
        List<string> keys = new List<string>();
        foreach (string key in Globals.GhostColors.Keys)
        {
            keys.Add(key);
        }

        for (int y = 0; y < _level.Height; y++)
        {
            for (int x = 0; x < _level.Width; x++)
            {
                string name = "";
                int depth = 0;                

                switch (_data[i])
                {
                    case 0:
                        name = "floor_" + i.ToString();
                        break;
                    case 1:
                        name = "wall_" + i.ToString();
                        break;
                    case 2:
                        name = "purple_wall_" + i.ToString();
                        break;
                    case 5:
                        name = "false_floor_" + i.ToString();
                        break;
                    default:                        
                        break;
                }

                if (_data[i] != 3) //Spawning of anything that isn't the player
                {
                    GameObject newNode = new GameObject(name);
                    newNode.transform.parent = _levelParent;

                    SpriteRenderer spriteRenderer = newNode.AddComponent<SpriteRenderer>();                    

                    switch (_data[i])
                    {
                        case 0: //Floor
                            
                            spriteRenderer.sprite = LoadSprite("Sprites/floor");
                            newNode.transform.tag = "Floor";
                            depth = 1;
                            spriteRenderer.color = FloorColorSecondary;
                            if ((i + y % 2) % 2 == 0)
                            {
                                spriteRenderer.color = FloorColorMain;
                            }                            

                            GameObject orb = Instantiate(LoadPrefab("Prefabs/OrbPrefab"), new Vector3(x * 0.8f, y * -0.8f, -1), Quaternion.identity);
                            orb.GetComponent<OrbObject>().LevelManager = this;
                            orb.transform.parent = _levelParent;
                            orb.transform.name = "orb_" + i.ToString();
                            
                            _maxScore++;
                            break;

                        case 1: //Wall
                            spriteRenderer.sprite = LoadSprite("Sprites/wall");
                            spriteRenderer.color = WallColor;
                            break;

                        case 2: //Purple Wall
                            spriteRenderer.sprite = LoadSprite("Sprites/wall");
                            spriteRenderer.color = PurpleWallColor;
                            break;

                        case 4: //Gate (horizontal)
                            Color32 newColor;
                            newColor = FloorColorSecondary;
                            if ((i + y % 2) % 2 == 0) //Every other floor tile gets the "main" floor color. This results in a checkered pattern on the floor.
                            {
                                newColor = FloorColorMain;
                            }

                            SpawnSingleNode("floor_" + i.ToString(), "False Floor", "Sprites/floor", x, y, 0, 1, newColor);
                            GameObject gateObject = (GameObject)Instantiate(Resources.Load("Prefabs/GatePrefab"), new Vector3(x * 0.8f, y * -0.8f, 0), Quaternion.identity);
                            gateObject.transform.parent = _levelParent;
                            break;

                        case 5: //False floor
                            spriteRenderer.sprite = LoadSprite("Sprites/floor");
                            spriteRenderer.color = FloorColorSecondary;
                            newNode.transform.tag = "False Floor";
                            if ((i + y % 2) % 2 == 0)
                            {
                                spriteRenderer.color = FloorColorMain;
                            }
                            depth = 1;

                            if (_ghostCount < _maxGhostCount && !_usedGhostSpawns.Contains(new Vector2(x * 0.8f, y * -0.8f)))
                            {
                                GameObject newGhost = (GameObject)Instantiate(Resources.Load("Prefabs/GhostPrefab"), new Vector3(x * 0.8f, y * -0.8f, -2), Quaternion.identity);
                                SpriteRenderer ghostSpriteRenderer = newGhost.GetComponent<SpriteRenderer>();

                                _usedGhostSpawns.Add(new Vector2(x * 0.8f, y * -0.8f));
                                
                                string colorKey = keys[Random.Range(0, keys.Count - 1)];
                                while(_usedGhostColors.Contains(colorKey)) //Find a key that hasn't yet been used.
                                {
                                    colorKey = keys[Random.Range(0, keys.Count - 1)];
                                }

                                _usedGhostColors.Add(colorKey);

                                ghostSpriteRenderer.color = (Color32)Globals.GhostColors[colorKey];

                                _ghostCount++;
                            }

                            break;

                        case 6: //Gate (vertical)                            
                            newColor = FloorColorSecondary;
                            if ((i + y % 2) % 2 == 0)
                            {
                                newColor = FloorColorMain;
                            }
                            SpawnSingleNode("floor_" + i.ToString(), "False Floor", "Sprites/floor", x, y, 0, 1, newColor);

                            gateObject = (GameObject)Instantiate(Resources.Load("Prefabs/GatePrefab"), new Vector3(x * 0.8f, y * -0.8f, 0), Quaternion.identity);
                            gateObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90f));
                            gateObject.transform.parent = _levelParent;
                            break;

                        case 7: //Teleporter
                            newColor = FloorColorSecondary;
                            if ((i + y % 2) % 2 == 0)
                            {
                                newColor = FloorColorMain;
                            }
                            SpawnSingleNode("special_floor_" + i.ToString(), "Special Floor", "Sprites/floor", x, y, 0, 1, newColor); //Add a "special" floor that only the player can walk on.

                            GameObject teleporterObject = (GameObject)Instantiate(Resources.Load("Prefabs/TeleporterPrefab"), new Vector3(x * 0.8f, y * -0.8f, -1), Quaternion.identity);
                            teleporterObject.transform.parent = _levelParent;
                            break;

                    }

                    if (depth == 0)
                    {
                        BoxCollider2D boxCollider2D = newNode.AddComponent<BoxCollider2D>();                        
                    }

                    newNode.transform.position = new Vector3(x * 0.8f, y * -0.8f, depth);
                    newNode.transform.gameObject.layer = 8;

                } else
                {

                    Color32 newColor;
                    newColor = FloorColorSecondary;
                    if ((i+ y % 2) % 2 == 0)
                    {
                        newColor = FloorColorMain;
                    }                    

                    //Spawn a floor object underneath the player                    
                    SpawnSingleNode("floor_" + i.ToString(), "Floor", "Sprites/floor", x, y, 8, 1, newColor);
                    startingNodeIndex = i;

                    Instantiate(LoadPrefab("Prefabs/PlayerPrefab"), new Vector3(x * 0.8f, y * -0.8f, -1), Quaternion.identity);                    
                    
                }

                i++;

            }
        }

        GameObject playerObject = GameObject.Find("Player");
        if (playerObject)
        {            
            playerObject.GetComponent<PlayerObject>().Entity.StartingNodeIndex = startingNodeIndex;
        }

        _scoreText.text = "score: " + _currentScore.ToString() + "/" + _maxScore.ToString();

        if(GameObject.FindGameObjectsWithTag("Teleporter").Length > 2) //Remove all teleporters if there are more than two present.
        {
            RemoveTeleporters();
        }


    }

}
