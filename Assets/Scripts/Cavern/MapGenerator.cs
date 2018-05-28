using UnityEngine;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour
{
    MeshGenerator meshGenerator;

    public int width, height;
    public bool GenerateInRunTime = false;
    public string seed;
    public bool generateRandomSeed;
    public bool generateExit;
    public bool generateEntrance;

    [Range(1, 10)]
    public int borderSize = 2;

    [Range(0, 30)]
    public int smoothness = 10;

    [Range(0, 100)]
    public int fillPercent;

    enum Meshes { EMPTY = 0, WALL = 1 };
    int[,] map;

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that initializes variables/objects
    // ------------------------------
    void Start()
    {
        meshGenerator = GetComponent<MeshGenerator>();
        generateRandomSeed = true;

        if (GenerateInRunTime)
            GenerateMap();
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that checks for a right mouse button click and generates a cave 
    // ------------------------------
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            GenerateMap();
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: Function that generates a cave and adds border 
    // ------------------------------
    public void GenerateMap()
    {
        map = new int[width, height];

        PopulateMap();

        for (int i = 0; i < smoothness; i++)
        {
            SmoothMap();
        }

        ProcessMap();

        int borderSize = 1;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int i = 0; i < borderedMap.GetLength(0); ++i)
            for (int j = 0; j < borderedMap.GetLength(1); ++j)
                if (j >= borderedMap.GetLength(1) - 5 && j <= borderedMap.GetLength(1) && generateExit)
                    // Create exit(s)
                    borderedMap[i, j] = (int)Meshes.EMPTY;
                else if (i >= borderSize && i < width + borderSize && j >= borderSize && j < height + borderSize)
                    borderedMap[i, j] = map[i - borderSize, j - borderSize];
                else
                    borderedMap[i, j] = (int)Meshes.EMPTY;

        meshGenerator.GenerateMesh(borderedMap, 1);
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    void PopulateMap()
    {
        if (generateRandomSeed)
                seed = Time.time.ToString() + UnityEngine.Random.Range(0, 1000);

        System.Random randomNum = new System.Random(seed.GetHashCode());

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                if ((i >= 0 && i <= 5 + borderSize) || (i >= width - 5 - borderSize && i <= width))
                {
                    // Always put walls on the sides of the cave
                    map[i, j] = (int)Meshes.WALL;
                }
                else if (i >= width / 2 - 5 && i <= width / 2 + 5 && j >= 0 && j <= 7 && generateEntrance)
                {
                    // Create entrance
                    map[i, j] = (int)Meshes.EMPTY;
                }
                else if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                {
                    // Always put walls around the cave
                    map[i, j] = (int)Meshes.WALL;
                }
                else
                {
                    // Obtain random number (either 1 [wall] or 0 [empty])
                    map[i, j] = (randomNum.Next(0, 100) < fillPercent) ? (int)Meshes.WALL : (int)Meshes.EMPTY;
                }
            }
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    void SmoothMap()
    {
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                int counter = GetNeighbours(i, j);

                if (counter > 4)
                    map[i, j] = (int)Meshes.WALL;
                else if (counter < 4)
                    map[i, j] = (int)Meshes.EMPTY;
            }
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    int GetNeighbours(int gridX, int gridY)
    {
        int counter = 0;
        for (int i = gridX - 1; i <= gridX + 1; ++i)
        {
            for (int j = gridY - 1; j <= gridY + 1; ++j)
            {
                if (IsInRange(i, j))
                {
                    // Check if this is the current tile
                    if (i != gridX || j != gridY)
                    {
                        counter += map[i, j];
                    }
                }
                else
                {
                    // Increase wall count if this is an edge 
                    ++counter;
                }
            }
        }

        return counter;
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    bool IsInRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    void ProcessMap()
    {
        List<Room> survivingRooms = new List<Room>();
        List<List<Coord>> wallRegions = GetRegions((int)Meshes.WALL);
        List<List<Coord>> roomRegions = GetRegions((int)Meshes.EMPTY);

        int numOfWalls = 50;
        int numOfEmpty = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            // Check if this region contains less than 50 walls
            if (wallRegion.Count < numOfWalls)
            {
                // If so, set this wall region to empty
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = (int)Meshes.EMPTY;
                }
            }
        }

        foreach (List<Coord> roomRegion in roomRegions)
        {
            // Check if this region contains less than 50 walls
            if (roomRegion.Count < numOfEmpty)
            {
                // If so, set this empty region to wall
                foreach (Coord tile in roomRegion)
                {
                    //map[tile.tileX, tile.tileY] = (int)Meshes.WALL;
                }
            }
            else
            {
                // Otherwise, this region has survived the elmination process, add it to the list of surviving rooms
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        //survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        List<Coord> line = GetLine(tileA, tileB);

        foreach (Coord c in line)
        {
            DrawCircle(c, 5);
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    List<List<Coord>> GetRegions(int tileTjpe)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                if (mapFlags[i, j] == (int)Meshes.EMPTY && map[i, j] == tileTjpe)
                {
                    List<Coord> newRegion = GetRegionTiles(i, j);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = (int)Meshes.WALL;
                    }
                }
            }
        }

        return regions;
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = (int)Meshes.WALL;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = (int)Meshes.WALL;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    class Room : IComparable<Room>
    {
        public Room()
        { }

        // Coordinates of all points that the room cointains 
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;

        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();
            edgeTiles = new List<Coord>();

            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (x >= 1 && y >= 1)
                            {
                                if (map[x, y] == (int)Meshes.WALL)
                                {
                                    edgeTiles.Add(tile);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        // ------------------------------
        // Author: Rony Hanna
        // Description: 
        // ------------------------------
        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        // ------------------------------
        // Author: Rony Hanna
        // Description: 
        // ------------------------------
        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }

            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        // ------------------------------
        // Author: Rony Hanna
        // Description: 
        // ------------------------------
        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        // ------------------------------
        // Author: Rony Hanna
        // Description: 
        // ------------------------------
        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    // ------------------------------
    // Author: Rony Hanna
    // Description: 
    // ------------------------------
    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;

                    if (IsInRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }
}