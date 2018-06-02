using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class DG_ToolKitEditorWindow : EditorWindow
{
    #region Variables

    public static DG_ToolKitEditorWindow CurrWindow;
    int tab;
    bool GenerateInRunTime = false;
    Vector2 ScrollView;

    #endregion

    #region Cavern Variables

    public int C_width, C_height;
    public string C_seed = "0";
    public bool C_isRandomSeed;
    public int C_randomFillPercent;
    public Vector3 C_Pos;
    Material C_RockMat;
    Material C_WallMat;
    MeshFilter C_Mesh;
    public string C_Name;
    int C_NumberofPresses = 0;
    bool C_GeneratedOnce = false;
    GameObject C_CurrCavern;

    #endregion

    #region Maze Variables

    public int M_width = 5, M_height = 5;
    public string M_name;
    public GameObject M_wall, M_floorTile, M_marker, M_markerFront;
    public bool M_tile;

    int M_numberOfPresses = 0;
    bool M_generatedOnce = false;
    GameObject M_CurrMaze;

    #endregion

    #region Dungeon Variables

    public int D_width, D_height;
    public string D_seed = "0";
    public bool D_isRandomSeed;
    public Vector3 D_Pos;
    public string D_Name;
    public int D_NumberofPresses = 0;
    public bool D_GeneratedOnce = false;
    public LayerMask D_mask;
    public int D_ignoremask;
    public List<room> D_Rooms;
    GameObject D_CurrDungeon;
    bool D_ListFoldOut = false;
    int newCount = 0;
    bool D_StartNodeFoldOut = false;
    DMainNodes D_StartNode;
    bool D_EndNodeFoldOut = false;
    DMainNodes D_EndNode;
    int D_TargetDifficultyMin = 0;
    int D_TargetDifficultyMax = 0;
    bool D_UseTargetDifs = false;
    bool D_DifficultiesError = false;

    #endregion

    #region Main Methods

    public static void InitEditorWindow()
    {
        CurrWindow = (DG_ToolKitEditorWindow)EditorWindow.GetWindow<DG_ToolKitEditorWindow>();
        CurrWindow.titleContent = new GUIContent("DG ToolKit");
        CreateViews();
    }

    private void OnEnable()
    {
        D_Rooms = new List<room>();
        Debug.Log("Enable Window");
    }

    private void OnDestroy()
    {
        Debug.Log("Disable Window");
    }

    private void Update()
    {
    }

    private void OnGUI()
    {
        tab = GUILayout.Toolbar(tab, new string[] { "Cavern", "Maze", "Dungeon" });
        switch (tab)
        {
            case 0:
                CavernTab();
                break;
            case 1:
                MazeTab();
                break;
            case 2:
                DungeonTab();
                break;
            default:
                break;
        }
        // Get and Process Current Event
        Event e = Event.current;
        ProcessEvents(e);

        Repaint();
    }

    #endregion

    #region Utillity Methods

    static void CreateViews()
    {
        CurrWindow = (DG_ToolKitEditorWindow)EditorWindow.GetWindow<DG_ToolKitEditorWindow>();
    }

    void ProcessEvents(Event e)
    {

    }

    #region Cavern Methods
    void CavernTab()
    {
        ScrollView = GUILayout.BeginScrollView(ScrollView, false, true);
        C_Name = "CG_Cavern " + C_NumberofPresses;
        C_Name = EditorGUILayout.TextField("Object Name", C_Name);
        if (GUILayout.Toggle(GenerateInRunTime, "Generate In Run Time"))
        {
            GenerateInRunTime = true;
        }
        else
        {
            GenerateInRunTime = false;
        }
        C_Pos = EditorGUILayout.Vector3Field("Object Position", C_Pos);

        C_width = EditorGUILayout.IntField("Width", C_width);
        C_height = EditorGUILayout.IntField("Height", C_height);
        C_randomFillPercent = EditorGUILayout.IntSlider("Fill Percentage", C_randomFillPercent, 0, 100);
        C_seed = EditorGUILayout.TextField("Seed", C_seed);
        if (GUILayout.Toggle(C_isRandomSeed, "Use Random Seed"))
        {
            C_isRandomSeed = true;
        }
        else
        {
            C_isRandomSeed = false;
        }

        C_RockMat = EditorGUILayout.ObjectField("Rock Material", C_RockMat, typeof(Material)) as Material;
        C_WallMat = EditorGUILayout.ObjectField("Wall Material", C_WallMat, typeof(Material)) as Material;

        if (GUILayout.Button("Generate New"))
        {
            GenereateNewCavern();
        }
        if (GUILayout.Button("Regenerate"))
        {

            if (C_GeneratedOnce && C_CurrCavern != null)
            {
                if (Selection.gameObjects.Length > 0)
                {
                    foreach (GameObject obj in Selection.gameObjects)
                    {
                        if (obj.GetComponent<CaveGeneratorScript>() != null)
                        {
                            C_CurrCavern = obj.gameObject;
                            RegenerateCavern();
                        }
                    }
                }
                else
                {
                    RegenerateCavern();
                }

            }
        }
        GUILayout.EndScrollView();
    }

    void GenereateNewCavern()
    {
        C_GeneratedOnce = true;
        C_NumberofPresses++;
        //Make Game Object
        GameObject g = new GameObject(C_Name.ToString());
        g.transform.position = C_Pos;
        g.AddComponent<MeshFilter>();
        g.AddComponent<MeshRenderer>();
        g.AddComponent<CaveGeneratorScript>();
        g.AddComponent<CaveMeshGeneratorScript>();

        CaveGeneratorScript MapGen = g.GetComponent<CaveGeneratorScript>();
        MapGen.height = C_height;
        MapGen.width = C_width;
        MapGen.seed = C_seed;
        MapGen.generateRandomSeed = C_isRandomSeed;
        MapGen.fillPercent = C_randomFillPercent;
        MapGen.GenerateAtRunTime = GenerateInRunTime;

        g.GetComponent<MeshRenderer>().material = C_RockMat;

        GameObject w = new GameObject("Walls");
        w.transform.parent = g.transform;
        w.transform.position = C_Pos;
        w.AddComponent<MeshFilter>();
        w.AddComponent<MeshRenderer>();
        w.GetComponent<MeshRenderer>().material = C_WallMat;

        g.GetComponent<CaveMeshGeneratorScript>().walls = w.GetComponent<MeshFilter>(); //C_Mesh;
        if (!GenerateInRunTime)
        {
            MapGen.GenerateMap();
        }
        C_CurrCavern = g;
    }

    void RegenerateCavern()
    {
        C_GeneratedOnce = true;

        CaveGeneratorScript MapGen = C_CurrCavern.GetComponent<CaveGeneratorScript>();
        MapGen.height = C_height;
        MapGen.width = C_width;
        MapGen.seed = C_seed;
        MapGen.generateRandomSeed = C_isRandomSeed;
        MapGen.fillPercent = C_randomFillPercent;
        MapGen.GenerateAtRunTime = GenerateInRunTime;

        C_CurrCavern.GetComponent<MeshRenderer>().material = C_RockMat;

        GameObject w = C_CurrCavern.transform.Find("Walls").gameObject;
        w.GetComponent<MeshRenderer>().material = C_WallMat;

        C_CurrCavern.GetComponent<CaveMeshGeneratorScript>().walls = w.GetComponent<MeshFilter>(); //C_Mesh;
        if (!GenerateInRunTime)
        {
            MapGen.GenerateMap();
        }
    }

    #endregion

    #region Maze Methods
    void MazeTab()
    {
        ScrollView = GUILayout.BeginScrollView(ScrollView, false, true);
        M_name = "CG_Maze " + M_numberOfPresses;
        M_name = EditorGUILayout.TextField("Object Name", M_name);
        if (GUILayout.Toggle(GenerateInRunTime, "Generate In Run Time"))
        {
            GenerateInRunTime = true;
        }
        else
        {
            GenerateInRunTime = false;
        }
        M_width = EditorGUILayout.IntField(new GUIContent("Width", "Number of cells horizontally"), M_width);
        M_height = EditorGUILayout.IntField(new GUIContent("Height", "Number of cells vertically"), M_height);
        M_wall = EditorGUILayout.ObjectField(new GUIContent("Wall", "Gameobject to be used as wall"), M_wall, typeof(GameObject), true) as GameObject;
        M_floorTile = EditorGUILayout.ObjectField(new GUIContent("Floor", "Gameobject to be used as floor"), M_floorTile, typeof(GameObject), true) as GameObject;
        M_marker = EditorGUILayout.ObjectField(new GUIContent("Marker", "Marker gameobject to place at dead ends. Should have a texture or color to distinguish."), M_marker, typeof(GameObject), true) as GameObject;
        M_markerFront = EditorGUILayout.ObjectField(new GUIContent("MarkerTwo", "OPTIONAL Marker gameobject to place at dead ends."), M_markerFront, typeof(GameObject), true) as GameObject;
        if (GUILayout.Toggle(M_tile, new GUIContent("Tiles", "Should the floor object be used per cell or one floor for whole maze?")))
        {
            M_tile = true;
        }
        else
        {
            M_tile = false;
        }

        if (GUILayout.Button("Generate New"))
        {
            GenerateNewMaze();
        }
        if (GUILayout.Button("Regenerate"))
        {
            RegenerateMaze();
        }
        GUILayout.EndScrollView();
    }

    void GenerateNewMaze()
    {
        M_numberOfPresses++;
        MazeGenerator.m_name = M_name;
        CreateMaze();
    }

    void CreateMaze()
    {
        MazeGenerator.GenerateInRunTime = GenerateInRunTime;
        MazeGenerator.m_numRows = M_height;
        MazeGenerator.m_numColumns = M_width;
        MazeGenerator.m_tiles = M_tile;
        if (M_height <= 0)
        {
            EditorUtility.DisplayDialog("ERROR", "Can't have negative rows.", "OK");
        }
        else if(M_width <=0)
        {
            EditorUtility.DisplayDialog("ERROR", "Can't have negative columns.", "OK");
        }
        else if (M_wall == null)
        {
            EditorUtility.DisplayDialog("ERROR", "Wall gameobject is not set", "I'll fix that");
        }
        else if (M_floorTile == null)
        {
            EditorUtility.DisplayDialog("ERROR", "FloorTile gameobject is not set", "I'll fix that");
        }
        else if (M_marker == null)
        {
            EditorUtility.DisplayDialog("ERROR", "Marker gameobject is not set", "I'll fix that");
        }
        else
        {
            MazeGenerator.m_wall = M_wall;
            MazeGenerator.m_floor = M_floorTile;
            MazeGenerator.m_marker = M_marker;
            MazeGenerator.m_markerFront = M_markerFront;
            if (!GenerateInRunTime)
            {
                M_CurrMaze = MazeGenerator.GenerateMaze();
            }
        }
    }

    void RegenerateMaze()
    {
        DestroyImmediate(M_CurrMaze);
        M_CurrMaze = null;
        CreateMaze();
    }

    #endregion

    #region Dungeon Methods

    void DungeonTab()
    {
        ScrollView = GUILayout.BeginScrollView(ScrollView, false, true);
        D_Name = "CG_Dungeon " + D_NumberofPresses;
        D_Name = EditorGUILayout.TextField("Object Name", D_Name);
        if (GUILayout.Toggle(GenerateInRunTime, "Generate In Run Time"))
        {
            GenerateInRunTime = true;
        }
        else
        {
            GenerateInRunTime = false;
        }
        D_Pos = EditorGUILayout.Vector3Field("Object Top Left Position", D_Pos);

        D_width = EditorGUILayout.IntField("Width", D_width);
        D_height = EditorGUILayout.IntField("Height", D_height);

        D_UseTargetDifs = GUILayout.Toggle(D_UseTargetDifs, new GUIContent("Use Target Difficulties", "Use this if you want the generator to only find rooms between a certain range of difficulties"));
        if (D_UseTargetDifs)
        {
            D_TargetDifficultyMin = EditorGUILayout.IntField("Minimum", D_TargetDifficultyMin);
            D_TargetDifficultyMax = EditorGUILayout.IntField("Maximum", D_TargetDifficultyMax);
        }
        else
        {
            D_TargetDifficultyMin = 0;
            D_TargetDifficultyMax = 0;
        }

        D_seed = EditorGUILayout.TextField("Seed", D_seed);
        if (GUILayout.Toggle(D_isRandomSeed, "Use Random Seed"))
        {
            D_isRandomSeed = true;
        }
        else
        {
            D_isRandomSeed = false;
        }

        D_StartNodeFoldOut = EditorGUILayout.Foldout(D_StartNodeFoldOut, "Starting Room");
        if (D_StartNodeFoldOut)
        {
            D_StartNode.Position = EditorGUILayout.Vector2IntField("Position In Map", D_StartNode.Position);
            D_StartNode.Size = EditorGUILayout.Vector2Field("Room Size", D_StartNode.Size);
            D_StartNode.RoomObj = EditorGUILayout.ObjectField("Room Object", D_StartNode.RoomObj, typeof(GameObject)) as GameObject;
        }

        D_EndNodeFoldOut = EditorGUILayout.Foldout(D_EndNodeFoldOut, "Ending Room");
        if (D_EndNodeFoldOut)
        {
            D_EndNode.Position = EditorGUILayout.Vector2IntField("Position In Map", D_EndNode.Position);
            D_EndNode.Size = EditorGUILayout.Vector2Field("Room Size", D_EndNode.Size);
            D_EndNode.RoomObj = EditorGUILayout.ObjectField("Room Object", D_EndNode.RoomObj, typeof(GameObject)) as GameObject;
        }

        D_ListFoldOut = EditorGUILayout.Foldout(D_ListFoldOut, "Rooms");
        if (D_ListFoldOut)
        {
            ShowList();
        }

        //D_mask = EditorGUILayout.ObjectField("Mask", D_mask, typeof(LayerMask)) as LayerMask;
        //D_ignoremask = EditorGUILayout.IntField("ignore mask", D_ignoremask);

        if (GUILayout.Button("Generate New"))
        {

            if (D_EndNode.Position.x + D_EndNode.Size.x > D_width ||
                              D_EndNode.Position.y + D_EndNode.Size.y > D_height ||
                              D_StartNode.Position.x + D_StartNode.Size.x > D_width ||
                              D_StartNode.Position.y + D_StartNode.Size.y > D_height ||
                              D_EndNode.Position.x < 0 ||
                               D_EndNode.Position.y < 0 ||
                               D_StartNode.Position.x < 0 ||
                               D_StartNode.Position.y < 0)
            {
                EditorUtility.DisplayDialog("ERROR",
                                                 "You have placed either the starting room or the ending room outside of the confines of the dungeon bondaries \n Please check your postion and your room size", "OK");
            }
            else if (newCount <= 0)
            {
                EditorUtility.DisplayDialog("ERROR",
                                                 "Your list of rooms is empty", "OK");
            }
            else if (D_EndNode.Position == D_StartNode.Position || D_StartNode.Position == D_EndNode.Position)
            {
                EditorUtility.DisplayDialog("ERROR",
                                                 "Your ending and starting rooms are in the same position", "OK");
            }
            else if (D_EndNode.Size.x == 0 ||
                D_EndNode.Size.y == 0 ||
                D_StartNode.Size.x == 0 ||
                D_StartNode.Size.y == 0)
            {
                EditorUtility.DisplayDialog("ERROR",
                                                 "You have not set the size of your starting or ending rooms", "OK");
            }
            //else if (D_DifficultiesError)
            //{
            //    EditorUtility.DisplayDialog("ERROR",
            //                                     " There are less than two rooms that are with in your target difficulty Range\n Please check this before continuing", "OK");
            //}
            else
            {
                GenereateNewDungeon();
            }
        }
        if (GUILayout.Button("Regenerate"))
        {

            if (D_GeneratedOnce && D_CurrDungeon != null)
            {
                if (Selection.gameObjects.Length > 0)
                {
                    foreach (GameObject obj in Selection.gameObjects)
                    {
                        if (obj.GetComponent<DungeonMapGeneration>() != null)
                        {
                            D_CurrDungeon = obj.gameObject;

                            if (D_EndNode.Position.x + D_EndNode.Size.x > D_width ||
                                D_EndNode.Position.y + D_EndNode.Size.y > D_height ||
                                D_StartNode.Position.x + D_StartNode.Size.x > D_width ||
                                D_StartNode.Position.y + D_StartNode.Size.y > D_height ||
                                D_EndNode.Position.x < 0 ||
                                 D_EndNode.Position.y < 0 ||
                                 D_StartNode.Position.x < 0 ||
                                 D_StartNode.Position.y < 0)
                            {
                                EditorUtility.DisplayDialog("ERROR",
                                                 "You have placed either the starting room or the ending room outside of the confines of the dungeon bondaries \n Please check your postion and your room size", "OK");
                            }
                            else if (newCount <= 0)
                            {
                                EditorUtility.DisplayDialog("ERROR",
                                                                 "Your list of rooms is empty", "OK");
                            }
                            else if (D_EndNode.Position == D_StartNode.Position || D_StartNode.Position == D_EndNode.Position)
                            {
                                EditorUtility.DisplayDialog("ERROR",
                                                                 "Your ending and starting rooms are in the same position", "OK");
                            }
                            else if (D_EndNode.Size.x == 0 ||
                                D_EndNode.Size.y == 0 ||
                                D_StartNode.Size.x == 0 ||
                                D_StartNode.Size.y == 0)
                            {
                                EditorUtility.DisplayDialog("ERROR",
                                                                 "You have not set the size of your starting or ending rooms", "OK");
                            }
                            else if(D_DifficultiesError)
                            {
                                EditorUtility.DisplayDialog("ERROR",
                                                                 " There are less than two rooms that are with in your target difficulty Range\n Please check this before continuing", "OK");
                            }
                            else
                            {
                                RegenereateDungeon();
                            }

                        }
                    }
                }
                else
                {
                    RegenereateDungeon();
                }

            }
        }
        GUILayout.EndScrollView();
    }

    void ShowList()
    {
        newCount = Mathf.Max(0, EditorGUILayout.IntField("Size Of List", D_Rooms.Count));
        while (newCount < D_Rooms.Count)
            D_Rooms.RemoveAt(D_Rooms.Count - 1);
        while (newCount > D_Rooms.Count)
            D_Rooms.Add(new room(0, new Vector2(0, 0), null));

        int ErrorCount = 0;

        for (int i = 0; i < D_Rooms.Count; i++)
        {
            D_Rooms[i].FoldOut = EditorGUILayout.Foldout(D_Rooms[i].FoldOut, "Room " + i);
            if (D_Rooms[i].FoldOut)
            {
                D_Rooms[i].Difficulty = EditorGUILayout.IntField("Room Difficulty", D_Rooms[i].Difficulty);
                D_Rooms[i].Size = EditorGUILayout.Vector2Field("Room Size", D_Rooms[i].Size);
                D_Rooms[i].RoomObj = EditorGUILayout.ObjectField("Room Object", D_Rooms[i].RoomObj, typeof(GameObject)) as GameObject;

                if(D_Rooms[i].Difficulty >= D_TargetDifficultyMin && D_Rooms[i].Difficulty <= D_TargetDifficultyMax)
                {
                    ErrorCount++;
                }

            }
        }

        if(ErrorCount < 2)
        {
            D_DifficultiesError = true;
        }
        else
        {
            D_DifficultiesError = false;
        }
    }

    void GenereateNewDungeon()
    {
        D_GeneratedOnce = true;
        D_NumberofPresses++;

        //Make Game Object
        GameObject g = new GameObject(D_Name.ToString());
        g.transform.position = D_Pos;
        g.AddComponent<DungeonMapGeneration>();

        DungeonMapGeneration DGen = g.GetComponent<DungeonMapGeneration>();
        DGen.Height = D_height;
        DGen.Width = D_width;
        DGen.Seed = D_seed;
        DGen.UsingRandom = D_isRandomSeed;
        DGen.Rooms = D_Rooms;
        DGen.TargetDifficultyMax = D_TargetDifficultyMax;
        DGen.TargetDifficultyMin = D_TargetDifficultyMin;
        //DGen.mask = D_mask;
        //DGen.ignoremask = D_ignoremask;
        DGen.GenerateInRunTime = GenerateInRunTime;
        DGen.StartNode = D_StartNode;
        DGen.EndNode = D_EndNode;

        if (!GenerateInRunTime)
        {
            DGen.MapGeneration();
        }
        D_CurrDungeon = g;
    }

    void RegenereateDungeon()
    {
        DungeonMapGeneration DGen = D_CurrDungeon.GetComponent<DungeonMapGeneration>();
        DGen.ReStartDungeon();
        DGen.Height = D_height;
        DGen.Width = D_width;
        DGen.Seed = D_seed;
        DGen.UsingRandom = D_isRandomSeed;
        DGen.Rooms = D_Rooms;
        DGen.TargetDifficultyMax = D_TargetDifficultyMax;
        DGen.TargetDifficultyMin = D_TargetDifficultyMin;
        //DGen.mask = D_mask;
        // DGen.ignoremask = D_ignoremask;
        DGen.GenerateInRunTime = GenerateInRunTime;

        if (!GenerateInRunTime)
        {
            DGen.MapGeneration();
        }
        //GameObject g = new GameObject(C_Name.ToString());
    }

    #endregion

    #endregion
}