using System;
using UnityEditor;
using UnityEngine;

public static class EditorWindowExtension
{
    public static GUI.Scope BeginScope(this EditorWindow window, Scope scope)
    {
        switch (scope)
        {
            case Scope.Horizontal:
                return new EditorGUILayout.HorizontalScope();

            case Scope.Vertical:
                return new EditorGUILayout.VerticalScope();
        }

        return null;
    }

    public static IDisposable Enabled(this EditorWindow window, bool enabled)
    {
        return new GUIEnabledDisposable(enabled);
    }
}