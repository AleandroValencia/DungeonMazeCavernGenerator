using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NONSTATICMazeGenerator : MonoBehaviour
{
    [System.Serializable]
    public class Cell
    {
        public int index = 0;
        public bool visited = false;
        public bool edgeCell = false;
        public GameObject northWall = null;
        public GameObject eastWall = null;
        public GameObject westWall = null;
        public GameObject southWall = null;
        public GameObject floorTile = null;
    }

    public enum WALLINDEX
    {
        NORTH = 1,
        EAST,
        WEST,
        SOUTH
    }

    private struct Marker
    {
        public GameObject m_markerObject;
        public int m_pattern;
    }

    public  string m_name = "Maze";
    public  int m_numRows = 2;
    public  int m_numColumns = 2;
    public  GameObject m_wall;
    public  GameObject m_floor;
    public  GameObject m_marker;
    public  GameObject m_markerFront;
    public  bool m_tiles = true;
    public  bool GenerateInRunTime = false;

    private  bool m_horizontalWall = false;
    private  GameObject m_mazeObject;
    private  Cell[] m_cells;

    private  int m_currentCell = 0;
    private  int m_totalCells = 0;
    private  int m_currentNeighbor = 0;
    private  int m_previousCell = 0;
    private  List<int> m_cellStack;
    private  WALLINDEX m_wallToBreak = 0;

    private  int m_endTileDistance = 0;
    private  Cell m_endCell;
    private  bool m_deadEnd = false;

    private  int m_markerCount = 0;
    private  List<Marker> m_markers;
    private  GameObject m_emptyGameObject;

    // Use this for initialization
    void Start()
    {
        if (GenerateInRunTime)
        {
            GenerateMaze();
        }
    }

    /// <summary>
    /// Reset values to 0 because of  script
    /// </summary>
     void ResetValues()
    {
        m_currentCell = 0;
        m_totalCells = 0;
        m_currentNeighbor = 0;
        m_previousCell = 0;
        m_wallToBreak = 0;
        m_endTileDistance = 0;
        m_markerCount = 0;
        m_endCell = null;
        if (m_markers != null)
            m_markers.Clear();
        else
            m_markers = new List<Marker>();
        if (m_cellStack != null)
            m_cellStack.Clear();
    }

    /// <summary>
    /// Generates the Mazeobject with walls and floor
    /// </summary>
    /// <returns>Gameobject holding the walls and floor of the maze</returns>
    public  GameObject GenerateMaze()
    {
        m_emptyGameObject = new GameObject();
        ResetValues();
        m_totalCells = m_numColumns * m_numRows;
        CreateWalls();
        CreateCells();
        CreateMaze();

        return m_mazeObject;
    }

    /// <summary>
    /// Generate walls in a grid formation
    /// </summary>
     void CreateWalls()
    {
        // Create the object to hold the maze walls and floor
        m_mazeObject = new GameObject();
        m_mazeObject.name = m_name;
        float wallLength = 1.0f;

        // Get the longer side of the wall gameobject and set the wallLength
        if (m_wall.GetComponent<Transform>().localScale.x > m_wall.GetComponent<Transform>().localScale.z)
        {
            wallLength = m_wall.GetComponent<Transform>().localScale.x;
            m_horizontalWall = true;
        }
        else
        {
            wallLength = m_wall.GetComponent<Transform>().localScale.z;
            m_horizontalWall = false;
        }

        // Calculate the bottom left corner of the maze using the wallLenght and number of rows and columns
        Vector3 bottomLeftCorner = new Vector3((-m_numColumns / 2) + wallLength / 2, 0.0f, (-m_numRows / 2) + wallLength / 2);
        Vector3 currentPos = bottomLeftCorner;
        GameObject tempWall;

        // Place walls along x axis
        for (int z = 0; z < m_numRows; ++z)
        {
            for (int x = 0; x <= m_numColumns; ++x)
            {
                currentPos = new Vector3((bottomLeftCorner.x + (x * wallLength) - wallLength / 2), 0.0f, bottomLeftCorner.z + (z * wallLength) - wallLength / 2);
                if (m_horizontalWall)
                {
                    tempWall = Instantiate(m_wall, currentPos, Quaternion.Euler(0.0f, 90.0f, 0.0f), m_mazeObject.transform);
                }
                else
                {
                    tempWall = Instantiate(m_wall, currentPos, Quaternion.identity, m_mazeObject.transform);
                }
            }
        }

        // Place walls along z axis
        for (int z = 0; z <= m_numRows; ++z)
        {
            for (int x = 0; x < m_numColumns; ++x)
            {
                currentPos = new Vector3(bottomLeftCorner.x + (x * wallLength), 0.0f, bottomLeftCorner.z + (z * wallLength) - wallLength);
                if (m_horizontalWall)
                {
                    tempWall = Instantiate(m_wall, currentPos, Quaternion.identity, m_mazeObject.transform);
                }
                else
                {
                    tempWall = Instantiate(m_wall, currentPos, Quaternion.Euler(0.0f, 90.0f, 0.0f), m_mazeObject.transform);
                }
            }
        }

        CreateFloor(wallLength);
    }

    /// <summary>
    /// Creates the floor of the maze
    /// </summary>
    /// <param name="_wallLength">Length of the wall therefore length of the floor tile of a cell</param>
     void CreateFloor(float _wallLength)
    {
        GameObject tempFloor;

        // Calculate yPosition of floor tile.
        float yPos = -(m_wall.GetComponent<Transform>().localScale.y / 2) - (m_floor.GetComponent<Transform>().localScale.y / 2);

        if (m_tiles)
        {
            // Scale tile to fit one cell
            Vector3 scale = m_floor.GetComponent<Transform>().localScale;
            scale.x = _wallLength;
            scale.z = _wallLength;
            m_floor.GetComponent<Transform>().localScale = scale;

            Vector3 bottomLeftCorner = new Vector3((-m_numColumns / 2) + _wallLength / 2, yPos, (-m_numRows / 2));
            Vector3 currentPos = bottomLeftCorner;

            for (int z = 0; z < m_numRows; ++z)
            {
                for (int x = 0; x < m_numColumns; ++x)
                {
                    currentPos = new Vector3((bottomLeftCorner.x + (x * _wallLength)), yPos, bottomLeftCorner.z + (z * _wallLength));
                    tempFloor = Instantiate(m_floor, currentPos, Quaternion.identity, m_mazeObject.transform);
                }
            }
        }
        else
        {
            // Scale tile to fit whole maze
            Vector3 scale = m_floor.GetComponent<Transform>().localScale;
            scale.x = _wallLength * m_numColumns;
            scale.z = _wallLength * m_numRows;
            m_floor.GetComponent<Transform>().localScale = scale;
            Vector3 pos = m_mazeObject.transform.position;
            pos.x += _wallLength / 2;
            pos.y = yPos;

            tempFloor = Instantiate(m_floor, pos, Quaternion.identity, m_mazeObject.transform);
        }
    }

    /// <summary>
    /// Assigns each square on the grid to a cell object
    /// </summary>
     void CreateCells()
    {
        m_cellStack = new List<int>();
        m_cellStack.Clear();
        GameObject[] allWalls;
        int numWalls = m_mazeObject.transform.childCount;
        allWalls = new GameObject[numWalls];
        m_cells = new Cell[m_numRows * m_numColumns];
        int verticalWallCount = 0;
        int horizontalWallCount = 0;
        int indexInRow = 0;

        // Populate the array with the children of the maze gameobject (includes floor tiles)
        for (int i = 0; i < numWalls; ++i)
        {
            allWalls[i] = m_mazeObject.transform.GetChild(i).gameObject;
        }

        // Create a cell for each square on the grid
        for (int i = 0; i < m_cells.Length; ++i)
        {
            if (indexInRow == m_numRows)
            {
                verticalWallCount++;
                indexInRow = 0;
            }
            m_cells[i] = new Cell();
            m_cells[i].index = i;
            m_cells[i].westWall = allWalls[verticalWallCount];
            m_cells[i].southWall = allWalls[horizontalWallCount + (m_numRows + 1) * m_numColumns];

            verticalWallCount++;
            indexInRow++;
            horizontalWallCount++;

            m_cells[i].eastWall = allWalls[verticalWallCount];
            m_cells[i].northWall = allWalls[(horizontalWallCount + (m_numRows + 1) * m_numColumns) + m_numRows - 1];

            // Formula to count total wall index: 4 + 2(xy - x - y + 1) + 3(x + y - 2)
            int floorTile = 4 + 2 * (m_numRows * m_numColumns - m_numColumns - m_numRows + 1) + 3 * (m_numColumns + m_numRows - 2);
            if (m_tiles)
            {
                m_cells[i].floorTile = allWalls[floorTile + i];
            }
            else
            {
                m_cells[i].floorTile = allWalls[floorTile];
            }

            // Determine if cell is on the edge
            for (int j = 1; j < m_numRows; ++j)
            {
                // Remove the last two "OR" statements to just make the top and bottom rows edge cells
                if (i < m_numColumns || i > m_totalCells - m_numColumns || i == m_numRows * j || i == (m_numRows * (j + 1)) - 1)
                {
                    m_cells[i].edgeCell = true;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Starts on an edge and traverses to unvisited cells and removes the walls inbetween them
    /// until a cell's neighbours have all been visited then backtracks visiting unvisited cells
    /// </summary>
     void CreateMaze()
    {
        m_currentCell = Random.Range(0, m_numColumns);
        m_cells[m_currentCell].visited = true;
        DestroyImmediate(m_cells[m_currentCell].southWall);
        int numVisitedCells = 1;

        while (numVisitedCells < m_totalCells)
        {
            GetNeighbor();
            if (m_cells[m_currentNeighbor].visited == false && m_cells[m_currentCell].visited == true)
            {
                RemoveWall();
                m_cells[m_currentNeighbor].visited = true;
                numVisitedCells++;
                m_cellStack.Add(m_currentCell);
                m_currentCell = m_currentNeighbor;
                if (m_cellStack.Count > 0)
                {
                    m_previousCell = m_cellStack.Count - 1;
                }
            }
        }

        BreakEndCellWall();
        Debug.Log("Dead ends: " + m_markerCount);
    }

    /// <summary>
    /// Destroys the wall of the exit tile
    /// </summary>
     void BreakEndCellWall()
    {
        // Break end cell wall
        if (m_endCell == null)
        {
            Debug.LogError("Error in code: m_endCell not set. Try Again");
        }
        else
        {
            if (m_endCell.index >= m_totalCells - m_numColumns)
            {
                DestroyImmediate(m_endCell.northWall);
            }
            else if (m_endCell.index < m_numColumns)
            {
                DestroyImmediate(m_endCell.southWall);
            }
            else
            {
                for (int i = 0; i < m_numRows; ++i)
                {
                    if (m_endCell.index == m_numRows * i)
                    {
                        DestroyImmediate(m_endCell.westWall);
                    }
                    else if (m_endCell.index == (m_numRows * (i + 1)) - 1)
                    {
                        DestroyImmediate(m_endCell.eastWall);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Destroys the wall depending on m_wallToBreak var
    /// </summary>
     void RemoveWall()
    {
        switch (m_wallToBreak)
        {
            case WALLINDEX.NORTH: DestroyImmediate(m_cells[m_currentCell].northWall); break;
            case WALLINDEX.EAST: DestroyImmediate(m_cells[m_currentCell].eastWall); break;
            case WALLINDEX.WEST: DestroyImmediate(m_cells[m_currentCell].westWall); break;
            case WALLINDEX.SOUTH: DestroyImmediate(m_cells[m_currentCell].southWall); break;
            default: break;
        }
    }

    /// <summary>
    /// Checks for neighbours. If a neighbour exists, traverse to that cell and break the wall linking them
    /// Else backtrack to previous cell
    /// </summary>
     void GetNeighbor()
    {
        int index = 0;
        int[] neighbors = new int[4];
        WALLINDEX[] connectingWall = new WALLINDEX[4];

        // Check for neighbour EAST of current cell
        if (m_currentCell + 1 < m_totalCells && (m_currentCell + 1) % m_numColumns != 0)
        {
            if (!m_cells[m_currentCell + 1].visited)
            {
                neighbors[index] = m_currentCell + 1;
                connectingWall[index] = WALLINDEX.EAST;
                index++;
            }
        }
        // Check for neighbour WEST of current cell
        if (m_currentCell - 1 >= 0 && (m_currentCell % m_numColumns) != 0)
        {
            if (!m_cells[m_currentCell - 1].visited)
            {
                neighbors[index] = m_currentCell - 1;
                connectingWall[index] = WALLINDEX.WEST;
                index++;
            }
        }
        // Check for neighbour NORTH of current cell
        if (m_currentCell + m_numColumns < m_totalCells)
        {
            if (!m_cells[m_currentCell + m_numColumns].visited)
            {
                neighbors[index] = m_currentCell + m_numColumns;
                connectingWall[index] = WALLINDEX.NORTH;
                index++;
            }
        }
        // Check for neighbor SOUTH of current cell
        if (m_currentCell - m_numColumns >= 0)
        {
            if (!m_cells[m_currentCell - m_numColumns].visited)
            {
                neighbors[index] = m_currentCell - m_numColumns;
                connectingWall[index] = WALLINDEX.SOUTH;
                index++;
            }
        }

        // If a neighbour exists
        if (index != 0)
        {
            // Select a random neighbour and make the current cell that neighbour and destroy the wall linking them
            int selectedNeighbor = Random.Range(0, index);
            m_currentNeighbor = neighbors[selectedNeighbor];
            m_wallToBreak = connectingWall[selectedNeighbor];

            if (m_deadEnd)
            {
                m_deadEnd = false;
            }
        }
        else
        {
            SetMarker();
            SetEndTile();

            // Backtrack to previous cell
            if (m_previousCell > 0)
            {
                m_currentCell = m_cellStack[m_previousCell];
                --m_previousCell;
            }
        }
    }

    /// <summary>
    /// Sets the end tile if distance is greater than previous distance and is an edge tile
    /// </summary>
     void SetEndTile()
    {
        // if current cell is an edge cell
        if (m_cells[m_currentCell].edgeCell == true)
        {
            // If distance from start cell is the longest
            //if (m_cellStack.Count > m_endTileDistance)
            if (m_previousCell + 1 > m_endTileDistance)
            {
                // Assign the end cell
                m_endTileDistance = m_cellStack.Count;
                m_endCell = m_cells[m_currentCell];
            }
        }
    }

    /// <summary>
    /// Sets a marker if reached a dead end
    /// </summary>
     void SetMarker()
    {
        // Reached a dead end (not backtracking)
        if (!m_deadEnd)
        {
            m_markerCount++;
            m_deadEnd = true;
            GameObject marker = null;
            Transform markerTransform;

            // Calculate position on the parent wall
            //float yPos = (Mathf.Floor((m_markerCount % 7) / 3) - 1) / 3.0f;
            //float zPos = (m_markerCount % 3 - 1) / 3.0f;
            float yPos = 0.0f;
            float zPos = 0.0f;

            // Set marker
            if (m_cells[m_currentCell].northWall == null)
            {
                // Apply marker to southwall
                //marker = Instantiate(m_marker, m_cells[m_currentCell].southWall.transform);
                marker = GenerateMarker(m_cells[m_currentCell].southWall.transform);
                markerTransform = marker.GetComponent<Transform>();
                markerTransform.localPosition = new Vector3(-0.51f, yPos, zPos);
                markerTransform.localRotation = Quaternion.AngleAxis(90, Vector3.up);
                markerTransform.localScale = new Vector3(0.3f, 0.25f, 1);
            }
            else if (m_cells[m_currentCell].southWall == null)
            {
                // Apply marker to northwall
                // marker = Instantiate(m_marker, m_cells[m_currentCell].northWall.transform);
                marker = GenerateMarker(m_cells[m_currentCell].northWall.transform);
                markerTransform = marker.GetComponent<Transform>();
                markerTransform.localPosition = new Vector3(0.51f, yPos, zPos);
                markerTransform.localRotation = Quaternion.AngleAxis(-90, Vector3.up);
                markerTransform.localScale = new Vector3(0.3f, 0.25f, 1);
            }
            else if (m_cells[m_currentCell].eastWall == null)
            {
                // Apply marker to westwall  
                //marker = Instantiate(m_marker, m_cells[m_currentCell].westWall.transform);
                marker = GenerateMarker(m_cells[m_currentCell].westWall.transform);
                markerTransform = marker.GetComponent<Transform>();
                markerTransform.localPosition = new Vector3(0.51f, yPos, zPos);
                markerTransform.localRotation = Quaternion.AngleAxis(-90, Vector3.up);
                markerTransform.localScale = new Vector3(0.3f, 0.25f, 1);
            }
            else if (m_cells[m_currentCell].westWall == null)
            {
                // Apply marker to eastwall 
                // marker = Instantiate(m_marker, m_cells[m_currentCell].eastWall.transform);
                marker = GenerateMarker(m_cells[m_currentCell].eastWall.transform);
                markerTransform = marker.GetComponent<Transform>();
                markerTransform.localPosition = new Vector3(-0.51f, yPos, zPos);
                markerTransform.localRotation = Quaternion.AngleAxis(90, Vector3.up);
                markerTransform.localScale = new Vector3(0.3f, 0.25f, 1);
            }
            marker.name = "Marker";
        }
    }

     GameObject GenerateMarker(Transform _wallParent)
    {
        bool generating = true;
        Marker parentMarker = new Marker();
        parentMarker.m_markerObject = Instantiate(m_emptyGameObject, _wallParent);
        GameObject markerChild = null;

        while (generating)
        {
            int count = 0;
            for (int i = 0; i < 9; ++i)
            {
                if (Random.Range(0, 2) == 0)
                {
                    markerChild = m_marker;
                }
                else
                {
                    markerChild = m_markerFront;
                    count += i;
                }

                if (markerChild != null)
                {
                    // Set position in square
                    float xPos = (((i + 1) % 3) - 1) / 3.0f;
                    float yPos = (Mathf.Floor(i / 3) - 1) / 3.0f;
                    Vector3 pos = markerChild.transform.localPosition;
                    pos.x = xPos;
                    pos.y = yPos;
                    markerChild.transform.localPosition = pos;
                    markerChild.transform.localScale = new Vector3(0.3f, 0.3f, 1.0f);

                    if (parentMarker.m_markerObject != null)
                    {
                        markerChild.name = "Child";
                        Instantiate(markerChild, parentMarker.m_markerObject.transform);
                    }
                }
            }

            parentMarker.m_pattern = count;

            if (m_markers != null && m_markers.Count > 0)
            {
                foreach (Marker marker in m_markers)
                {
                    if (count == marker.m_pattern)
                    {
                        generating = true;
                        for (int i = parentMarker.m_markerObject.transform.childCount - 1; i >= 0; i--)
                        {
                            DestroyImmediate(parentMarker.m_markerObject.transform.GetChild(i).gameObject);
                        }
                        break;
                    }
                    else
                    {
                        generating = false;
                    }
                }
            }
            else
            {
                generating = false;
            }
        }

        m_markers.Add(parentMarker);
        return parentMarker.m_markerObject;
    }

     int NthTriangleNumber(int _n)
    {
        return ((_n * (_n + 1)) / 2);
    }
}
