using System.Collections.Specialized;
using UnityEngine;

public static class Globals
{    
    //Node Colors
    public static readonly Color32 WallColor = new Color32(73, 195, 243, 255);
    public static readonly Color32 FloorColorMain = new Color32(135, 135, 135, 255);
    public static readonly Color32 FloorColorSecondary = new Color32(87, 87, 87, 255);
    public static readonly Color32 PurpleWallColor = new Color32(181, 120, 200, 255);
    public static readonly Color32 GateColor = new Color32(103, 251, 250, 255);

    //Ghost Colors
    public static readonly OrderedDictionary GhostColors = new OrderedDictionary()
    {
        { "Red", new Color32(159, 38, 25, 255) },
        { "Blue", new Color32(9, 111, 235, 255) },
        { "Yellow", new Color32(235, 227, 9, 255) },
        { "Purple", new Color32(122, 22, 235, 255) },
        { "Pink", new Color32(199, 38, 151, 255) },
        { "Green", new Color32(134, 199, 38, 255) }
    };

    //Misc
    public const bool ShowAITargeting = true; //Should the targeting crosshairs used by the AI be visible?
    public static readonly int[] TargetOffsets = {0};
}