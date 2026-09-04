// ----------------------------------------------------------------------
// --------------------  AI: Attack 0.06
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------

using UnityEngine;

public class DD_Attack : MonoBehaviour
{
    // ---------------------------------------------------------------------
    private DD_GameManager gameManager;
    public float range = 10;
    public float speed = 3;
    public float damage = 30;
    public int teamID = -1; // Dont Attack own team

    // ---------------------------------------------------------------------
    void Start()
    {    // The game manager will be use to access the game board
        gameManager = GameObject.Find("GameManager").GetComponent<DD_GameManager>();
        Destroy(gameObject, (range / speed));
    }

    // ---------------------------------------------------------------------
    void FixedUpdate()
    {
        MoveAttack();
        CheckCurrentPositionForTargets();
    }//-----


    // ---------------------------------------------------------------------
    private void CheckCurrentPositionForTargets()
    {
        int currentX = (int)Mathf.Round(transform.position.x);
        int currentZ = (int)Mathf.Round(transform.position.z);

        // If the Current Position is on the board
        if (currentX >= 0 && currentX < gameManager.playArea.GetLength(1) && currentZ >= 0 && currentZ < gameManager.playArea.GetLength(1))
        {
            GameObject objectFound = gameManager.playArea[currentZ, currentX];
            //   print(objectFound);

            if (objectFound && objectFound.GetComponent<DD_UnitHealth>()) // object has health 
            {
                //   print("found team ID" + objectFound.GetComponent<DD_Unit>().team.teamID);
                //   print(" this ID" + teamID);

                if (objectFound.GetComponent<DD_Unit>().team.teamID != teamID) // Ensure Enemy team
                {
                    objectFound.GetComponent<DD_UnitHealth>().Damage(damage); // apply damage
                    Destroy(gameObject); // remove bullet
                }
            }
        }
    }//----


    // ---------------------------------------------------------------------
    private void MoveAttack()
    {
        transform.Translate(0, 0, speed * Time.deltaTime);
    }//----

}//==========
