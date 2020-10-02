using UnityEngine;

public class Graphics : MonoBehaviour
{
    private void Awake()
    {        
        //QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 420; //Set max fps regardless of vsync
    }
}