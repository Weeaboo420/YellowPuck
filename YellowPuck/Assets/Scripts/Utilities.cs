using UnityEditor;
using UnityEngine;

public static class Utilities
{
    public static Sprite LoadSprite(string path)
    {
        return Resources.Load<Sprite>(path);
    }

    public static GameObject LoadPrefab(string path)
    {
        return Resources.Load(path) as GameObject;
    }

    public static bool InRange(float num, float target, float range)
    {
        if(num <= target + range && num >= target - range)
        {
            return true;
        }

        return false;
    }

}