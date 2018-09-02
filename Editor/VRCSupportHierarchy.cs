using UnityEditor;

public class VRCSupportHierarchy
{
    [MenuItem("GameObject/오챠★/EmoteOnOff", true, 0)]
    public static bool ValidateEmoteOnOff()
    {
        return Selection.gameObjects.Length == 1;
    }

    [MenuItem("GameObject/오챠★/EmoteOnOff", false, 0)]
    public static void InvokeEmoteOnOff()
    {
        var gameObject = Selection.gameObjects[0];

        if (VRCSupportWindow.Window == null)
            VRCSupportWindow.Init();

        VRCSupportWindow.Window.EmoteOnOff.TargetObject = gameObject;
        VRCSupportWindow.Window.EmoteOnOff.OffName = gameObject.name + " OFF";
        VRCSupportWindow.Window.EmoteOnOff.OnName = gameObject.name + " ON";
        VRCSupportWindow.Window.Focus();
    }
}