// ----------------------------------------------------------------------
// --------------------  AI: Building Controlled 0.09
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------
using UnityEngine;


    public class DD_Building : MonoBehaviour
    {
        public int teamID = 0;
        public float cost = 10;
        public float buildTime = 10;
        public Vector2 buildingSize = new(1, 1);
        public Vector3 currentPosition = Vector3.zero;
        public bool useArrayShape = false;


    public int[,] buildingShape =  
        {
        { 0, 0, 0, 1, 0, 0, 0 },
        { 0, 0, 1, 1, 1, 0, 0 },
        { 0, 1, 1, 1, 1, 1, 0 },
        { 1, 1, 1, 1, 1, 1, 1 },
        { 0, 1, 1, 1, 1, 1, 0 },
        { 0, 0, 1, 1, 1, 0, 0 },
        { 0, 0, 0, 1, 0, 0, 0 },
        };

}
