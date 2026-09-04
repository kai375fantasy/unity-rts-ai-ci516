// ----------------------------------------------------------------------
// --------------------  AI: Base Object Class 0.05
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------

using UnityEngine;

public abstract class DD_BaseObject : MonoBehaviour
{
    // This will Appear on Derived Class Ojects
    [Header("Base Object Settings")]
    public bool isAlive = false; 
    public float health = 100;
    [HideInInspector]
    public int xPos, zPos; // position in array


}//===========
