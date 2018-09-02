using UnityEngine;

static class GameObjectExtension
{
    public static void SetParent(this GameObject gameObject, GameObject parent)
    {
        gameObject.transform.SetParent(parent.transform);
    }
}