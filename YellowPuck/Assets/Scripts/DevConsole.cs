using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DevConsoleCommands //A static class that will hold all functions performed when typing commands into the dev console.
{
    public static void ReloadLevel()
    {
        //Old debug code, keeping it for now
        /*GameObject nextSceneObject = new GameObject("NextSceneObject");
        NextSceneObject nextSceneObjectScript = nextSceneObject.AddComponent<NextSceneObject>();
        nextSceneObjectScript.SetIndex(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene("Loading");*/
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void ToggleGhostCursors()
    {
        foreach(GhostObject ghostObject in GameObject.FindObjectsOfType<GhostObject>())
        {
            ghostObject.ToggleCursors();
        }
    }

    public static void TurboMode()
    {
        Time.timeScale = 1.5f;
    }

    public static void DisableTurboMode()
    {
        Time.timeScale = 1f;
    }
}

public sealed class DevConsole : MonoBehaviour
{
    private string _inputBuffer = "";
    private const int _maxBufferSize = 20; //Defines how many characters can be buffered before resetting the input buffer.
    private const float _caretBlinkRate = 1f / 2f;
    private bool _consoleEnabled = false;

    private GameObject _inputField;
    private Text _inputText;
    private bool _canUseBackspace = true;
    private bool _caretVisible = true;
    private bool _caretEnumeratorRunning = false;
    private string _caretColor = "#ffffffff";


    private static readonly Dictionary<string, Action> _commandsDict = new Dictionary<string, Action>()
    {
        {"reload", DevConsoleCommands.ReloadLevel },
        {"turbo", DevConsoleCommands.TurboMode },
        {"no_turbo", DevConsoleCommands.DisableTurboMode },
        {"toggle_crosshairs", DevConsoleCommands.ToggleGhostCursors }
    };

    private void Start()
    {
        _inputField = GameObject.Find("consoleInput");
        _inputText = _inputField.transform.Find("consoleText").GetComponent<Text>();
        _inputField.SetActive(_consoleEnabled);

        if(Time.timeScale != 1f) //Resets turbo mode
        {
            DevConsoleCommands.DisableTurboMode();
        }

    }

    private void UpdateUI()
    {
        if (!_caretEnumeratorRunning)
        {
            StartCoroutine(caretBlink());
        }
        _inputText.text = "> " + _inputBuffer + "<color=" + _caretColor + ">_</color>";        
    }

    private IEnumerator delayBackspace()
    {
        _canUseBackspace = false;
        yield return new WaitForSeconds(0.078f);
        _canUseBackspace = true;
    }

    private IEnumerator caretBlink()
    {
        _caretEnumeratorRunning = true;        
        yield return new WaitForSeconds(_caretBlinkRate);
        _caretVisible = !_caretVisible;

        switch(_caretVisible)
        {
            case true:
                _caretColor = "#ffffffff";
                break;
            case false:
                _caretColor = "#ffffff00";
                break;
        }
        _caretEnumeratorRunning = false;
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _consoleEnabled = !_consoleEnabled;
            _inputField.SetActive(_consoleEnabled);
            UpdateUI();
        }

        if (_consoleEnabled)
        {
            bool addToBuffer = true;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                foreach (string key in _commandsDict.Keys)
                {
                    if (_inputBuffer == key)
                    {
                        _commandsDict[_inputBuffer].Invoke();
                    }
                }

                _inputBuffer = "";
                UpdateUI();
            }

            else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKey(KeyCode.Backspace))
            {
                if (_inputBuffer.Length >= 1 && _canUseBackspace)
                {
                    StartCoroutine(delayBackspace());
                    _inputBuffer = _inputBuffer.Remove(_inputBuffer.Length - 1);
                    addToBuffer = false;
                }
                UpdateUI();
            }

            else if(_inputBuffer.Length < _maxBufferSize && addToBuffer && Input.inputString.Length > 0)
            {
                _inputBuffer += Input.inputString;
                StopCoroutine(delayBackspace());
                UpdateUI();                
            }            
        }
    }

}
