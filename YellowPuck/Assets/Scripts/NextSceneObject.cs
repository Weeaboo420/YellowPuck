using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneObject : MonoBehaviour
{
    private int _buildIndex;

    public void SetIndex(int buildIndex)
    {
        if(buildIndex > 0 && buildIndex <= SceneManager.sceneCountInBuildSettings-1)
        {
            _buildIndex = buildIndex;
        }
    }

    public int GetNextSceneIndex()
    {
        return _buildIndex;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
