using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    private NextSceneObject _nextSceneObject;
    private int _nextIndex;

    IEnumerator delay()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(_nextIndex);
    }

    private void Start()
    {
        _nextSceneObject = GameObject.Find("NextSceneObject").GetComponent<NextSceneObject>();
        _nextIndex = _nextSceneObject.GetNextSceneIndex();
        Destroy(_nextSceneObject.gameObject);
        StartCoroutine(delay());
    }

}
