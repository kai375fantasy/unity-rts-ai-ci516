// ----------------------------------------------------------------------
// --------------------  AI: Resource 0.05
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------

using UnityEngine;

public class DD_Resource : DD_BaseObject
{
    // ---------------------------------------------------------------------
    public float resourceHeld = 10;
    private DD_GameManager gameManager;

    private void Start()
    {  // The game manager will be use to access the game board
        gameManager = GameObject.Find("GameManager").GetComponent<DD_GameManager>();
        xPos = (int)transform.position.x;
        zPos = (int)transform.position.z;
    }//-----

    // ---------------------------------------------------------------------
    private void FixedUpdate()
    {
        CheckResourceLevel();
    }//-----

    // ---------------------------------------------------------------------
    private void CheckResourceLevel()
    {
        if (resourceHeld <= 0.2F)
        {                   
            gameManager.playArea[zPos, xPos] = null; // clear resource from array
            gameManager.activeResources.Remove(gameObject);
            isAlive = false;    
            gameObject.SetActive(false);  
        }
    }//-----

    // ---------------------------------------------------------------------
    // Receiver - units access this to steal the resource
    public void GetResource(float amountToHarvest)
    {
        if (resourceHeld > amountToHarvest) resourceHeld -= amountToHarvest;
    }//-----

}//==========
