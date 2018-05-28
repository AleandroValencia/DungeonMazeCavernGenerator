using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class DungeonMapGeneration : MonoBehaviour
{
    #region Variables

    public int Width;
    public int Height;
    public string Seed;
    public bool UsingRandom;
    public List<room> Rooms;
    RoomPivot[] roomPivots;
    room Temp;
    RaycastHit Hit;
    public bool GenerateInRunTime = false;
    bool placed = false;
    public DMainNodes StartNode;
    public DMainNodes EndNode;
    DMapNode DMapStartNode;
    DMapNode DMapEndNode;
    DMapNode[,] Map;
    List<RoomPivot> FinalDungeon;
    int MaxSize;
    Vector2Int[] waypoints;
    public int TargetDifficultyMin = 0;
    public int TargetDifficultyMax = 0;
    bool pathSuccess = false;
    public Vector3 Scale = new Vector3(1,1,1);
    #endregion

    #region Main Methods

    void Awake()
    {
       if (GenerateInRunTime)
       {
            MapGeneration();
       }
    }

    #endregion

    #region Utillity Methods
    public void MapGeneration()
    {
        int countError = 0;

        foreach(room r in Rooms)
        {
            if(r.Difficulty >= TargetDifficultyMin && r.Difficulty <= TargetDifficultyMax)
            {
                countError++;
            }
        }

        //Debug.Log(countError);

        if (countError >= 2)
        {

            while (!pathSuccess)
            {
                //Debug.Log("MapGen MaxSize : " + MaxSize);
                MaxSize = Width * Height;
                Map = new DMapNode[Width, Height];
                roomPivots = new RoomPivot[MaxSize];
                for (int i = 0; i < MaxSize; i++)
                {
                    roomPivots[i] = new RoomPivot(new DMapNode(0, new Vector2(), "N00", new Vector2Int()), null, new Vector3(0,0,0),new Vector2Int(), 0, "null");
                    roomPivots[i].MapNode.Symbol = "X00";
                    roomPivots[i].HPos = i;
                }

                int h = 0;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        roomPivots[h].PosInMap = new Vector2Int((int)x, y);
                        h++;
                    }
                }
                ResetMap();

               // ShowMap();

                FillMapRandomRooms();
                FindPath();
                if(!pathSuccess)
                {
                    Debug.Log("The map generated was not successfully made trying again");
                }
            }
            //ShowMap();
            //ShowMapWithPath();

            for (int i = 0; i < waypoints.Length; i++)
            {
                GetRoomPosition(Map[waypoints[i].x, waypoints[i].y].Symbol.ToString());
            }

            AddObjectsToMap();
        }
        this.transform.localScale = Scale;
    }

    void GetRoomPosition(string Symbol)
    {
        for (int y = 0; y < roomPivots.Length; y++)
        {
            if (roomPivots[y].MapNode.Symbol == Symbol)
            {
                roomPivots[y].MapNode.IsInMap = true;
            }
        }
    }

    void FillMapRandomRooms()
    {
        int h1 = 0;
        for (int y1 = 0; y1 < Height; y1++)
        {
            for (int x1 = 0; x1 < Width; x1++)
            {
                if (x1 == StartNode.Position.x && y1 == StartNode.Position.y)
                {
                    DMapStartNode = new DMapNode(0, StartNode.Size, "S00", StartNode.Position);
                    roomPivots[h1].Name = "Start";
                    roomPivots[h1].MapNode.Room = StartNode.RoomObj;
                    roomPivots[h1].MapNode.Room.name = StartNode.RoomObj.name;
                    roomPivots[h1].MapNode.Size = StartNode.Size;
                    roomPivots[h1].MapNode.Symbol = "S00";
                    roomPivots[h1].MapNode.IsInMap = true;
                    roomPivots[h1].MapNode.GirdPosition = StartNode.Position;
                    AddtoMap(StartNode.Size, StartNode.Position, "S00", 0);
                }
                else if (x1 == EndNode.Position.x && y1 == EndNode.Position.y)
                {
                    DMapEndNode = new DMapNode(0, EndNode.Size, "E00", EndNode.Position);
                    roomPivots[h1].Name = "End";
                    roomPivots[h1].MapNode.Room = EndNode.RoomObj;
                    roomPivots[h1].MapNode.Room.name = EndNode.RoomObj.name;
                    roomPivots[h1].MapNode.Size = EndNode.Size;
                    roomPivots[h1].MapNode.Symbol = "E00";
                    roomPivots[h1].MapNode.IsInMap = true;
                    roomPivots[h1].MapNode.GirdPosition = EndNode.Position;
                    AddtoMap(EndNode.Size, EndNode.Position, "E00", 0);
                }

                roomPivots[h1].PosInWorld = new Vector3(x1 + this.transform.position.x, 0.0f + this.transform.position.y, -y1 + this.transform.position.z);
                h1++;
            }
        }

        if (UsingRandom)
        {
            Seed = Time.time.ToString() + UnityEngine.Random.Range(0, 1000);
        }
 
        System.Random RandSeed = new System.Random(Seed.GetHashCode());

   

        int i = 0;
        float z = 0;
        int h = 0;
        int y = 0;
        int x = 0;
        int number = 0;
        bool first = true;
        bool b_hit = false;
        bool MadeSmaller = false;
        while (y < Height)
        {
            while (x < Width)
            {
                room Room;
                i = RandSeed.Next(0, Rooms.Count);

                Room = Rooms[i];
                //Debug.Log(Room.RoomObj.name);

                //Debug.Log("number : " + number);
                // check y
                if (Room.Size.y > ((Height) - y) )
                {
                    //Debug.Log("Getting smaller room");
                    Room = GetSmallerRoomY(((Height) - y));
                    MadeSmaller = true;
                }

                //Debug.Log("width : " + Width);
                //Debug.Log("z : " + z);
                //Debug.Log("h : " + h);
                // check x
                if (Room.Size.x > ((Width) - z))
                {
                    //Debug.Log("Getting smaller room");
                    if(MadeSmaller)
                    {
                       Room = GetRoom(new Vector2Int((int)((Width) - z), ((Height) - y)));
                    }
                    else
                    {
                        Room = GetSmallerRoomX((int)((Width) - z));
                    }
                }
   

                //Debug.Log("height left: " + ((Height) - y));
                //Debug.Log("width left: " + ((Width) - z));
                //Debug.Log(Room.RoomObj.name);

                //Debug.Log("number : " + number);

                if (!first)
                {
                    if (!CheckHitMap(new Vector2(Room.Size.x, Room.Size.y), new Vector2Int((int)z, y)))
                    {
                        if (number <= 9)
                        {
                            AddtoMap(new Vector2(Room.Size.x, Room.Size.y), new Vector2Int((int)z, y), "O0" + number, Room.Difficulty);
                            roomPivots[h].MapNode.Room  = Room.RoomObj;
                            roomPivots[h].MapNode.Room.name = Room.RoomObj.name;
                            roomPivots[h].MapNode.Size = new Vector2(Room.Size.x, Room.Size.y);
                            roomPivots[h].MapNode.Symbol = "O0" + number;
                            roomPivots[h].MapNode.GirdPosition = roomPivots[h].PosInMap;
                        }
                        else
                        {
                            roomPivots[h].MapNode.Size = new Vector2(Room.Size.x, Room.Size.y);
                            roomPivots[h].MapNode.Room = Room.RoomObj;
                            roomPivots[h].MapNode.Room.name = Room.RoomObj.name;
                            roomPivots[h].MapNode.Symbol = "O" + number;
                            AddtoMap(new Vector2(Room.Size.x, Room.Size.y), new Vector2Int((int)z, y), "O" + number, Room.Difficulty);
                            roomPivots[h].MapNode.GirdPosition = roomPivots[h].PosInMap;
                        }
                        z += Room.Size.x;
                        roomPivots[h].Name = "Room Pivot" + h;
                        h += (int)Room.Size.x;
                        x++;
                        number++;
                    }
                    else
                    {
                        while (!placed)
                        {
                            //Debug.Log("\nChecking for place");
                            //Debug.Log("width : " + Width);
                            //Debug.Log("z : " + z);
                            //Debug.Log("h : " + h);

                            if (z >= Width || Room.Size.x > ((Width) - z))
                            {
                                h += (int)((Width) - z);
                                b_hit = true;
                                break;
                            }
                            else
                            {
                                if (!CheckHitMap(new Vector2(Room.Size.x, Room.Size.y), new Vector2Int((int)z, y)))
                                {
                                    if (number <= 9)
                                    {
                                        AddtoMap(new Vector2(Room.Size.x, Room.Size.y), new Vector2Int((int)z, y), "O0" + number, Room.Difficulty);
                                        roomPivots[h].MapNode.Room = Room.RoomObj;
                                        roomPivots[h].MapNode.Room.name = Room.RoomObj.name;
                                        roomPivots[h].MapNode.Size = new Vector2(Room.Size.x, Room.Size.y);
                                        roomPivots[h].MapNode.Symbol = "O0" + number;
                                        roomPivots[h].MapNode.GirdPosition = roomPivots[h].PosInMap;
                                    }
                                    else
                                    {
                                        roomPivots[h].MapNode.Size = new Vector2(Room.Size.x, Room.Size.y);
                                        roomPivots[h].MapNode.Symbol = "O" + number;
                                        AddtoMap(new Vector2(Room.Size.x, Room.Size.y), new Vector2Int((int)z, y), "O" + number, Room.Difficulty);
                                        roomPivots[h].MapNode.Room = Room.RoomObj;
                                        roomPivots[h].MapNode.Room.name = Room.RoomObj.name;
                                        roomPivots[h].MapNode.GirdPosition = roomPivots[h].PosInMap;
                                    }
                                    z += Room.Size.x;
                                    roomPivots[h].Name = "Room Pivot" + h;
                                    placed = true;
                                    h += (int)Room.Size.x;
                                    x++;
                                    number++;
                                }
                                else
                                {
                                    //AddtoMap(new Vector2(Rooms[i].Size.x, Rooms[i].Size.y), new Vector2Int((int)z, y), "X");
                                    z += Room.Size.x;
                                    //roomPivots[h].name = "null";
                                    h += (int)Room.Size.x;
                                    x++;
                                }
                        }
                    }
                    }
                }
                else
                {
                    if (!CheckHitMap(new Vector2(Room.Size.x, Room.Size.y), new Vector2Int((int)z, y)))
                    {
                        if (number <= 9)
                        {
                            AddtoMap(new Vector2(Room.Size.x, Room.Size.y), new Vector2Int((int)z, y), "O0" + number, Room.Difficulty);
                            roomPivots[h].MapNode.Room = Room.RoomObj;
                            roomPivots[h].MapNode.Room.name = Room.RoomObj.name;
                            roomPivots[h].MapNode.Size = new Vector2(Room.Size.x, Room.Size.y);
                            roomPivots[h].MapNode.Symbol = "O0" + number;
                            roomPivots[h].MapNode.GirdPosition = roomPivots[h].PosInMap;
                        }
                        else
                        {
                            roomPivots[h].MapNode.Size = new Vector2(Room.Size.x, Room.Size.y);
                            roomPivots[h].MapNode.Room = Room.RoomObj;
                            roomPivots[h].MapNode.Room.name = Room.RoomObj.name;
                            roomPivots[h].MapNode.Symbol = "O" + number;
                            AddtoMap(new Vector2(Room.Size.x, Room.Size.y), new Vector2Int((int)z, y), "O" + number, Room.Difficulty);
                            roomPivots[h].MapNode.GirdPosition = roomPivots[h].PosInMap;
                        }

                        roomPivots[h].Name = "Room Pivot" + h;
                        z += Room.Size.x;
                        x++;
                        h += (int)Room.Size.x;
                        number++;
                    }
                    first = false;
                }
                if (b_hit)
                {
                    break;
                }
                placed = false;
                //ShowMap();
                if (z == Width)
                {
                    break;
                }
            }
            MadeSmaller = false;
            b_hit = false;
            z = 0;
            x = 0;
            y++;
            //ShowMap();
        }

    }

   
    void AddObjectsToMap()
    {
        for(int h = 0; h < roomPivots.Length; h++)
        {
            if (roomPivots[h].MapNode.IsInMap == true)
            {
                if (roomPivots[h].MapNode.Size.x != 0)
                {
                    if (roomPivots[h].MapNode.Room != null)
                    {
                        roomPivots[h].RoomPiv = new GameObject();
                        roomPivots[h].RoomPiv.transform.parent = gameObject.transform;
                        roomPivots[h].RoomPiv.name = roomPivots[h].Name;
                        roomPivots[h].RoomPiv.transform.position = roomPivots[h].PosInWorld;
                        GameObject _Room = Instantiate(roomPivots[h].MapNode.Room);
                        _Room.name = roomPivots[h].MapNode.Room.name;
                        _Room.transform.parent = roomPivots[h].RoomPiv.transform;
                        _Room.transform.localPosition = new Vector3((roomPivots[h].MapNode.Size.x / 2), 0.0f, (-roomPivots[h].MapNode.Size.y / 2));
                    }
                }
            }
        }
    }

    room GetRoom(Vector2Int _RoomSize)
    {
        //Debug.Log("room size: " + _RoomSize);
        List<room> TempList = new List<room>();
        System.Random RandSeed = new System.Random(Seed.GetHashCode());

        foreach (room r in Rooms)
        {
            if(r.Size == _RoomSize)
            {
                //Debug.Log(r.RoomObj.name);
                TempList.Add(r);
            }
        }

        //Debug.Log("rooms.count: " + (TempList.Count ));
        int rand = RandSeed.Next(0, TempList.Count);
        //Debug.Log("rand: " + rand);
        Temp = TempList[rand];
        return Temp;
    }

    room GetSmallerRoomY(int _Y)
    {
        List<room> TempList = new List<room>();
        System.Random RandSeed = new System.Random(Seed.GetHashCode());
        foreach (room r in Rooms)
        {
            if (r.Size.y <= _Y)
            {
                TempList.Add(r);
            }
        }
        //Debug.Log("rooms.count: " + (TempList.Count));
        int rand = RandSeed.Next(0, TempList.Count);
        //Debug.Log("rand: " + rand);
        Temp = TempList[rand];

        return Temp;
    }

    room GetSmallerRoomX(int _X)
    {
        List<room> TempList = new List<room>();
        System.Random RandSeed = new System.Random(Seed.GetHashCode());
        foreach (room r in Rooms)
        {
            if (r.Size.x <= _X)
            {
                TempList.Add(r);
            }
        }

        //Debug.Log("rooms.count: " + (TempList.Count));
        int rand = RandSeed.Next(0, TempList.Count);
        //Debug.Log("rand: " + rand);
        Temp = TempList[rand];

        return Temp;
    }

    public void ReStartDungeon()
    {
        foreach (RoomPivot obj in roomPivots)
        {
            DestroyImmediate(obj.RoomPiv);
        }
        ResetMap();
    }

    void ShowMapWithPath()
    {
        //Debug.Log(waypoints.Length);
        string output = "";
        int num = 0;
        for (int x1 = 0; x1 < Width; x1++)
        {
            output += "------------";
        }
        output += "\n";
        Debug.Log(output);
        output = "";
        bool ispath = false;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (x == waypoints[i].x && y == waypoints[i].y)
                    {
                        if (num <= 9)
                        {
                            output += "P0" + num + " ";
                            ispath = true;
                        }
                        else
                        {
                            output += "P" + num + " ";
                            ispath = true;
                        }
                    }
                }
                if (!ispath)
                {
                    if (Map[x, y].Symbol != "X00")
                    {
                        output += Map[x, y].Symbol + " ";
                    }
                    else
                    {
                        if (num <= 9)
                        {
                            output += "X0" + num + " ";
                        }
                        else
                        {

                            output += "X" + num + " ";
                        }
                    }
                }
                num++;
                ispath = false;
            }
            output += "\n";
            Debug.Log(output);
            output = "";
        }

        for (int x2 = 0; x2 < Width; x2++)
        {
            output += "------------";
        }
        output += "\n";
        Debug.Log(output);
        output = "";
    }

    void ShowMap()
    {
        string output = "";
        int num = 0;
        for (int x1 = 0; x1 < Width; x1++)
        {
            output += "------------";
        }
        output += "\n";
        Debug.Log(output);
        output = "";

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                    if (Map[x, y].Symbol != "X00")
                    {
                        output += Map[x, y].Symbol + " ";
                    }
                    else
                    {
                        if (num <= 9)
                        {
                            output += "X0" + num + " ";
                        }
                        else
                        {

                            output += "X" + num + " ";
                        }
                    }
                num++;
            }
            output += "\n";
            Debug.Log(output);
            output = "";
        }

        for (int x2 = 0; x2 < Width; x2++)
        {
            output += "------------";
        }
        output += "\n";
        Debug.Log(output);
        output = "";
    }

    bool CheckHitMap(Vector2 size, Vector2Int pos)
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                if (Map[pos.x + x, pos.y + y].Symbol != "X00")
                {
                    return true;
                }
            }
        }
        return false;
    }

    void ResetMap()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Map[x, y] = new DMapNode(0, new Vector2(), "X00", new Vector2Int());
            }
        }
    }

    void AddtoMap(Vector2 size, Vector2Int pos, string _Symbol, int _Difficulty)
    {
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Map[pos.x + x, pos.y + y].Symbol = _Symbol;
                Map[pos.x + x, pos.y + y].Difficulty = _Difficulty;
                Map[pos.x + x, pos.y + y].GirdPosition = new Vector2Int(pos.x + x, pos.y + y);
            }
        }
    }

    #endregion

    #region PathFinding Methods

    public void FindPath()
    {
        waypoints = new Vector2Int[0];
        pathSuccess = false;

       // Debug.Log("Starting Path Finding");

        DMapNode startNode = DMapStartNode;
        DMapNode targetNode = DMapEndNode;
        startNode.parent = startNode;

        //Debug.Log("FindPath MaxSize : " + MaxSize);
        Heap<DMapNode> openSet = new Heap<DMapNode>(MaxSize);
        HashSet<DMapNode> closedSet = new HashSet<DMapNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            DMapNode currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode.GirdPosition == targetNode.GirdPosition)
            {
                targetNode.parent = currentNode.parent;
                pathSuccess = true;
                break;
            }

            if (TargetDifficultyMin == 0 && TargetDifficultyMax == 0)
            {
                foreach (DMapNode neighbour in GetNeighboursFourAxis(currentNode))
                {
                    //Debug.Log("in neighbour loop");

                    //Debug.Log("current Node : " + currentNode.Symbol);
                    if (closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.Difficulty;
                    if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newMovementCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;
                        //Debug.Log("NodeCurrently looking at: " + neighbour.GirdPosition);
                        //Debug.Log("current node parented: " + neighbour.parent.GirdPosition);
                        //Debug.Log("newMovementCostToNeighbour: " + newMovementCostToNeighbour);
                        //path.Add(currentNode);
                        //Debug.Log("neighbour.parent : " + neighbour.parent.Symbol);

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
            else
            {

                foreach (DMapNode neighbour in GetNeighboursFourAxis(currentNode))
                {
                    //Debug.Log("in neighbour loop");

                    //Debug.Log("current Node : " + currentNode.Symbol);
                    if (closedSet.Contains(neighbour))
                    {
                        continue;
                    }


                    if (neighbour.Difficulty <= TargetDifficultyMax && neighbour.Difficulty >= TargetDifficultyMin)
                    {
                        int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.Difficulty;
                        if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                        {
                            neighbour.GCost = newMovementCostToNeighbour;
                            neighbour.HCost = GetDistance(neighbour, targetNode);
                            neighbour.parent = currentNode;
                            //Debug.Log("NodeCurrently looking at: " + neighbour.GirdPosition);
                            //Debug.Log("current node parented: " + neighbour.parent.GirdPosition);
                            //Debug.Log("newMovementCostToNeighbour: " + newMovementCostToNeighbour);
                            //path.Add(currentNode);
                            //Debug.Log("neighbour.parent : " + neighbour.parent.Symbol);

                            if (!openSet.Contains(neighbour))
                                openSet.Add(neighbour);
                            else
                                openSet.UpdateItem(neighbour);

                        }
                    }
                }

            }

        }

        if (pathSuccess)
        {
            //Debug.Log("start node parent : " + startNode.parent.GirdPosition);
            //Debug.Log("end node parent : " + targetNode.parent.GirdPosition);
            waypoints = RetracePath(startNode, targetNode);
            //waypoints = ListToVector(path);
            //Array.Reverse(waypoints);
            pathSuccess = waypoints.Length > 0;
            //Debug.Log("waypoints : " + waypoints.Length);
        }

         //Debug.Log("Path success : " + pathSuccess);

    }

    Vector2Int[] RetracePath(DMapNode _StartNode, DMapNode _EndNode)
    {
        List<DMapNode> path = new List<DMapNode>();
        DMapNode currentNode = _EndNode;
        while (currentNode != _StartNode)
        {
            //Debug.Log("current: " + currentNode.GirdPosition);
            //Debug.Log("Parent: " + currentNode.parent.GirdPosition);
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector2Int[] waypoints = ListToVector(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector2Int[] ListToVector(List<DMapNode> _Path)
    {
        List<Vector2Int> Waypoints = new List<Vector2Int>();

        for (int i = 1; i < _Path.Count; i++)
        {
            Waypoints.Add(_Path[i].GirdPosition);
        }
        return Waypoints.ToArray();
    }

    int GetDistance(DMapNode _NodeA, DMapNode _NodeB)
    {
        int dstX = (int)Mathf.Abs(_NodeA.GirdPosition.x - _NodeB.GirdPosition.x);
        int dstY = (int)Mathf.Abs(_NodeA.GirdPosition.y - _NodeB.GirdPosition.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    List<DMapNode> GetNeighboursFourAxis(DMapNode _Node)
    {
        List<DMapNode> Neighbours = new List<DMapNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                if (x == -1 && y == -1)
                    continue;
                if (x == -1 && y == 1)
                    continue;
                if (x == 1 && y == 1)
                    continue;
                if (x == 1 && y == -1)
                    continue;

                int CheckX = Mathf.RoundToInt(_Node.GirdPosition.x + x);
                int CheckY = Mathf.RoundToInt(_Node.GirdPosition.y + y);

                if (CheckX >= 0 && CheckX < Width && CheckY >= 0 && CheckY < Height)
                {
                    //Debug.Log("Neighbour Position: " + CheckX +  " , " + CheckY);
                    Neighbours.Add(Map[CheckX, CheckY]);
                }
            }
        }
        return Neighbours;
    }

    #endregion


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //debug//
    #region Debug Methods

    public static void DrawBoxCastOnHit(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float hitInfoDistance, Color color)
    {
        origin = CastCenterOnCollision(origin, direction, hitInfoDistance);
        DrawBox(origin, halfExtents, orientation, color);
    }
    public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color)
    {
        DrawBox(new Box(origin, halfExtents, orientation), color);
    }
    public static void DrawBox(Box box, Color color)
    {
        Debug.DrawLine(box.frontTopLeft, box.frontTopRight, color, 400);
        Debug.DrawLine(box.frontTopRight, box.frontBottomRight, color, 400);
        Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color, 400);
        Debug.DrawLine(box.frontBottomLeft, box.frontTopLeft, color, 400);

        Debug.DrawLine(box.backTopLeft, box.backTopRight, color, 400);
        Debug.DrawLine(box.backTopRight, box.backBottomRight, color, 400);
        Debug.DrawLine(box.backBottomRight, box.backBottomLeft, color, 400);
        Debug.DrawLine(box.backBottomLeft, box.backTopLeft, color, 400);

        Debug.DrawLine(box.frontTopLeft, box.backTopLeft, color, 400);
        Debug.DrawLine(box.frontTopRight, box.backTopRight, color, 400);
        Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color, 400);
        Debug.DrawLine(box.frontBottomLeft, box.backBottomLeft, color, 400);
    }
    public struct Box
    {
        public Vector3 localFrontTopLeft { get; private set; }
        public Vector3 localFrontTopRight { get; private set; }
        public Vector3 localFrontBottomLeft { get; private set; }
        public Vector3 localFrontBottomRight { get; private set; }
        public Vector3 localBackTopLeft { get { return -localFrontBottomRight; } }
        public Vector3 localBackTopRight { get { return -localFrontBottomLeft; } }
        public Vector3 localBackBottomLeft { get { return -localFrontTopRight; } }
        public Vector3 localBackBottomRight { get { return -localFrontTopLeft; } }
        public Vector3 frontTopLeft { get { return localFrontTopLeft + origin; } }
        public Vector3 frontTopRight { get { return localFrontTopRight + origin; } }
        public Vector3 frontBottomLeft { get { return localFrontBottomLeft + origin; } }
        public Vector3 frontBottomRight { get { return localFrontBottomRight + origin; } }
        public Vector3 backTopLeft { get { return localBackTopLeft + origin; } }
        public Vector3 backTopRight { get { return localBackTopRight + origin; } }
        public Vector3 backBottomLeft { get { return localBackBottomLeft + origin; } }
        public Vector3 backBottomRight { get { return localBackBottomRight + origin; } }
        public Vector3 origin { get; private set; }
        public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
        {
            Rotate(orientation);
        }
        public Box(Vector3 origin, Vector3 halfExtents)
        {
            this.localFrontTopLeft = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
            this.localFrontTopRight = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
            this.localFrontBottomLeft = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
            this.localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);

            this.origin = origin;
        }
        public void Rotate(Quaternion orientation)
        {
            localFrontTopLeft = RotatePointAroundPivot(localFrontTopLeft, Vector3.zero, orientation);
            localFrontTopRight = RotatePointAroundPivot(localFrontTopRight, Vector3.zero, orientation);
            localFrontBottomLeft = RotatePointAroundPivot(localFrontBottomLeft, Vector3.zero, orientation);
            localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
        }
    }
    static Vector3 CastCenterOnCollision(Vector3 origin, Vector3 direction, float hitInfoDistance)
    {
        return origin + (direction.normalized * hitInfoDistance);
    }
    static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 direction = point - pivot;
        return pivot + rotation * direction;
    }

    #endregion
}

#region Map Struct 

[System.Serializable]
class DMapNode : IHeapItem<DMapNode>
{
    public int Difficulty;
    public Vector2 Size;
    public string Symbol;
    public Vector2Int GirdPosition;
    public GameObject Room;

    public int GCost;
    public int HCost;
    public DMapNode parent;
    int heapIndex;
    public bool IsInMap = false;
    public int FCost { get { return GCost + HCost; } }
    public int HeapIndex { get { return heapIndex; } set { heapIndex = value; } }

    public DMapNode(int _Difficulty, Vector2 _Size, string _Symbol, Vector2Int _GirdPosition)
    {
        Difficulty = _Difficulty;
        Size = _Size;
        Symbol = _Symbol;
        GirdPosition = _GirdPosition;
    }

    public int CompareTo(DMapNode nodeToCompare)
    {
        int compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0)
        {
            compare = HCost.CompareTo(nodeToCompare.HCost);
        }
        return -compare;
    }
}
struct RoomPivot 
{
    public DMapNode MapNode;
    public GameObject RoomPiv;
    public Vector2Int PosInMap;
    public int HPos;
    public String Name;
    public Vector3 PosInWorld;

    public RoomPivot(DMapNode _MapNode, GameObject _RoomPiv, Vector3 _PosInWorld, Vector2Int _PosInMap, int _HPos, String _Name)
    {
        MapNode = _MapNode;
        RoomPiv = _RoomPiv;
        PosInMap = _PosInMap;
        HPos = _HPos;
        Name = _Name;
        PosInWorld = _PosInWorld;
    }
}

#endregion

[System.Serializable]
public struct DMainNodes
{
    public int Difficulty;
    public Vector2 Size;
    public Vector2Int Position;
    public GameObject RoomObj;

    public DMainNodes(int _Difficulty, Vector2 _Size, Vector2Int _Position, GameObject _RoomObj)
    {
        Difficulty = _Difficulty;
        Size = _Size;
        Position = _Position;
        RoomObj = _RoomObj;
    }
}

//[System.Serializable]
//public class room
//{
//    public int Difficulty;
//    public Vector2 Size;
//    public GameObject RoomObj;
//    [HideInInspector]
//    public bool FoldOut = false;

//    public room(int _Difficulty, Vector2 _Size, GameObject _RoomObj)
//    {
//        Difficulty = _Difficulty;
//        Size = _Size;
//        RoomObj = _RoomObj;
//    }
//}


