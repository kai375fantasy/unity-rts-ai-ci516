// ----------------------------------------------------------------------
// --------------------  AI: Player Input Manager 0.10
// -------------------- David Dorrington, UoB Games, 2024
// ---------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

public class DD_PlayerInputManager : MonoBehaviour
{
    // ---------------------------------------------------------------------
    [Header("Camera Settings")]
    public GameObject gameCamera;
    public float camMoveSpeed = 10f;
    public float moveAreaBorderSize = 50;

    [Header("Player Set Positions")]
    public Vector2 leftClickPosition = new(-1, -1);
    public Vector2 rightClickPosition = new(-1, -1);
    public Vector2 leftDownPosition = new(-1, -1);
    public Vector2 leftUpPosition = new(-1, -1);
    public bool mouseLUp = false;
    public bool mouseLDown = false;
    public bool mouseRDown = false;

    // GUI Action Buttons
    public bool button1Activated = false;
    public bool button2Activated = false;
    public bool button3Activated = false;
    public bool button4Activated = false;

    public Slider slider1 = null;
    public Slider slider2 = null;
    public Slider slider3 = null;
    public Slider slider4 = null;

    // ---------------------------------------------------------------------
    private void Update() // Checked Every frame 
    {
        GetPlayerMouseClick();
        CameraControl();
    }//-----

     // ---------------------------------------------------------------------
    public void GetActionButton(int buttonPressed)
    {
        switch (buttonPressed)
        {
            case 1:
                button1Activated = true;
                break;
            case 2:
                button2Activated = true;
                break;
            case 3:
                button3Activated = true;
                break;
            case 4:
                button4Activated = true;
                break;
            default:
                button1Activated = false;
                button2Activated = false;
                button3Activated = false;
                button4Activated = false;
                break;
        }
    }//------
    // ---------------------------------------------------------------------
    private void CameraControl()
    {
        // Detect input and update camera postion
        float newCamX = gameCamera.transform.position.x + Input.GetAxis("Horizontal") * camMoveSpeed * Time.deltaTime;
        float newCamZ = gameCamera.transform.position.z + Input.GetAxis("Vertical") * camMoveSpeed * Time.deltaTime;
        float newCamY = gameCamera.transform.position.y;

        if (Input.mousePosition.y > 0 && Input.mousePosition.y < moveAreaBorderSize) newCamZ = gameCamera.transform.position.z - camMoveSpeed * Time.deltaTime;
        if (Input.mousePosition.x > 0 && Input.mousePosition.x < moveAreaBorderSize) newCamX = gameCamera.transform.position.x - camMoveSpeed * Time.deltaTime; ;
        if (Input.mousePosition.y < Screen.height && Input.mousePosition.y > Screen.height - moveAreaBorderSize) newCamZ = gameCamera.transform.position.z + camMoveSpeed * Time.deltaTime; ;
        if (Input.mousePosition.x < Screen.width && Input.mousePosition.x > Screen.width - moveAreaBorderSize) newCamX = gameCamera.transform.position.x + camMoveSpeed * Time.deltaTime; ;


        gameCamera.transform.position = new(newCamX, newCamY, newCamZ);

        // Zoom by adjusting Field of view with mouse wheel
        if (Input.mouseScrollDelta.y < 0)
            gameCamera.GetComponent<Camera>().fieldOfView++;

        if (Input.mouseScrollDelta.y > 0)
            gameCamera.GetComponent<Camera>().fieldOfView--;
    }//---

 // ================================================================================

    private void GetPlayerMouseClick()
    { 
        //if the left or right mouse button clicked
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Create Ray cast source and target 
            Ray rayCamToMousePostion = Camera.main.ScreenPointToRay(Input.mousePosition);

            // cast Ray 100m and store data in _rh_hit)
            if (Physics.Raycast(rayCamToMousePostion, out RaycastHit rayHit, 100F))
            {
                // Set Position LM Clicked id on the gameBoard 
                if (Mathf.Round(rayHit.point.x) > 0 && Mathf.Round(rayHit.point.x) < 100
                    && Mathf.Round(rayHit.point.z) > 0 && Mathf.Round(rayHit.point.z) < 100)
                {
                    if (Input.GetMouseButtonDown(0)) // Left Button                   
                        leftClickPosition = new((int)rayHit.point.x, (int)rayHit.point.z);

                    if (Input.GetMouseButtonDown(1)) // Right Button            
                    {
                        rightClickPosition = new((int)rayHit.point.x, (int)rayHit.point.z);
                        mouseRDown = true;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0)) // Left Mouse still held Down
        {
            mouseLUp = false;
            mouseLDown = true;
            // Create Ray cast source and target 
            Ray rayCamToMousePostion = Camera.main.ScreenPointToRay(Input.mousePosition);

            // cast Ray 100m and store data in _rh_hit)
            if (Physics.Raycast(rayCamToMousePostion, out RaycastHit rayHit, 100F))
            {
                // Set Position LM Clicked id on the gameBoard 
                if (Mathf.Round(rayHit.point.x) > 0 && Mathf.Round(rayHit.point.x) < 100
                    && Mathf.Round(rayHit.point.z) > 0 && Mathf.Round(rayHit.point.z) < 100)
                    leftDownPosition = new((int)rayHit.point.x, (int)rayHit.point.z);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            mouseLUp = true;
            mouseLDown = false;
        }

        if (Input.GetMouseButtonUp(1))
        {
            mouseRDown = false;
        }

    }//------


}//==========
