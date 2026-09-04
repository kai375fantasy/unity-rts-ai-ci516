// ----------------------------------------------------------------------
// --------------------  AI Game Manager: 0.09
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DD_GameManager : MonoBehaviour
{
    // ---------------------------------------------------------------------
    [Header("Game UI")]
    public Text LeftTextWindow;

    [Header("Teams")]
    public GameObject[] teams = new GameObject[2];
    public int teamClearDiameter = 10;

    [Header("Game Objects")]
    public GameObject objectParent;
    public GameObject obstaclePrefab;
    public Vector2 obstaclesMinMax = new(100, 500);
    public GameObject resourcePF;
    public int resourcesToSpawn = 100;

    [Header("Mouse Clicked Position Markers")]
    public GameObject selectionMarkerPrefab;
    public GameObject redMarkerPrefab;
    private GameObject redTargetMarker;
    private GameObject selectionMarker;
    private bool playerMarkerActive = false;
    public Vector3 playerSetTargetPos = new(-50, 50, -50);

    // Game Data
    public GameObject[,] playArea = { };
    public List<GameObject> activeUnits = new();
    private readonly List<GameObject> activeTeams = new();
    public List<GameObject> activeResources = new();
    public List<GameObject> selectedPlayerUnits = new();

    // Connected Other Classes
    public DD_PlayerInputManager playerInputManager;
    public DD_LevelData levelData;
    public DD_AI_Class ai;

    public DD_Team playerTeam;
    public JQ_StartGame startGame;
    private int playerUnitsKilled = 0;


    // ---------------------------------------------------------------------
    private void Awake()// Runs before start on other objects
    {
        // Create an Array for play area or Game Board
        playArea = new GameObject[100, 100]; // rows z, cols x
        playerInputManager = GetComponent<DD_PlayerInputManager>();

        levelData = GetComponent<DD_LevelData>();
        ai = GetComponent<DD_AI_Class>();

    }//------

    // ---------------------------------------------------------------------
    private void Start()
    {
        AddInitialGameObjects();
    }//---

    // ---------------------------------------------------------------------
    private void FixedUpdate() // Capped at 50 FPS
    {
        DisplayGameData();
        CheckVictoryCondition();


    }//---

    // ---------------------------------------------------------------------
    private void Update() // variable FPS based on frame render time
    {
        GetPlayerSelection();
    }//---


    // ---------------------------------------------------------------------
    private void AddInitialGameObjects()
    {
        LoadLevel();
        AddTeams();
        AddInitialResourceObjects(resourcesToSpawn);
        // AddObstacles((int)obstaclesMinMax.x, (int)obstaclesMinMax.y);    
    }//------





    //****************************************************************************
    // ************          Selection Methods
    //****************************************************************************



    // ---------------------------------------------------------------------
    private void SelectUnits()
    {
        selectedPlayerUnits.Clear(); // clear last selection

        if (selectedPlayerUnits.Count == 0) // Deselect unit
            foreach (GameObject unit in activeUnits)
                if (unit.GetComponent<DD_UnitPlayerControl>())
                    unit.GetComponent<DD_UnitPlayerControl>().isSelected = false;

        // find units within these array position based on Player mouse clicks     
        int startCol = (int)playerInputManager.leftClickPosition.x;
        int startRow = (int)playerInputManager.leftClickPosition.y;
        int endCol = (int)playerInputManager.leftDownPosition.x;
        int endRow = (int)playerInputManager.leftDownPosition.y;

        // ensure position are on the board
        if (startCol < 0 || startCol > playArea.GetLength(1)) return;
        if (startRow < 0 || startRow > playArea.GetLength(0)) return;
        if (endCol < 0 || endCol > playArea.GetLength(1)) return;
        if (endRow < 0 || endRow > playArea.GetLength(0)) return;

        // Swap start and end if end is lower
        if (endCol < startCol) (startCol, endCol) = (endCol, startCol);
        if (endRow < startRow) (startRow, endRow) = (endRow, startRow);

        // Loop through all selected slots to find a PC unit
        for (int col = startCol; col <= endCol; col++)
        {
            for (int row = startRow; row <= endRow; row++)
            {
                if (playArea[row, col] && playArea[row, col].GetComponent<DD_Unit>()) // Ensure the object is a unit
                {
                    if (playArea[row, col].GetComponent<DD_Unit>().team.teamID == 1) // the Player controlled team
                        selectedPlayerUnits.Add(playArea[row, col]);
                }
            }
        }
        // print(selectedPlayerUnits.Count + " units selected");

        // Set each unit as isSelected
        if (selectedPlayerUnits.Count > 0)
            foreach (GameObject unit in selectedPlayerUnits)
                unit.GetComponent<DD_UnitPlayerControl>().isSelected = true;

    }//-----



    // ---------------------------------------------------------------------
    private void SetSquad()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!playerTeam.squadActive)
            {
                print("Setting squad");
                playerTeam.squad.Clear();
                playerTeam.squadActive = true;
                foreach (GameObject unit in selectedPlayerUnits)
                {
                    playerTeam.squad.Add(unit);
                }
            }
            else
            {
                print("Clearing squad");
                playerTeam.squad.Clear();
                playerTeam.squadActive = false;
            }
        }
    }//-----


    // ---------------------------------------------------------------------
    private void GetPlayerSelection()
    {
        //place marker gameobject 
        if (selectionMarker == null) // if there is no marker already in the scene add one        
            selectionMarker = (GameObject)Instantiate(selectionMarkerPrefab, new(playerInputManager.leftDownPosition.x, -0.9F, playerInputManager.leftDownPosition.y), transform.rotation);

        if (playerInputManager.mouseLDown)
        {
            selectedPlayerUnits.Clear();
            playerMarkerActive = true;
            int startX = (int)Mathf.Round(playerInputManager.leftClickPosition.x);
            int startZ = (int)Mathf.Round(playerInputManager.leftClickPosition.y);
            int endX = (int)Mathf.Round(playerInputManager.leftDownPosition.x);
            int endZ = (int)Mathf.Round(playerInputManager.leftDownPosition.y);
            float width = startX - endX;
            float height = startZ - endZ;

            // Scale Marker with mouse drag
            selectionMarker.transform.localScale = new(width, 0.01f, height);
            selectionMarker.transform.position = new(endX + width / 2, 0.1f, endZ + height / 2);
        }

        if (playerInputManager.mouseLUp && playerMarkerActive) // move off the board when mouse button up
        {
            SelectUnits();
            playerMarkerActive = false;
        }

        if (!playerMarkerActive) // Hide the selection Box
        {
            selectionMarker.transform.position = new(-2, 0, 0);
            selectionMarker.transform.localScale = new(1, 0.01F, 1);
        }

        //---------------------------------------------------------------
        // Red Marker for Target Setting

        if (playerInputManager.mouseRDown)
        {
            playerSetTargetPos = new(playerInputManager.rightClickPosition.x, 0, playerInputManager.rightClickPosition.y);

            //place marker gameobject on position where mouse was clicked
            if (redTargetMarker == null)
            {// if there is no marker already in the scene
                redTargetMarker = (GameObject)Instantiate(redMarkerPrefab, new(playerInputManager.rightClickPosition.x, -0.9F, playerInputManager.rightClickPosition.y), transform.rotation);
            }
            else
            {
                redTargetMarker.transform.position = new(playerInputManager.rightClickPosition.x, -0.9F, playerInputManager.rightClickPosition.y);
            }
        }


        SetSquad();

    }//-----

    //****************************************************************************
    // ************         Level Loader
    //****************************************************************************

    // ---------------------------------------------------------------------
    private void LoadLevel()
    {
        // Loop through all array slots - Spawn Objects bases on number
        for (int row = 0; row < levelData.level1.GetLength(0); row++)
        {
            for (int col = 0; col < levelData.level1.GetLength(1); col++)
            {
                int newX = col;
                int newZ = levelData.level1.GetLength(0) - 1 - row;

                // check if space is empty
                if (playArea[newZ, newX] == null)
                {
                    if (levelData.level1[row, col] == 1) // Obstacle
                    {
                        GameObject newObstacle = Instantiate(obstaclePrefab, new Vector3((float)newX, -0.5f, (float)newZ), transform.rotation);
                        newObstacle.transform.parent = objectParent.transform;
                        playArea[newZ, newX] = newObstacle;
                    }
                }
            }
        }
    }//----
    //****************************************************************************
    // ************           Add Objects
    //****************************************************************************

    // ---------------------------------------------------------------------
    private void AddTeams()
    {
        int newZ = -5;
        int newX = -5;
        int teamIndex = 0;

        foreach (GameObject team in teams)
        {
            GameObject newTeam = Instantiate(teams[teamIndex], new Vector3((float)newX, 0, (float)newZ), transform.rotation);

            newX = (int)Mathf.Round(newTeam.GetComponent<DD_Team>().teamPosition.x);
            newZ = (int)Mathf.Round(newTeam.GetComponent<DD_Team>().teamPosition.y);

            if (playArea[newZ, newX] == null)
            {
                // Add team object to array
                playArea[newZ, newX] = newTeam;
                newTeam.transform.position = new(newX, 0, newZ);
                newTeam.GetComponent<DD_Team>().teamID = teamIndex + 1;
            }
            activeTeams.Add(newTeam);

            if (newTeam.GetComponent<DD_Team>().playerControlledTeam) playerTeam = newTeam.GetComponent<DD_Team>();

            teamIndex++;
        }
    }//-----


    // ---------------------------------------------------------------------
    private bool CheckTeamsOccupyingSlot(int xPos, int zPos)
    {
        foreach (GameObject team in activeTeams)
        {
            int startPosX = (int)team.GetComponent<DD_Team>().teamPosition.x - teamClearDiameter / 2;
            int endPosX = (int)team.GetComponent<DD_Team>().teamPosition.x + teamClearDiameter / 2;
            int startPosZ = (int)team.GetComponent<DD_Team>().teamPosition.y - teamClearDiameter / 2;
            int endPosZ = (int)team.GetComponent<DD_Team>().teamPosition.y + teamClearDiameter / 2;

            if (xPos >= startPosX && xPos <= endPosX && zPos >= startPosZ && zPos <= endPosZ)
                return true;
        }
        return false;
    }//-----

    // ---------------------------------------------------------------------
    private void AddInitialResourceObjects(int resourceAmount)
    {
        for (int i = 0; i < resourceAmount; i++)
        {
            int newZ = Random.Range(0, playArea.GetLength(0));
            int newX = Random.Range(0, playArea.GetLength(1));

            if (CheckTeamsOccupyingSlot(newX, newZ)) continue;

            // check if space is empty
            if (playArea[newZ, newX] == null)
            {
                // Add object to array
                GameObject newResource = Instantiate(resourcePF, new Vector3((float)newX, -0.5f, (float)newZ), transform.rotation);

                playArea[newZ, newX] = newResource;

                // child to Objects to keep Hierachy Organised
                newResource.transform.parent = objectParent.transform;

                activeResources.Add(newResource);
            }
        }
    }//------   

    // ---------------------------------------------------------------------
    private void AddObstacles(int min, int max)
    {
        //Add a Random number of Box Objects
        int spawnAmount = Random.Range(min, max);

        for (int i = 0; i < spawnAmount; i++)
        {
            int newZ = Random.Range(0, playArea.GetLength(0));
            int newX = Random.Range(0, playArea.GetLength(1));

            if (CheckTeamsOccupyingSlot(newX, newZ)) continue;

            // check if space is empty
            if (playArea[newZ, newX] == null)
            {
                // Add block object to array
                playArea[newZ, newX] = (GameObject)Instantiate(obstaclePrefab, new Vector3((float)newX, -0.5f, (float)newZ), transform.rotation);

                // child to Objects to keep Hierachy Organised
                playArea[newZ, newX].transform.parent = objectParent.transform;
            }
        }
    }//------   

    // ---------------------------------------------------------------------
    private void AddRectange(int xStart, int zStart, int width, int height, bool isFilled)
    {
        if (xStart + width < playArea.GetLength(1) && zStart + height < playArea.GetLength(0))
        {
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    int newZ = zStart + row;
                    int newX = xStart + col;

                    if (isFilled)
                    {
                        // check if space is empty
                        if (playArea[newZ, newX] == null)
                        {
                            // Add block object to array
                            playArea[newZ, newX] = (GameObject)Instantiate(obstaclePrefab, new Vector3((float)newX, -0.5f, (float)newZ), transform.rotation);

                            // child to Objects to keep Hierachy Organised
                            playArea[newZ, newX].transform.parent = objectParent.transform;
                        }
                    }
                    else
                    {
                        if (row == 0 || row == height - 1 || col == 0 || col == width - 1)
                        {
                            // Add block object to array
                            playArea[newZ, newX] = (GameObject)Instantiate(obstaclePrefab, new Vector3((float)newX, -0.5f, (float)newZ), transform.rotation);

                            // child to Objects to keep Hierachy Organised
                            playArea[newZ, newX].transform.parent = objectParent.transform;
                        }
                    }
                }
            }
        }
        else
        {
            print("Square is off the board");
        }
    }//-----



    // ---------------------------------------------------------------------
    private void DisplayGameData()
    {
        // Left Hand Text Window
        LeftTextWindow.text = "Game States \n ==================\n";
        // Display Team Stats
        foreach (GameObject team in activeTeams)
        {
            DD_Team teamScript = team.GetComponent<DD_Team>();
            LeftTextWindow.text += "\n\nTeam: " + teamScript.teamName;
            LeftTextWindow.text += "\nUnits: " + teamScript.activeTeamMembers + " / " + teamScript.teamMembersTotal;
            LeftTextWindow.text += "\nResources: " + (int)teamScript.teamResources;
        }
    }//-----


    //win condition
    private void CheckVictoryCondition()
    {
        // victory condition: all enemy resources are depleted
        bool allEnemiesResourcesDepleted = true;
        foreach (var team in teams)
        {
            if (team != playerTeam.gameObject && team.GetComponent<DD_Team>().teamResources > 0)
            {
                allEnemiesResourcesDepleted = false;
                break;
            }
        }

        if (allEnemiesResourcesDepleted)
        {
            startGame.EndGame();
        }
    }
    public void PlayerUnitKilled()
    {
        playerUnitsKilled++;
        if (playerUnitsKilled >= 2)
        {
            startGame.EndGame();
        }
    }

}//==========