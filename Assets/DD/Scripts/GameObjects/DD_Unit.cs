// ----------------------------------------------------------------------
// --------------------  AI: Unit Object Class: 0.09
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Unit Enums
public enum States { idle, stepRandom, wander, chase, attack, flee, harvest, deposit }
public enum Heading { north, east, south, west }

public class DD_Unit : DD_BaseObject
{
    [Header("Unit State")]

    public DD_Team team;
    public int unitID = -1;
    public States unitState = States.idle;
    public bool isPlayerControlled = false;

    [Header("Movement Data ")]
    public Vector3 basePosition = Vector3.zero;
    public Vector3 targetPosition = Vector3.zero;
    public Vector3 currentPosition = Vector3.zero;
    public Vector3 startPosition = Vector3.zero;
    public Vector3 nextPosition = Vector3.zero;
    public float speed = 1;
    public bool isMoving = false;
    public bool obstacleAhead = false;
    public Heading unitHeading;
    public float stopDistance = 2;
    public float originalSpeed = 1;


    [Header("Resources")]
    public Vector3 nearestResourcePosition = new(-50, 50, -50);
    public bool isDepositing = false;
    public float resourceRange = 15;
    public float resourceCarrying = 0;
    public float resourceLimit = 10;
    public float resourceHarvestSpeed = 5;
    private GameObject nearestResource = null;


    [Header("Combat")]
    // public int ammo = 1000;
    public Vector3 nearestEnemyPosition = new(-50, 50, -50);
    private GameObject nearestEnemy = null;
    public float enemyChaseRange = 20;
    public float attackRange = 10;
    public float attackCoolDown = 1;
    public GameObject attackPF = null;
    private float nextAttackTime = 0;
    public List<int> friendsIDs = new();

    [HideInInspector]
    public DD_GameManager gameManager;
    public DD_AI_Class aiClass; // A* Pathfinding

    // Waypoint Movement Variables      
    public List<int> waypointsToTarget = new();
    public List<Vector3> pathToTarget = new(); // 用于存储 A* 生成的路径


    //private int pathIndex = 0;
    private BTNode behaviorTree;

    // ---------------------------------------------------------------------
    private void Start()
    {
        // The game manager will be use to access the game board
        if (!gameManager) gameManager = GameObject.Find("GameManager").GetComponent<DD_GameManager>();

        xPos = (int)transform.position.x; // Set Current Position 
        zPos = (int)transform.position.z;
        transform.position = new(xPos, 0, zPos);
        currentPosition = transform.position;

        // Set initial states
        unitHeading = (Heading)Random.Range(0, 4);
        unitState = States.wander;

        // Set Team ID for who  not to attack including this unit
        friendsIDs.Add(team.teamID);
        originalSpeed = speed;

        // Set up the Behavior Tree
        InitializeBehaviorTree();
    }//----

    // ---------------------------------------------------------------------
    private void FixedUpdate()
    {
        if (!isAlive) return;
        behaviorTree.Execute(this);
        UnitActions();
    }//---


    // ---------------------------------------------------------------------
    private void UnitActions()
    {
        if (unitState == States.idle) Idle();
        if (unitState == States.stepRandom) StepRandom();
        if (unitState == States.wander) Wander();
        if (unitState == States.chase) ChaseDirect(false);
        if (unitState == States.flee) ChaseDirect(true);
        if (unitState == States.harvest) HarvestResource();
        if (unitState == States.deposit) DepositResource();
        if (unitState == States.attack) AttackEnemy();
        MoveUnit();
    }//------



    //****************************************************************************
    // ************             NPC Unit State Manager  有限状态机FSM
    //****************************************************************************

    // ---------------------------------------------------------------------
    private void ManageStates()
    {
        if (isPlayerControlled) return; // ignore if player controlled
        if (isMoving) return;
        if (isDepositing) return;

        if (gameManager.activeResources.Count > 0) // Resources
        {
            FindNearestResource();
            if (Vector3.Distance(currentPosition, nearestResourcePosition) > resourceRange)
                unitState = States.wander;
            if (Vector3.Distance(currentPosition, nearestResourcePosition) <= resourceRange)
                unitState = States.harvest;
            if (resourceCarrying > resourceLimit - 0.2F)
                unitState = States.deposit;
        }
        else
        {
            unitState = States.wander;
        }

        if (gameManager.activeUnits.Count > 1)   // Combat
        {
            FindNearestEnemy();
            if (Vector3.Distance(nearestEnemyPosition, currentPosition) < enemyChaseRange)
                unitState = States.attack;
        }


        if (unitState != States.wander && obstacleAhead)   // is the path blocked and not wandering
        {
            unitState = States.stepRandom;
            obstacleAhead = false;
        }

    }//----


    // ---------------------------------------------------------------------
    // waypoint movement
    // ---------------------------------------------------------------------
    public void WaypointMoveToTarget(Vector3 pTargetPosition)
    {
        // stop if target is not on the play area
        if (pTargetPosition.x < 0 || pTargetPosition.z < 0) return;
        if (pTargetPosition.x >= gameManager.playArea.GetLength(1) || pTargetPosition.z >= gameManager.playArea.GetLength(0)) return;

        if (waypointsToTarget.Count == 0) // no current waypoint - find some        
            if (!gameManager.ai.CheckTargetInLineOfSight(currentPosition, pTargetPosition)) // Player Set Target is not in sight            
                waypointsToTarget = (gameManager.ai.GetListOfWaypointsToTarget(currentPosition, pTargetPosition));

        // Move to waypoints
        if (waypointsToTarget.Count > 0) // waypoints still exist
        {
            // Move to first position in List
            targetPosition = gameManager.ai.wayPointPositions[waypointsToTarget[0]];
            ChaseDirect(false);

            // Remove Waypoint from list when we reach it
            if (Vector3.Distance(currentPosition, targetPosition) < stopDistance)
                waypointsToTarget.RemoveAt(0);

            // goal in sight - clear list
            if (gameManager.ai.CheckTargetInLineOfSight(currentPosition, pTargetPosition))
                waypointsToTarget.Clear();
        }
        else // Move to target
        {
            targetPosition = pTargetPosition;
            if (Vector3.Distance(pTargetPosition, transform.position) > stopDistance)
                ChaseDirect(false);
        }

    }//---

    //A* Pathfinding



    // --------------------------------------------------------------------------------------------------
    //                 ****************   Combat  **************** 
    // --------------------------------------------------------------------------------------------------

    // ---------------------------------------------------------------------
    public void FindNearestEnemy()
    {
        // Use the list of active units in the Game Manager if any units are in it
        if (gameManager.activeUnits.Count > 0)
        {
            // nearest Enemy 
            GameObject closestEnemy = null;
            float distanceToEnemy = Mathf.Infinity; // reset values
            nearestEnemyPosition = new(-50, -50, -50);

            foreach (GameObject foundActiveUnit in gameManager.activeUnits) // Loop through list of units
            {
                bool friendFound = false;
                foreach (int friendID in friendsIDs) // ignore this unit, own team members and Friends
                    if (foundActiveUnit.GetComponent<DD_Unit>().team.teamID == friendID) friendFound = true;

                if (friendFound) continue; // skip forward to next unit in Acive List

                float currentNearestDistance = Vector3.Distance(foundActiveUnit.transform.position, currentPosition);
                if (currentNearestDistance < distanceToEnemy)
                {
                    closestEnemy = foundActiveUnit;
                    distanceToEnemy = currentNearestDistance;
                }
            }
            // Set Target of Enemy Unit
            if (closestEnemy != null)
            {
                nearestEnemyPosition = new((int)Mathf.Round(closestEnemy.transform.position.x), 0, (int)Mathf.Round(closestEnemy.transform.position.z));
                nearestEnemy = closestEnemy;
            }
        }
    }//-----

    // ---------------------------------------------------------------------
    public void AttackEnemy()
    {
        if (!isAlive) return;
        if (!nearestEnemy)
        {
            nearestEnemyPosition = new(-50, -50, -50); // out of range
            return; // no enemy found
        }

        if (nearestEnemyPosition.x < 0 || nearestEnemyPosition.z < 0) return; //  stop if postion off the play area

        //Enemy in Chase Range
        targetPosition = nearestEnemyPosition;
        if (Vector3.Distance(nearestEnemyPosition, currentPosition) < enemyChaseRange && Vector3.Distance(nearestEnemyPosition, currentPosition) > attackRange)
            ChaseDirect(false);

        else if (Vector3.Distance(nearestEnemyPosition, currentPosition) < attackRange)
            SendAttack();
    }//-----


    // ---------------------------------------------------------------------
    public void SendAttack()
    {
        if (!attackPF) return; // no bullet object referenced

        if (nextAttackTime < Time.time)
        {
            transform.LookAt(nearestEnemyPosition);
            GameObject unitAttack = Instantiate(attackPF, gameObject.transform);
            unitAttack.transform.SetParent(null);
            unitAttack.GetComponent<DD_Attack>().teamID = team.teamID;
            nextAttackTime = Time.time + attackCoolDown;
        }
    }//-----


    // --------------------------------------------------------------------------------------------------
    //                 ****************   RESOURCES   **************** 
    // --------------------------------------------------------------------------------------------------


    // ---------------------------------------------------------------------
    public void FindNearestResource()
    {
        nearestResourcePosition = new(-50, -50, -50); // set to out of range
        nearestResource = null;

        if (gameManager.activeResources.Count > 0)// Are there some resources in the level
        {
            // Find the nearest
            GameObject closestResource = null;
            float distanceToResource = Mathf.Infinity;

            foreach (GameObject resource in gameManager.activeResources) // Loop though list of resources
            {
                float currentNearestDistance = Vector3.Distance(resource.transform.position, currentPosition);

                if (currentNearestDistance < distanceToResource)
                {
                    closestResource = resource;
                    distanceToResource = currentNearestDistance;
                }
            }
            // Set Target of resourse
            if (closestResource != null)
            {
                nearestResourcePosition = new((int)Mathf.Round(closestResource.transform.position.x), 0, (int)Mathf.Round(closestResource.transform.position.z));
                nearestResource = closestResource;
            }
        }
    }//----


    // ---------------------------------------------------------------------
    private void HarvestResource()
    {
        if (!nearestResource) // no resource exists
        {
            nearestResourcePosition = new(-50, -50, -50); // set to out of range    
            return; // no nearest resource found
        }

        if (Vector3.Distance(currentPosition, nearestResourcePosition) > stopDistance)
        {
            targetPosition = nearestResourcePosition;
            ChaseDirect(false);    // Move to resource
        }
        else if (nearestResource.GetComponent<DD_Resource>().resourceHeld > resourceHarvestSpeed * Time.deltaTime)
        {
            nearestResource.GetComponent<DD_Resource>().GetResource(resourceHarvestSpeed * Time.deltaTime);
            resourceCarrying += resourceHarvestSpeed * Time.deltaTime;
        }
    }//-----

    // ---------------------------------------------------------------------
    private void DepositResource()
    {
        // Move home
        if (Vector3.Distance(basePosition, currentPosition) > stopDistance && !isDepositing)
        {
            WaypointMoveToTarget(basePosition);
        }
        else // Unit is Close to home
        {
            // Stop depositing when unit resource almost empty
            if (resourceCarrying > 0 && resourceCarrying < 0.2F)
                isDepositing = false;
            else
                isDepositing = true;

            if (resourceCarrying > resourceHarvestSpeed * Time.deltaTime)
            {
                team.DepositResource(resourceHarvestSpeed * Time.deltaTime);
                resourceCarrying -= (resourceHarvestSpeed * Time.deltaTime);
            }
        }
    }//-----



    // ---------------------------------------------------------------------
    //                 ****************   Movement    **************** 
    //                 ****************               **************** 

    // ---------------------------------------------------------------------
    public void ChaseDirect(bool reverse)
    {
        if (!isMoving) // Set a new direction if unit is not moving
        {
            // Find
            // Straight Line to target  ---------------------------
            float dx = (targetPosition.x - currentPosition.x);
            float dz = (targetPosition.z - currentPosition.z);
            float angle = Mathf.Atan2(dx, dz);

            // use Trig to work out which slot is closest to a straight line to target
            if (!reverse)
            {
                if (Mathf.Abs(dx) > 0.1F) nextPosition.x = currentPosition.x + Mathf.Round(1.4F * Mathf.Sin(angle));
                if (Mathf.Abs(dz) > 0.1F) nextPosition.z = currentPosition.z + Mathf.Round(1.4F * Mathf.Cos(angle));
            }
            else
            {
                if (Mathf.Abs(dx) > 0.1F) nextPosition.x = currentPosition.x - Mathf.Round(1.4F * Mathf.Sin(angle));
                if (Mathf.Abs(dz) > 0.1F) nextPosition.z = currentPosition.z - Mathf.Round(1.4F * Mathf.Cos(angle));
            }

            // Round off next Pos
            nextPosition = new Vector3((int)nextPosition.x, 0, (int)nextPosition.z);
            int newX = (int)nextPosition.x;
            int newZ = (int)nextPosition.z;

            // Check if the new postion is on the board and free
            if (CheckNewPositionisFree(newX, newZ)) SetNewPosition(newX, newZ);
        }
    }//---


    // ---------------------------------------------------------------------
    private void Wander()
    {
        if (!isMoving)  // Only Set a new Next Position if the unit has finished moving slot
        {
            if (obstacleAhead) // path blocked - find a new heading
            {
                unitHeading = (Heading)Random.Range(0, 4); // pick a new direction from the heading enum
                isMoving = true;
                obstacleAhead = false;
            }
            else
            {
                // Find Next Postion based on heading
                if (unitHeading == Heading.north) nextPosition = new Vector3(currentPosition.x, 0, currentPosition.z + 1);
                if (unitHeading == Heading.south) nextPosition = new Vector3(currentPosition.x, 0, currentPosition.z - 1);
                if (unitHeading == Heading.east) nextPosition = new Vector3(currentPosition.x + 1, 0, currentPosition.z);
                if (unitHeading == Heading.west) nextPosition = new Vector3(currentPosition.x - 1, 0, currentPosition.z);

                // Round off next Pos
                nextPosition = new Vector3((int)nextPosition.x, 0, (int)nextPosition.z);
                int newX = (int)nextPosition.x;
                int newZ = (int)nextPosition.z;

                // Check if the new postion is on the board and free and Set it in the Array
                if (CheckNewPositionisFree(newX, newZ)) SetNewPosition(newX, newZ);
            }
        }
    }//-----


    // ---------------------------------------------------------------------
    private void StepRandom() // take one step in a random direction
    {

        if (!isMoving) // Only Set a new Next Position if the unit has finished moving slot
        {
            // generate random adjacent adjacent slot to move to     
            int direction = (int)UnityEngine.Random.Range(0, 4);
            if (direction == 0) nextPosition = new Vector3(currentPosition.x - 1, 0, currentPosition.z);
            if (direction == 1) nextPosition = new Vector3(currentPosition.x + 1, 0, currentPosition.z);
            if (direction == 2) nextPosition = new Vector3(currentPosition.x, 0, currentPosition.z - 1);
            if (direction == 3) nextPosition = new Vector3(currentPosition.x, 0, currentPosition.z + 1);

            // Round off next Pos
            nextPosition = new Vector3((int)nextPosition.x, 0, (int)nextPosition.z);
            int newX = (int)nextPosition.x;
            int newZ = (int)nextPosition.z;

            // Check if the new postion is on the board and free and Set it in the Array
            if (CheckNewPositionisFree(newX, newZ)) SetNewPosition(newX, newZ);
        }
    }//-----

    // ---------------------------------------------------------------------
    private void Idle()
    {
        // wait for some time then change state
    }//-----



    //=============================================================================================
    // Common Movement Methods

    // ---------------------------------------------------------------------
    private bool CheckNewPositionisFree(int newXPos, int newZPos)
    {
        if (newXPos >= 0 && newXPos < gameManager.playArea.GetLength(1) && newZPos >= 0 && newZPos < gameManager.playArea.GetLength(0))
        {
            if (gameManager.playArea[newZPos, newXPos] == null)
            {
                obstacleAhead = false;
                return true;
            }
            else
            {
                obstacleAhead = true;
                return false;
            }
        }
        obstacleAhead = true;
        return false;
    }//---


    // ---------------------------------------------------------------------
    private void SetNewPosition(int newXPos, int newZPos)
    {
        if (gameManager.playArea[newZPos, newXPos] == null) // Array slot is empty
        {
            gameManager.playArea[zPos, xPos] = null; // clear old slot in the Array
            gameManager.playArea[newZPos, newXPos] = gameObject; //set new slot                 
            xPos = newXPos;
            zPos = newZPos;
            currentPosition = nextPosition;
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }//----

    // ---------------------------------------------------------------------
    private void MoveUnit() // This updates / animates the Avatar position on screen
    {
        if (isMoving)
        {
            // check distance to new current position
            Vector2 nextMovePosition = new(currentPosition.x, currentPosition.z);
            Vector2 curentRealPos = new(transform.position.x, transform.position.z);

            if (Vector2.Distance(nextMovePosition, curentRealPos) > 0.1F) // more than 10cm from slot centre
            {
                transform.LookAt(currentPosition);
                transform.Translate(0, 0, speed * Time.deltaTime);
            }
            else
            {
                isMoving = false; // stop moving            
            }
        }
    }//----

    //Behaviro Tree
    private void InitializeBehaviorTree()
    {
        var root = new Selector();

        var combatSequence = new Sequence();
        combatSequence.AddChild(new ConditionNode(unit => unit.nearestEnemy != null && Vector3.Distance(unit.currentPosition, unit.nearestEnemyPosition) < unit.enemyChaseRange));
        combatSequence.AddChild(new ActionNode(unit => unit.unitState = States.attack));

        var resourceSequence = new Sequence();
        resourceSequence.AddChild(new ConditionNode(unit => unit.gameManager.activeResources.Count > 0));
        resourceSequence.AddChild(new ActionNode(unit => unit.FindNearestResource()));
        resourceSequence.AddChild(new ConditionNode(unit => Vector3.Distance(unit.currentPosition, unit.nearestResourcePosition) <= unit.resourceRange));
        resourceSequence.AddChild(new ActionNode(unit => unit.unitState = States.harvest));

        var wanderAction = new ActionNode(unit => unit.unitState = States.wander);

        root.AddChild(combatSequence);
        root.AddChild(resourceSequence);
        root.AddChild(wanderAction);

        behaviorTree = root;
    }

}//==========


public abstract class BTNode
{
    public abstract bool Execute(DD_Unit unit);
}

public class Selector : BTNode
{
    private List<BTNode> children = new List<BTNode>();

    public void AddChild(BTNode child)
    {
        children.Add(child);
    }

    public override bool Execute(DD_Unit unit)
    {
        foreach (var child in children)
        {
            if (child.Execute(unit))
            {
                return true;
            }
        }
        return false;
    }
}

public class Sequence : BTNode
{
    private List<BTNode> children = new List<BTNode>();

    public void AddChild(BTNode child)
    {
        children.Add(child);
    }

    public override bool Execute(DD_Unit unit)
    {
        foreach (var child in children)
        {
            if (!child.Execute(unit))
            {
                return false;
            }
        }
        return true;
    }
}

public class ConditionNode : BTNode
{
    private System.Func<DD_Unit, bool> condition;

    public ConditionNode(System.Func<DD_Unit, bool> condition)
    {
        this.condition = condition;
    }

    public override bool Execute(DD_Unit unit)
    {
        return condition(unit);
    }
}

public class ActionNode : BTNode
{
    private System.Action<DD_Unit> action;

    public ActionNode(System.Action<DD_Unit> action)
    {
        this.action = action;
    }

    public override bool Execute(DD_Unit unit)
    {
        action(unit);
        return unit.unitState != States.idle; // 只有状态被修改时才返回 true
    }
}

