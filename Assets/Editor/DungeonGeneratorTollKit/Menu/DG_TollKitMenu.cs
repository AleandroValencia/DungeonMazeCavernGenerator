using System.Collections;
using UnityEditor;
using UnityEngine;

public static class DG_TollKitMenu
{
    [MenuItem("Dungeon Generator Toolkit/Launch Editor")]
    public static void InitNodeEditor()
    {
        DG_ToolKitEditorWindow.InitEditorWindow();
    }
}
