using UnityEditor;

public static class DG_TollKitMenu
{
    [MenuItem("Generator Toolkit/Launch Editor")]
    public static void InitNodeEditor()
    {
        DG_ToolKitEditorWindow.InitEditorWindow();
    }
}
