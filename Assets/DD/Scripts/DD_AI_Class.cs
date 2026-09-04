// ----------------------------------------------------------------------
// --------------------  AI: AI Class - Pathfinding 0.09
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DD_AI_Class : MonoBehaviour
{
    // ---------------------------------------------------------------------
    public Vector3[] wayPointPositions = new Vector3[25];
    DD_GameManager gameManager;
    public GameObject wpMarker;

    //Saerch Node
    private List<Node> openSet = new List<Node>();
    private List<Node> closedSet = new List<Node>();
    public List<Node> pathFound = new List<Node>();
    private Node startNode;
    private Node currentNode;
    public bool pathExists;
    public GameObject pathMakerPrefab;

    // ---------------------------------------------------------------------
    private void Start()
    {
        gameManager = GetComponent<DD_GameManager>();
        CreateWaypoints();
    }//----


    // ---------------------------------------------------------------------
    // -----------------------------     A STAR  --------------------------- 
    // ---------------------------------------------------------------------


    // Search Nodes
   

    //------------------------------------------------------------
    public class Node
    {
        public int FCost { get; set; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int XPos { get; set; }
        public int ZPos { get; set; }
        public Vector2 ParentPos { get; set; }
        public Node Parent { get; set; }


    }//-----

    //Heuristic Value using Manhattan distance
    int FindHCost(Vector3 startPosition, Vector3 targetPosition)
    {
        int dx = (int)Mathf.Abs(targetPosition.x - startPosition.x) * 10;
        int dz = (int)Mathf.Abs(targetPosition.z - startPosition.z) * 10;
        return  dx + dz;
    }//-----


    //------------------------------------------------------------
    private List<Node> FindNeighbours(int curX, int curZ)
    {
        List<Node> neighbours = new();    
        int newX, newZ;
        List<Node> allActiveCells = new();
        allActiveCells.AddRange(openSet);
        allActiveCells.AddRange(closedSet);

        for (int i = -1; i <= 1; i++) // Row z
        {
            for (int j = -1; j <= 1; j++) // Col x
            {   
                if(Mathf.Abs(i) == Mathf.Abs(j)) continue; // skip diagonal
                newX = curX + i;
                newZ = curZ + j;

                if (newX >= 0 && newX < gameManager.playArea.GetLength(1))
                {
                    if (newZ >= 0 && newZ < gameManager.playArea.GetLength(0))
                    {
                        //cell walkable
                        if (gameManager.playArea[newZ, newX] == null)
                        {
                            List<Node> xCells = allActiveCells.FindAll(x => x.XPos == newX);
                            Node existingCell = xCells.Find(x => x.ZPos == newZ);

                            if (existingCell == null)
                            {
                               neighbours.Add(new Node() { XPos = newX, ZPos = newZ });   
                            }
                        }
                    }
                }
            }
        }

        return (neighbours);
    }//-----



    //=======================================================================================    
    public List<Vector3> Pathfind(Vector3 pStartPos, Vector3 pTargetPos)
    {
        Debug.Log("Searching");
        pathExists = false;

        //path of positon to return
        List<Vector3> pathToTarget = new();

        //claer the open and closed set
        openSet.Clear();
        closedSet.Clear();
        pathFound.Clear();
        pathToTarget.Clear();

        //Set local variables
        Vector3 startPosition = pStartPos;
        Vector3 currentPosition = pTargetPos;
        Vector3 endPosition = pTargetPos;

        //add start cell to open list
        startNode = new Node()
        {
            XPos = (int)pStartPos.x,
            ZPos = (int)pStartPos.z,
            HCost = FindHCost(startPosition, endPosition),
            GCost = 0,
            FCost = FindHCost(startPosition, endPosition),
        };

        openSet.Add(startNode);

        ///use a for loop first to aviod
        ///
        while(openSet.Count > 0)
        {
            //find lowest F cost node in open list
            currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost || openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost)
                {
                    currentNode = openSet[i];
                }
            }
        }

        openSet.Remove(currentNode);
        closedSet.Add(currentNode);

        //is the currnet node the traget node ---- Found Goal----
        if (currentNode.XPos == endPosition.x && currentNode.ZPos == endPosition.z)
        {
            Debug.Log("Found Target");

            //create list of paht nodes
            Node pathNode = currentNode;

            while (pathNode.Parent != null)
            {
                pathFound.Add(pathNode);
                Instantiate(pathMakerPrefab, new Vector3(pathNode.XPos, -0.99f, pathNode.ZPos),transform.rotation);

                pathNode = pathNode.Parent;
            }
            pathExists = true;

            // make path of psositoins from nodes found
            foreach(Node node in pathFound)
            {
                pathToTarget.Insert(0, new(node.XPos, 0, node.ZPos));
            }
            print("setps to target: " + pathToTarget.Count);
            return pathToTarget;
        }

        //Find open neightbour cells
        List<Node> currentNeighbours = FindNeighbours(currentNode.XPos, currentNode.ZPos);

        foreach (Node neighbourCell in currentNeighbours)
        {
            // has the cell been checked already
            if (closedSet.Contains(neighbourCell)) continue;
            
            neighbourCell.ParentPos = new Vector2(currentNode.XPos, currentNode.ZPos);

            //is the new path to cell shorter
            int newMovementCost;
            int distanceToNeighbour;
            currentPosition = new Vector3 (neighbourCell.XPos, 0, neighbourCell.ZPos);

            if (Vector2.Distance(new Vector2(neighbourCell.XPos, neighbourCell.ZPos), new Vector2(currentNode.XPos, currentNode.ZPos)) < 1.1F)
            {
               distanceToNeighbour = 10;
            }
            else
            {
                distanceToNeighbour = 14;
            }

            newMovementCost = currentNode.GCost + distanceToNeighbour;

            // set the cell costs

            if (newMovementCost < neighbourCell.GCost || !openSet.Contains(neighbourCell))
            {
                neighbourCell.GCost = newMovementCost;
                neighbourCell.HCost = FindHCost(currentPosition, endPosition);
                neighbourCell.FCost = neighbourCell.GCost + neighbourCell.HCost;
                neighbourCell.Parent = currentNode;
                if (!openSet.Contains(neighbourCell))
                {
                    openSet.Add(neighbourCell);
                }
            }
        }



        return null;
    }//-----




    // ---------------------------------------------------------------------
    // -----------------------------     Waypoints --------------------------- 
    // ---------------------------------------------------------------------

    // ---------------------------------------------------------------------
    private void CreateWaypoints()
    {
        // Loop through all Level Map Array slots
        for (int row = 0; row < gameManager.levelData.level1.GetLength(0); row++) // Z axis
        {
            for (int col = 0; col < gameManager.levelData.level1.GetLength(1); col++) // X axis
            {
                int newX = col;
                int newZ = gameManager.levelData.level1.GetLength(0) - 1 - row;              

                if (gameManager.levelData.level1[row, col] > 900)
                {
                    int currentWaypoint = gameManager.levelData.level1[row, col] - 900; // convert to array index
                    wayPointPositions[currentWaypoint].x = newX;
                    wayPointPositions[currentWaypoint].z = newZ;
                    // Create Text marker with WP number
                    GameObject newWP = Instantiate(wpMarker, new Vector3((float)newX, -0.5f, (float)newZ), transform.rotation);
                    newWP.GetComponent<TextMesh>().text = currentWaypoint.ToString();
                    newWP.transform.parent = gameManager.objectParent.transform;
                }
            }
        }
    }//---



    // ---------------------------------------------------------------------
    public List<int> GetListOfWaypointsToTarget(Vector3 pStartPos, Vector3 pTargetPos)  // returns a list 
    {
        List<int> wayPointsFound = new();               
        int startWP = -1, targetWP = -1, nextWP = -1; 
        float distanceToStart = 1000, distanceToTarget = 1000;

        // Loop through all WPs    
        for (int i = 1; i < wayPointPositions.GetLength(0); i++)
        {                      
            float distance = Vector3.Distance(pStartPos, wayPointPositions[i]);  // Start Position   
            if (CheckTargetInLineOfSight(pStartPos, wayPointPositions[i]))
            {
                if (distance < distanceToStart) // WP is closer than previous
                {
                    distanceToStart = distance;
                    startWP = i;
                }
            }
        
            distance = Vector3.Distance(pTargetPos, wayPointPositions[i]);   // Target Position
            if (CheckTargetInLineOfSight(pTargetPos, wayPointPositions[i]))
            {
                if (distance < distanceToTarget) // WP is closer than previous
                {
                    distanceToTarget = distance;
                    targetWP = i;
                }
            }
        }
        if (distanceToTarget == 1000 || distanceToStart == 1000) return wayPointsFound; // stop - no valid waypoints found

        // find Path of Waypoints
        int currentWP = startWP;  

        for (int i = 0; i < wayPointPositions.GetLength(0); i++)
        {
            // Add current WP to List
            wayPointsFound.Add(currentWP);

            // Get Next WP from Table
            nextWP = gameManager.levelData.lookUpTable[currentWP - 1, targetWP - 1];

            if (currentWP == targetWP) // found the target            
                break; // stop looping            
            else            
                currentWP = nextWP;            
        } 
        return wayPointsFound;
    }//---




    // ---------------------------------------------------------------------
    public Vector3 GetNearestWaypointPos(Vector3 currentPosition)
    {
        int nearestWP = 0;
        float distanceToNearestWP = 1000; //  a large number off the board initially

        // Loop through all wps
        for (int i = 1; i < wayPointPositions.GetLength(0); i++)
        {
            float distance = Vector3.Distance(currentPosition, wayPointPositions[i]);

            // is waypoint in line of sight
            if (CheckTargetInLineOfSight(currentPosition, wayPointPositions[i]))
            {
                if (distance < distanceToNearestWP)
                {
                    distanceToNearestWP = distance;
                    nearestWP = i;
                }
            }
        }
        Vector3 wayPointPos = new(wayPointPositions[nearestWP].x, 0, wayPointPositions[nearestWP].z);
        return wayPointPos;
    }//---



    // ---------------------------------------------------------------------
    public bool CheckTargetInLineOfSight(Vector3 pStartPos, Vector3 pTargtetPos)
    {
        Vector3 nextPos = Vector3.zero, currentPos = pStartPos;
        bool canSeeTarget = false;
        bool searching = true;

        while (searching)   // Loop until target  == currentPos or tile is not empty
        {
            // Calculate angle to target
            int dX = (int)Mathf.Round(pTargtetPos.x) - (int)Mathf.Round(currentPos.x);
            int dZ = (int)Mathf.Round(pTargtetPos.z) - (int)Mathf.Round(currentPos.z);
            float angle = Mathf.Atan2(dX, dZ);

            // Calculate next Position
            nextPos.x = currentPos.x + Mathf.Round(1.4F * Mathf.Sin(angle));
            nextPos.z = currentPos.z + Mathf.Round(1.4F * Mathf.Cos(angle));

            // Is the next Pos the target?
            if (nextPos.x == pTargtetPos.x && nextPos.z == pTargtetPos.z)
            {
                canSeeTarget = true;
                searching = false; // exit loop                                
            }

            // is the next Pos empty
            if (gameManager.playArea[(int)Mathf.Round(nextPos.z), (int)Mathf.Round(nextPos.x)] != null)
            {
                searching = false; // exit loop                                   
            }
            currentPos = nextPos;
        }
        return canSeeTarget;
    }//-----


}//==========

