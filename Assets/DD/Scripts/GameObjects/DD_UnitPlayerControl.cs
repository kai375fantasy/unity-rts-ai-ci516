// ----------------------------------------------------------------------
// --------------------  AI: Unit Player Controlled 0.09
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DD_UnitPlayerControl : MonoBehaviour
{
    // ---------------------------------------------------------------------
    private DD_Unit unitScript = null;
    public DD_Team parentTeam;
    private GameObject highlight;
    public bool isSelected = false;

    // squad
    [Header("Squad Stuff")]
    public int squadID = -1;
    public bool isSquadLeader;
    public GameObject squadLeader;
    public bool useArrayFormation = true;

    // ---------------------------------------------------------------------
    private void Start()
    {
        // The game manager will be use to access the game board
        parentTeam = GetComponentInParent<DD_Team>();
        unitScript = GetComponent<DD_Unit>();     // Unit Control script Reference
        CreateHighlight(); // for showing selected
    }//-----


    // ---------------------------------------------------------------------
    private void FixedUpdate()
    {
        if (!unitScript.isAlive) return;
        if (unitScript.isPlayerControlled == false) return;
        UpdateHighlight();
        StateManagerPlayerControl();
        ManageSquad();
    }//-----  




    // ---------------------------------------------------------------------
    private void StateManagerPlayerControl()
    {
        if (unitScript.isMoving) return;
        if (unitScript.isDepositing) return;

        if (squadID > 1) // part of squad but not leader
        {
            SetSquadPosition();
            if (unitScript.speed == unitScript.originalSpeed) unitScript.speed *= 1.2F;
            return;
        }
        if (unitScript.speed != unitScript.originalSpeed) unitScript.speed = unitScript.originalSpeed;



        if (isSelected) // Move to Player Set Target
        {
            unitScript.targetPosition = unitScript.gameManager.playerSetTargetPos;

            if (Vector3.Distance(unitScript.currentPosition, unitScript.targetPosition) < unitScript.stopDistance) unitScript.unitState = States.idle;
           else
            unitScript.WaypointMoveToTarget(unitScript.targetPosition);                   
        }
        else // Wander, gather and fight when not selected
        {
            unitScript.unitState = States.wander;          

            // Other beahavior here
        }

        // is the path blocked and not wandering
        if (unitScript.unitState != States.wander && unitScript.obstacleAhead)
        {
            unitScript.unitState = States.stepRandom;
            unitScript.obstacleAhead = false;
        }

    }//-----






    // ---------------------------------------------------------------------
    //   A Star

    private List<Vector3> pathToTarget = new();

    private void AStarMoveToTarget(Vector3 pTargetPos)
    {
        if (pTargetPos.x < 0 || pTargetPos.z < 0) return; // target off board
        if (unitScript.gameManager.playArea[(int)pTargetPos.z, (int)pTargetPos.x]) return; // slot occupied

        if (unitScript.gameManager.ai.CheckTargetInLineOfSight(unitScript.currentPosition, pTargetPos))
        { // Target in sight, chase
            unitScript.targetPosition = pTargetPos;

            if (Vector3.Distance(pTargetPos, transform.position) < unitScript.stopDistance)
                unitScript.unitState = States.idle;
            else
                unitScript.ChaseDirect(false);

            pathToTarget.Clear();
        }
        else
        {
            if (pathToTarget.Count > 0)
            {                
                unitScript.targetPosition = pathToTarget[0];

                if (Vector3.Distance(pathToTarget[0], transform.position) < unitScript.stopDistance)
                    pathToTarget.RemoveAt(0);


                if (Vector3.Distance(pTargetPos, transform.position) < 0.3f) // could use stop distance
                {
                    unitScript.unitState = States.idle;
                    pathToTarget.Clear();
                }
                else
                {
                    unitScript.ChaseDirect(false);
                }
            }
            else
            {
                pathToTarget = unitScript.gameManager.ai.Pathfind(unitScript.currentPosition, pTargetPos);
            }
        }

    }//---
     // ---------------------------------------------------------------------






    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------
    private void ManageSquad()
    {    
        if (unitScript.isMoving) return;

        if (!parentTeam.squadActive)
        {
            isSquadLeader = false;
        }
        else
        {// check pos in squad
            squadID = -1;
            int index = 1;
            foreach (GameObject unit in parentTeam.squad)
            {
                if (index == 1) squadLeader = unit;

                if (unit == this.gameObject)
                {
                    squadID = index;
                }
                index++;
            }
        }
        if (squadID == 1) isSquadLeader = true;
        else isSquadLeader = false;

        if (squadID < 1) squadLeader = null;
    }//-----




    // ---------------------------------------------------------------------
    private void SetSquadPosition()
    {
        int xOffset = 0, zOffset = 0;
        if (useArrayFormation)
        {
            int leaderOffsetX = 0, leaderOffsetZ = 0;

            for (int row = 0; row < parentTeam.squadShape.GetLength(0); row++)
            {
                for (int col = 0; col < parentTeam.squadShape.GetLength(1); col++)
                {
                    if (parentTeam.squadShape[row, col] == 1)
                    {
                        leaderOffsetX = col;
                        leaderOffsetZ = row;
                    }
                    if (parentTeam.squadShape[row, col] == squadID)
                    {
                        xOffset = col;
                        zOffset = row;
                    }
                }
            }
            xOffset -= leaderOffsetX;
            zOffset -= leaderOffsetZ;
        }
        else// use Algorithm 
        {
            xOffset = 0;
            zOffset = squadID;
        }

        unitScript.targetPosition = new(squadLeader.transform.position.x + xOffset, 0, squadLeader.transform.position.z + zOffset);

        // Move to squad Postion
        if (Vector3.Distance(unitScript.targetPosition, unitScript.currentPosition) > unitScript.stopDistance)
            unitScript.ChaseDirect(false); 
    }//-----




    // ---------------------------------------------------------------------
    // BehaviourTreeStuff

    private bool IsResourceNear()
    { return true; }
    private bool HarvestResource()
    { return true; }
    private bool DepositResource()
    { return true; }


    // ---------------------------------------------------------------------
    private void UpdateHighlight()
    {
        if (isSelected) highlight.SetActive(true);
        else highlight.SetActive(false);
    }//-----

    // ---------------------------------------------------------------------
    private void CreateHighlight()
    {
        highlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlight.transform.position = new(transform.position.x, 0.7f, transform.position.z);
        highlight.transform.localScale = new(0.95f, 0.01f, 0.95f);
        highlight.transform.SetParent(transform);
        highlight.SetActive(false);
    }//----
}
