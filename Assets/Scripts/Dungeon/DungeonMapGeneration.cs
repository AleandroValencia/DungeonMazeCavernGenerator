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
        // if user wants the dungeon to be generated in run time 
        // then generate otherwise Map Generation will be called in editor script
       if (GenerateInRunTime)
       {
            MapGeneration();
       }
    }

    #endregion

    #region Utillity Methods
    // main function used to generate map
    public void MapGeneration()
    {
        // user error checking, checks if user has not put in difficulties weirdly
        int countError = 0;
        foreach(room r in Rooms)
        {
            if(r.Difficulty >= TargetDifficultyMin && r.Difficulty <= TargetDifficultyMax)
            {
                countError++;
            }
        }

        // if difficualties are ok continue
        if (countError >= 2)
        {
            //if pathfinder cannot find a path from start room to end room with rooms 
            // within the difficulty range user has chosen try again
            while (!pathSuccess)
            {
                // set up for map, adding room pivots to map
                // roompivots are there so that all rooms positions are counted from top left corner
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
                // resets map in case of regeneration
                ResetMap();

               // adds rooms so that they all fit with no spaces between them inside the spacified size of the map square
                FillMapRandomRooms();
                // trys to find a path with a star path finding of rooms that have a difficulty that ranges with in the user chosen difficulty range 
                FindPath();
                if(!pathSuccess)
                {
                    Debug.Log("The map generated was not successfully made trying again");
                }
            }

           
            // add rooms to actual game/engine world parenting them to  the dungeon object
            // only adds rooms identified by path as being in path
            for (int i = 0; i < waypoints.Length; i++)
            {
                GetRoomPosition(Map[waypoints[i].x, waypoints[i].y].Symbol.ToString());
            }
            AddObjectsToMap();
        }
        this.transform.localScale = Scale;
    }

   // gets all rooms with a specific symbol in map
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

    // adds rooms so that they all fit with no spaces between them inside the spacified size of the map square
    void FillMapRandomRooms()
    {
        // initialize  nodes in map including start room nodes and end rooms nodes in map
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

        // if user has chosen a random seed initialize seed here
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
        // begins loop to add all rooms into map square with no spaces
        // rooms are compacted in, can handle all room sizes
        // the generator goes node by node from the top left of the map 
        // to the bottom of the map from collum by collum then row by row
        // do this until the whole map has been filled
        // **** THERE HAVE NOT BEEN ANY GAME OBJECTS MADE YET THIS IS ALL ON AN 2D ARRAY IN SYSTEM VERY LIGHT WEIGHT ***
        while (y < Height)
        {
            while (x < Width)
            {
                // choose a room randomly from list of rooms provided
                room Room;
                i = RandSeed.Next(0, Rooms.Count);
                Room = Rooms[i];

                // begin checks to make sure room can be fit into map
                // check y
                if (Room.Size.y > ((Height) - y) )
                {
                    // if it cannot be fit in because it is too tall grab a shorter room from list of rooms
                    Room = GetSmallerRoomY(((Height) - y));
                    MadeSmaller = true;
                }
                // check x
                if (Room.Size.x > ((Width) - z))
                {
                    if(MadeSmaller)
                    {
                        // if it already has been made shorter because it was too tall
                        // and now it is too wide get the smallest rooms in all ways possible
                        // with in list of rooms 
                       Room = GetRoom(new Vector2Int((int)((Width) - z), ((Height) - y)));
                    }
                    else
                    {
                        // if it cannot be fit in because it is too wide grab a thinner room from list of rooms
                        Room = GetSmallerRoomX((int)((Width) - z));
                    }
                }

                // if room being put in is not first room (apart from start and end) being put into map
                if (!first)
                {
                    // check if room is colliding with another room in the position generator is tryign to put it in
                    // if it is not colliding with another room then place it down
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
                        // if it does collide with another room try to move a node to its right and place it again
                        // continue trying to do this until the room is too wide to be place on that row
                        // if it is too wide try another room
                        while (!placed)
                        {
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
                                    z += Room.Size.x;
                                    h += (int)Room.Size.x;
                                    x++;
                                }
                        }
                    }
                    }
                }
                else
                {
                    // if the room is the first room to be placed check if it is colliding with eaither the 
                    // starting room or ending room
                    // if it is move across row until it can be placed
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
        }

    }

    // add rooms to actual game/engine world parenting them to  the dungeon object
    // only adds rooms identified by path as being in path
    void AddObjectsToMap()
    {
        // using the waypoint positions given by the path finding algorithm find each room and add it to map
        // check where each room is and do not add room twice
        // this is checked through the symbol as each room has a symbol
        // so if a room takes up multiple nodes in map (and if it is not a 1 by 1 it will) than it will 
        // not be added multiple times on top of each other in the actuall game world 
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

    // get room from list of rooms of size specified
    // if there are multiple chooses at random
    room GetRoom(Vector2Int _RoomSize)
    {
        List<room> TempList = new List<room>();
        System.Random RandSeed = new System.Random(Seed.GetHashCode());

        foreach (room r in Rooms)
        {
            if(r.Size == _RoomSize)
            {
                TempList.Add(r);
            }
        }
        int rand = RandSeed.Next(0, TempList.Count);
        Temp = TempList[rand];
        return Temp;
    }

    // get room from list of rooms of height smaller than the height passed
    // if there are multiple chooses at random
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
        int rand = RandSeed.Next(0, TempList.Count);
        Temp = TempList[rand];
        return Temp;
    }

    // get room from list of rooms of width smaller than the width passed
    // if there are multiple chooses at random
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
        int rand = RandSeed.Next(0, TempList.Count);
        Temp = TempList[rand];
        return Temp;
    }

    // clears dungeon
    public void ReStartDungeon()
    {
        foreach (RoomPivot obj in roomPivots)
        {
            DestroyImmediate(obj.RoomPiv);
        }
        ResetMap();
    }

    // debug function to show path in map on console
    void ShowMapWithPath()
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

    // debug function to show map on console
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

    // hit check, checks if rooms are colliding when being placed
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

    // resets map 
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

    // adds room to map
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

    // path finding function using a star path finding
    public void FindPath()
    {
        waypoints = new Vector2Int[0];
        pathSuccess = false;

        DMapNode startNode = DMapStartNode;
        DMapNode targetNode = DMapEndNode;
        startNode.parent = startNode;

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
            waypoints = RetracePath(startNode, targetNode);
            pathSuccess = waypoints.Length > 0;
        }
    }

    // utility function for a star pathfinding
    Vector2Int[] RetracePath(DMapNode _StartNode, DMapNode _EndNode)
    {
        List<DMapNode> path = new List<DMapNode>();
        DMapNode currentNode = _EndNode;
        while (currentNode != _StartNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector2Int[] waypoints = ListToVector(path);
        Array.Reverse(waypoints);
        return waypoints;
    }


    // utility function for a star pathfinding
    Vector2Int[] ListToVector(List<DMapNode> _Path)
    {
        List<Vector2Int> Waypoints = new List<Vector2Int>();

        for (int i = 1; i < _Path.Count; i++)
        {
            Waypoints.Add(_Path[i].GirdPosition);
        }
        return Waypoints.ToArray();
    }


    // utility function for a star pathfinding
    int GetDistance(DMapNode _NodeA, DMapNode _NodeB)
    {
        int dstX = (int)Mathf.Abs(_NodeA.GirdPosition.x - _NodeB.GirdPosition.x);
        int dstY = (int)Mathf.Abs(_NodeA.GirdPosition.y - _NodeB.GirdPosition.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }


    // utility function for a star pathfinding
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
                    Neighbours.Add(Map[CheckX, CheckY]);
                }
            }
        }
        return Neighbours;
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
// pivot class
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

// node class
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



