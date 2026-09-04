// ----------------------------------------------------------------------
// --------------------  AI: Item Class 0.03

// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------

public class DD_Item : DD_BaseObject
{
    private void Start()
    {
        xPos = (int)transform.position.x;
        zPos = (int)transform.position.z;
        isAlive = true;
    }

}//==========

