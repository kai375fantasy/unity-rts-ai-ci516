// ----------------------------------------------------------------------
// --------------------  AI: PC Team - 0.10
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class DD_TeamPlayerControl : MonoBehaviour
{

    // ---------------------------------------------------------------------
    [Header("Units")]
    public float unitCreateTime = 5;
    public float unitSpawnTime = 0;
    [Header("Buildings")]
    public GameObject building1PF = null;
    public GameObject building2PF = null;
    public GameObject building3PF = null;
    public float building1CreateTime = 10;
    public float building2CreateTime = 5;
    public float building3CreateTime = 20;
    public float building1SpawnTime = 0;
    public float building2SpawnTime = 0;
    public float building3SpawnTime = 0;


    public DD_Team teamScript;
    // ---------------------------------------------------------------------

    void Start()
    {
        teamScript = GetComponent<DD_Team>();

    }

    // ---------------------------------------------------------------------

    void Update()
    {
        CheckActionButton();

    }


    // ---------------------------------------------------------------------
    private void FixedUpdate()
    {
        UpdateSliders();
    }



    // ---------------------------------------------------------------------
    private void UpdateSliders()
    {
        // Set the sliders position based on how long to next spawn time

        float sliderValue;

        // Slider 1 Units
        if (unitSpawnTime > Time.time)
            sliderValue = 1 - (unitSpawnTime - Time.time) / unitCreateTime;
        else
            sliderValue = 1;
        teamScript.gameManager.playerInputManager.slider1.value = sliderValue;


        // Slider 2 Blocks
        if (building1SpawnTime > Time.time)
            sliderValue = 1 - (building1SpawnTime - Time.time) / building1CreateTime;
        else
            sliderValue = 1;
        teamScript.gameManager.playerInputManager.slider2.value = sliderValue;


        // Slider 3 Turret
        if (building2SpawnTime > Time.time)
            sliderValue = 1 - (building2SpawnTime - Time.time) / building2CreateTime;
        else
            sliderValue = 1;
        teamScript.gameManager.playerInputManager.slider3.value = sliderValue;


        // Slider 3 Turret
        if (building3SpawnTime > Time.time)
            sliderValue = 1 - (building3SpawnTime - Time.time) / building3CreateTime;
        else
            sliderValue = 1;
        teamScript.gameManager.playerInputManager.slider4.value = sliderValue;

    }//-----





    // ---------------------------------------------------------------------
    private void CheckActionButton()
    {

        if (teamScript.gameManager.playerInputManager.button1Activated)
        {
            ActivateUnit();
            teamScript.gameManager.playerInputManager.button1Activated = false;
        }

        if (teamScript.gameManager.playerInputManager.button2Activated)
        {
            CreateBuilding(1);
            teamScript.gameManager.playerInputManager.button2Activated = false;
        }

        if (teamScript.gameManager.playerInputManager.button3Activated)
        {
            CreateBuilding(2);
            teamScript.gameManager.playerInputManager.button3Activated = false;
        }

        if (teamScript.gameManager.playerInputManager.button4Activated)
        {
            CreateBuilding(3);
            teamScript.gameManager.playerInputManager.button4Activated = false;
        }
    }




    // ---------------------------------------------------------------------
    private void CreateBuilding(int type)
    {
        // check spawn position is on the board
        if (teamScript.gameManager.playerInputManager.rightClickPosition.x < 0 || teamScript.gameManager.playerInputManager.rightClickPosition.y < 0) return;

        GameObject newBuilding = null;

        if (type == 1)
        {
            if (building1SpawnTime > Time.time) return;
            newBuilding = Instantiate(building1PF, teamScript.teamPosition, Quaternion.identity);
        }

        if (type == 2)
        {
            if (building2SpawnTime > Time.time) return;
            newBuilding = Instantiate(building2PF, teamScript.teamPosition, Quaternion.identity);
        }

        if (type == 3)
        {
            if (building3SpawnTime > Time.time) return;
            newBuilding = Instantiate(building3PF, teamScript.teamPosition, Quaternion.identity);
        }

        if (!newBuilding) return;

        DD_Building newBuildingScript = newBuilding.GetComponent<DD_Building>();

        // Does the team have enough resources
        if (teamScript.teamResources < newBuildingScript.cost)
        { Destroy(newBuilding); return; }

        int newX = (int)teamScript.gameManager.playerInputManager.rightClickPosition.x;
        int newZ = (int)teamScript.gameManager.playerInputManager.rightClickPosition.y;

        // Check play area is clear
        if (!CheckAreaIsClear(newX, newZ, (int)newBuildingScript.buildingSize.x, (int)newBuildingScript.buildingSize.y))
        { Destroy(newBuilding); return; }

        // Place the building
        newBuilding.transform.position = new(newX, 0, newZ);

        //teamScript.gameManager.playArea[newZ, newX] = newBuilding;
        SetBuildingArea(newBuilding, newX, newZ, (int)newBuildingScript.buildingSize.x, (int)newBuildingScript.buildingSize.y);

        teamScript.teamResources -= newBuildingScript.cost;


        if (type == 1) building1SpawnTime = Time.time + building1CreateTime;
        if (type == 2) building2SpawnTime = Time.time + building2CreateTime;
        if (type == 3) building3SpawnTime = Time.time + building3CreateTime;


    }//-----



    private void SetBuildingArea(GameObject newObject, int xStart, int zStart, int width, int depth)
    {
        for (int row = 0; row < width; row++)
        {
            for (int col = 0; col < depth; col++)
            {
                teamScript.gameManager.playArea[zStart + row, xStart + col] = newObject;
            }
        }

    }//----


    // ---------------------------------------------------------------------
    private bool CheckAreaIsClear(int xStart, int zStart, int width, int depth)
    {
        for (int row = 0; row < width; row++)
        {
            for (int col = 0; col < depth; col++)
            {
                if (teamScript.gameManager.playArea[zStart + row, xStart + col] != null)
                {
                    return false;
                }
            }
        }

        // resizeMarker
        return true;
    }//-----





    // ---------------------------------------------------------------------
    private void ActivateUnit()
    {
        // stop if resources insufficient
        if (teamScript.teamResources < teamScript.unitCost) return;
        if (unitSpawnTime > Time.time) return;

        GameObject newUnit = null;
        DD_Unit unitScript;

        // Find first Available Unit 
        foreach (GameObject unit in teamScript.teamUnits)
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
        int newX = (int)Mathf.Round(teamScript.teamPosition.x);
        int newZ = (int)Mathf.Round(teamScript.teamPosition.y); // y is z in this case as V2

        if (teamScript.spawnDirection == Heading.east) newX += teamScript.unitSpawnDistance;
        if (teamScript.spawnDirection == Heading.west) newX -= teamScript.unitSpawnDistance;
        if (teamScript.spawnDirection == Heading.north) newZ += teamScript.unitSpawnDistance;
        if (teamScript.spawnDirection == Heading.south) newZ -= teamScript.unitSpawnDistance;

        if (teamScript.gameManager.playArea[newZ, newX] == null)
        {
            // Add unit to array
            teamScript.gameManager.playArea[newZ, newX] = newUnit;
            newUnit.transform.position = new(newX, 0, newZ); // position unit

            // Active unit and add to the totals
            unitScript = newUnit.GetComponent<DD_Unit>();
            unitScript.currentPosition = new(newX, 0, newZ); // reset current position
            unitScript.xPos = newX;
            unitScript.zPos = newZ;
            unitScript.isAlive = true;
            unitScript.health = newUnit.GetComponent<DD_UnitHealth>().maxHealth;
            teamScript.activeTeamMembers++;
            teamScript.gameManager.activeUnits.Add(newUnit);

            // reduce resesource & set next spawn time
            teamScript.teamResources -= teamScript.unitCost;
        }

        unitSpawnTime = Time.time + unitCreateTime;
    }//----






}//===========
