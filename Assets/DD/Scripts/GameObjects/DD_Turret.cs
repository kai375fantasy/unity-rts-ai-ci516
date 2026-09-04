
using System.Collections.Generic;
using UnityEngine;

public class DD_Turret : MonoBehaviour
{
    public DD_GameManager gameManager;
    public Vector3 nearestEnemyPosition = new(-50, 50, -50);
    private GameObject nearestEnemy = null;

    public float attackRange = 10;
    public float attackCoolDown = 1;
    public GameObject attackPF = null;
    private float nextAttackTime = 0;

    public List<int> friendsIDs = new();
    public Vector3 currentPosition;
    public int teamID = 1;


    // Start is called before the first frame update
    void Start()
    {
        // The game manager will be use to access the game board
        gameManager = GameObject.Find("GameManager").GetComponent<DD_GameManager>();
        friendsIDs.Add(teamID);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        FindNearestEnemy();
        AttackEnemy();
    }

    //                   ****************   COMBAT ***************************
    // ---------------------------------------------------------------------
    public void AttackEnemy()
    {
        if (!nearestEnemy)
        {
            nearestEnemyPosition = new(-50, -50, -50); // out of range
            return; // no enemy found
        }
        //Enemy in attack
        if (Vector3.Distance(nearestEnemyPosition, currentPosition) < attackRange) // in Attack range
        {
            SendAttack();
        }
    }//-----


    private void SendAttack()
    {
        if (!attackPF) return; // no bullet object referenced

        if (nextAttackTime < Time.time)
        {
            transform.LookAt(nearestEnemyPosition);
            GameObject unitAttack = Instantiate(attackPF, gameObject.transform);
            unitAttack.transform.SetParent(null);
            unitAttack.GetComponent<DD_Attack>().teamID = teamID;
            nextAttackTime = Time.time + attackCoolDown;
        }
    }//-----


    // ---------------------------------------------------------------------
    private void FindNearestEnemy()
    {
        // Use the list of active resoures in the Game Manager if any units are in it
        if (gameManager.activeUnits.Count > 0)
        {
            currentPosition = transform.position;

            // nearest Enemy 
            GameObject closestEnemy = null;
            float distanceToEnemy = Mathf.Infinity;

            foreach (GameObject foundActiveUnit in gameManager.activeUnits) // Loop through list of units
            {
                bool friendFound = false;
                // ignore own team members and Friends
                foreach (int friendID in friendsIDs)
                {
                    if (foundActiveUnit.GetComponent<DD_Unit>().team.teamID == friendID) friendFound = true;
                }

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
            else
            {
                nearestEnemyPosition = new(-100, -100, -100);
            }
        }
    }//-----
}
