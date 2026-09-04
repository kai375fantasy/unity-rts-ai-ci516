// ----------------------------------------------------------------------
// --------------------  AI: Team Class 0.09
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class DD_Team : MonoBehaviour
{
    // ---------------------------------------------------------------------

    [Header("Team Stats")]
    public bool playerControlledTeam = false;
    public Color teamColour = Color.blue;
    public int teamID = 0;
    public string teamName = "Team X";
    public int teamMembersTotal = 20;
    public int activeTeamMembers = 0;
    public float teamResources = 100;
    public Vector2 teamPosition = Vector2.zero;
    public Heading spawnDirection = Heading.east;
    public int unitSpawnDistance = 3;

    [Header("Team Units")]
    public GameObject unit = null;
    public float unitCost = 5;
    public float spawnCoolDownTime = 5.0f;
    private float nextSpawnTime = 0.0f;
    public List<GameObject> teamUnits = new();
    public DD_GameManager gameManager;

    [Header("Squad Stuff")]
    public List<GameObject> squad = new();
    public bool squadActive = false;


    // ---------------------------------------------------------------------
    private void Start()
    {
        // The game manager will be use to access the game board
        gameManager = GameObject.Find("GameManager").GetComponent<DD_GameManager>();
        gameObject.GetComponent<Renderer>().material.color = teamColour;
        CreateUnits();
    }//-----

    // ---------------------------------------------------------------------
    private void FixedUpdate()
    {
        if (playerControlledTeam) return;
        SpawnUnit();
    }//-----


    // Update is called once per frame
    void Update()
    {
        SetSquadFormation();
    }//---



    // ---------------------------------------------------------------------
    private void SelectLeader()
    {
        if (playerControlledTeam && squadActive)
            if (Input.GetKeyDown(KeyCode.L))
                squad[0].GetComponent<DD_UnitPlayerControl>().isSelected = true;

    }//---

    // ===========================================================================
    public int[,] squadShape =  {   { 0, 0, 0, 0, 0, 0, 0 },
                                    { 0, 2, 17, 6, 16, 3, 0 },
                                    { 0, 14, 0, 0, 0, 15, 0 },
                                    { 0, 8, 0, 1, 0, 7, 0 },
                                    { 0, 13, 0, 0, 0, 12, 0 },
                                    { 0, 4, 10, 9, 11, 5, 0 },
                                    { 0, 0, 0, 0, 0, 0, 0 }
    };


    // ---------------------------------------------------
    void SetSquadFormation()
    {
        SelectLeader();

        if (Input.GetKeyDown(KeyCode.Y))
        {
            print("Setting Formation 1");
            squadShape = new int[7, 7]
            {
                { 1, 0, 2, 0, 3, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0 },
                { 4, 0, 5, 0, 6, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0 },
                { 7, 0, 8, 0, 9, 0, 0 }
            };
        }
        // Formation 2  --------------------	
        if (Input.GetKeyDown(KeyCode.U))
        {
            print("Setting Formation 2");
            squadShape = new int[7, 7]
            {
                { 0, 0, 1, 0, 0 , 0, 0 },
                { 0, 0, 0, 0, 0 , 0, 0 },
                { 0, 2, 0, 3, 0 , 0, 0 },
                { 0, 0, 6, 0, 0 , 0, 0 },
                { 4, 7, 0, 8, 5 , 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0 }
            };
        }
        // Formation 3	--------------------			
        if (Input.GetKeyDown(KeyCode.I))
        {
            print("Setting Formation 3");
            squadShape = new int[5, 5]
            {
                { 5, 2, 1, 3, 4 },
                { 6, 7, 8, 9, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 }
            };
        }
        // Formation 4	--------------------			
        if (Input.GetKeyDown(KeyCode.O))
        {
            print("Setting Formation 4");
            squadShape = new int[5, 5]
            {
                { 2, 0, 0, 0, 3 },
                { 0, 5, 0, 6, 0 },
                { 0, 0, 1, 0, 0 },
                { 0, 7, 0, 8, 0 },
                { 4, 0, 0, 0, 5 }
            };
        }


        // Formation 5  --------------------	
        if (Input.GetKeyDown(KeyCode.P))
        {
            print("Setting Formation 5");
            squadShape = new int[7, 7]
            {
                { 1, 0, 0, 0, 0 , 0, 0 },
                { 0, 2, 0, 0, 0 , 0, 0 },
                { 0, 8, 3, 0, 0 , 0, 0 },
                { 0, 0, 9, 4, 0 , 0, 0 },
                { 0, 0, 0, 10, 5 , 0, 0 },
                { 0, 0, 0, 0, 11 , 6, 0 },
                { 0, 0, 0, 0, 0 , 12, 7 }
            };
        }

    }//-----







    // ---------------------------------------------------------------------
    private void SpawnUnit()
    {
        if (nextSpawnTime > Time.time) return;    // wait until coolDown over   
        if (teamResources < unitCost) return;  // Stop if no resources are available

        //  print("resources available to create new life");
        GameObject newUnit = null;
        DD_Unit unitScript;

        // Find first Available Unit 
        foreach (GameObject unit in teamUnits)
        {
            unitScript = unit.GetComponent<DD_Unit>();
            if (unitScript.isAlive == false)
            {
                newUnit = unit;
                break; // stop searching for inactive units
            }
        }

        if (!newUnit) return; // exit as no available units 

        // Unit found so Set unit Position and add to playArea Array
        int newX = (int)Mathf.Round(teamPosition.x);
        int newZ = (int)Mathf.Round(teamPosition.y); // y is z in this case as V2

        if (spawnDirection == Heading.east) newX += unitSpawnDistance;
        if (spawnDirection == Heading.west) newX -= unitSpawnDistance;
        if (spawnDirection == Heading.north) newZ += unitSpawnDistance;
        if (spawnDirection == Heading.south) newZ -= unitSpawnDistance;

        if (gameManager.playArea[newZ, newX] == null)
        {
            // Add resource object to array
            gameManager.playArea[newZ, newX] = newUnit;
            newUnit.transform.position = new(newX, 0, newZ); // position unit

            // Active unit and add to the totals
            unitScript = newUnit.GetComponent<DD_Unit>();
            unitScript.currentPosition = new(newX, 0, newZ); // reset current position
            unitScript.xPos = newX;
            unitScript.zPos = newZ;
            unitScript.isAlive = true;
            unitScript.currentPosition = new(newX, 0, newZ);

            activeTeamMembers++;
            gameManager.activeUnits.Add(newUnit);

            // reduce resesource & set next spawn time
            teamResources -= unitCost;
        }
        nextSpawnTime = Time.time + spawnCoolDownTime;
    }//-----



    // ---------------------------------------------------------------------
    private void CreateUnits()
    {
        for (int i = 0; i < teamMembersTotal; i++)
        {
            int newX = i;
            int newZ = -3 - teamID; // position inactive units off the board in rows of teams

            GameObject newUnit = Instantiate(unit, new Vector3((float)newX, -0.5f, (float)newZ), transform.rotation);
            newUnit.transform.SetParent(transform); // keep the hierarchy tidy
            newUnit.GetComponent<Renderer>().material.color = teamColour;
            DD_Unit unitScript = newUnit.GetComponent<DD_Unit>();
            unitScript.unitID = i;
            unitScript.basePosition = new(teamPosition.x, 0, teamPosition.y); // tell each unit where home is
            unitScript.team = GetComponent<DD_Team>();

            if (playerControlledTeam) unitScript.isPlayerControlled = true; // If team is PC

            // Add to list of units
            teamUnits.Add(newUnit);
        }
    }//----

    // ---------------------------------------------------------------------
    public void DepositResource(float DepositRate) // receiver for unit resources
    {
        teamResources += DepositRate;
    }//-----

}//==========
