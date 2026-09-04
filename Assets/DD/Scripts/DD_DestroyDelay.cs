// ----------------------------------------------------------------------
// --------------------  AI:Object Destroy
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------

using UnityEngine;

public class DD_DestroyDelay : MonoBehaviour
{
    public float delay = 5;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, delay);

    }//-----


}//==========
