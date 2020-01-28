using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MazeMaker : MonoBehaviour
{

    [System.Serializable]
    public class Cell
    {
        public bool visited;
        public GameObject north;//1
        public GameObject east;//2
        public GameObject west;//3
        public GameObject south;//4

    }


    [SerializeField]
    private Camera camera;

    [SerializeField]
    private InputField HeightInput;
    [SerializeField]
    private InputField WidthInput;

    [SerializeField]
    private GameObject wall;
    [SerializeField]
    private GameObject floor;


    private GameObject wallHolder;
    private GameObject tempWall;
    private GameObject tempFloor;

    private int row = 5;
    private int column = 5;
    private int totalCells;
    private int currentNeighbour = 0;
    private int backingUp = 0;
    private int currentCell = 0;
    private float wallLength = 1.0f;

    private Vector3 beginPosition;

    public Cell[] cells;

    List<int> cellList;

    public void GenerateNewMaze()
    {
        //Finding the parent object of the walls if exists
        GameObject previousMaze = GameObject.FindGameObjectWithTag("Walls");
       
            row = int.Parse(HeightInput.text);
            column = int.Parse(WidthInput.text);
            totalCells = row * column;
            Transform cameraPosition = camera.transform;
            int cameraHeight;

        

        if (row >= column)
        {
            cameraHeight = row;
        }
        else { 
            cameraHeight = column;
            }

            cameraPosition.position = new Vector3(0, cameraHeight + 1, 0);
            //Destroying previous maze if exist
            if (previousMaze != null)
            {
                Destroy(previousMaze);
            }
            CreateWall();
  
       
    }

    /// Creating wall gameobject based on rows and columns given
    private void CreateWall()
    {
        wallHolder = new GameObject();
        wallHolder.name = "Walls";
        wallHolder.tag = "Walls";
        tempFloor = new GameObject();

        beginPosition = new Vector3((-column / 2) + wallLength / 2, 0.0f, (-row / 2) + wallLength / 2);
        Vector3 myPos = beginPosition;
        
        //for creating columns
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j <= column; j++)
            {
                myPos = new Vector3(beginPosition.x + (j * wallLength) - wallLength / 2, 0.0f, beginPosition.z + (i * wallLength) - wallLength / 2);
                tempWall = Instantiate(wall, myPos, Quaternion.identity) as GameObject;
                tempWall.name = "column " + i + "," + j;
                tempWall.transform.parent = wallHolder.transform;
            }
        }

        //for creating rows
        for (int i = 0; i <= row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                myPos = new Vector3(beginPosition.x + (j * wallLength), 0.0f, beginPosition.z + (i * wallLength) - wallLength);
                tempWall = Instantiate(wall, myPos, Quaternion.Euler(0, 90, 0)) as GameObject;
                tempWall.name = "row " + i + "," + j;
                tempWall.transform.parent = wallHolder.transform;
            }
        }

        for (int i = 0; i <= row - 1; i++)
        {
            for (int j = 0; j < column; j++)
            {
                myPos = new Vector3(beginPosition.x + (j * wallLength), -0.5f, beginPosition.z + (i * wallLength) - wallLength / 2);
                tempFloor = Instantiate(floor, myPos, Quaternion.identity) as GameObject;
                tempFloor.transform.parent = wallHolder.transform;
            }
        }
        CreateCells();
    }

  
    /// Assigning created walls to the cells direction (north,east,west,south)
    private void CreateCells()
    {
        cellList = new List<int>();
        int children = wallHolder.transform.childCount;
        GameObject[] allWalls = new GameObject[children];
        cells = new Cell[totalCells];

        int eastWestProccess = 0;
        int childProcess = 0;
        int termCount = 0;
        int cellProccess = 0;

        //Assigning all the walls to the allwalls array
        for (int i = 0; i < children; i++)
        {
            allWalls[i] = wallHolder.transform.GetChild(i).gameObject;
        }

        //Assigning walls to the cells
        for (int j = 0; j < column; j++)
        {
            cells[cellProccess] = new Cell();

            cells[cellProccess].west = allWalls[eastWestProccess];
            cells[cellProccess].south = allWalls[childProcess + (column + 1) * row];
            termCount++;
            childProcess++;
            cells[cellProccess].north = allWalls[(childProcess + (column + 1) * row) + column - 1];
            eastWestProccess++;
            cells[cellProccess].east = allWalls[eastWestProccess];

            cellProccess++;
            if (termCount == column && cellProccess < cells.Length)
            {
                eastWestProccess++;
                termCount = 0;
                j = -1;
            }

        }
        CreateMaze();
    }

    
    /// Getting a random neighbour if not visited and wall between them
    
   private void GiveMeNeighbour()
    {
        int length = 0;
        int[] neighbour = new int[4];
        int[] connectingWall = new int[4];
        int check = 0;
        check = (currentCell + 1) / column;
        check -= 1;
        check *= column;
        check += column;

        //north
        if (currentCell + column < totalCells)
        {
            if (cells[currentCell + column].visited == false)
            {
                neighbour[length] = currentCell + column;
                connectingWall[length] = 1;
                length++;
            }
        }

        //east
        if (currentCell + 1 < totalCells && (currentCell + 1) != check)
        {
            if (cells[currentCell + 1].visited == false)
            {
                neighbour[length] = currentCell + 1;
                connectingWall[length] = 2;
                length++;
            }
        }

        //west
        if (currentCell - 1 >= 0 && currentCell != check)
        {
            if (cells[currentCell - 1].visited == false)
            {
                neighbour[length] = currentCell - 1;
                connectingWall[length] = 3;
                length++;
            }
        }

        //south
        if (currentCell - column >= 0)
        {
            if (cells[currentCell - column].visited == false)
            {
                neighbour[length] = currentCell - column;
                connectingWall[length] = 4;
                length++;
            }
        }

        //Getting random neighbour and destroying the wall
        if (length != 0)
        {
            int randNeighbour = Random.Range(0, length);
            currentNeighbour = neighbour[randNeighbour];
            DestroyWall(connectingWall[randNeighbour]);
        }
        else if (backingUp > 0)
        {
            currentCell = cellList[backingUp];
            backingUp--;
        }
    }

  private void CreateMaze()
    {
        bool startedBuilding = false;
        int visitedCells = 0;
        while (visitedCells < totalCells)
        {
            if (startedBuilding)
            {
                GiveMeNeighbour();
                if (!cells[currentNeighbour].visited && cells[currentCell].visited)
                {
                    int randomNeighbour = Random.Range(0, 5);
                    cells[currentNeighbour].visited = true;
                    visitedCells++;
                    cellList.Add(currentCell);
                    currentCell = currentNeighbour;

                    if (cellList.Count > 0)
                        backingUp = cellList.Count - 1;
                }
            }
            else
            {
                currentCell = Random.Range(0, totalCells);
                cells[currentCell].visited = true;
                visitedCells++;
                startedBuilding = true;
            }
        }
    }

    private void DestroyWall(int neighbour)
    {
        switch (neighbour)
        {
            //case 1 means north wall
            case 1:
                Destroy(cells[currentCell].north);
                break;

            //case 2 means east wall
            case 2:
                Destroy(cells[currentCell].east);
                break;

            //case 3 means west wall
            case 3:
                Destroy(cells[currentCell].west);
                break;

            //case 4 means south wall
            case 4:
                Destroy(cells[currentCell].south);
                break;

            default:
                break;
        }
    }
}



