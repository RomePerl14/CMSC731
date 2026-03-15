using UnityEngine;
using UnityEngine.InputSystem;


public class TeleportButton : MonoBehaviour
{
    public string button_name = "button";
    private float distance = -0.5f;
    public Camera playerCamera;
    private Transform obj;
    private Transform player;
    private bool button_press_1 = false;
    private UnityEngine.Vector3 origin = new UnityEngine.Vector3(0,1,10);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        obj = this.GetComponent<Transform>();
        player = playerCamera.GetComponent<Transform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // use the spring body to go down
        ClickMrMousey();
        this.obj.localPosition = new UnityEngine.Vector3(0,distance,0);
        if(button_press_1 == true && player.localPosition != origin)
        {
            player.localPosition = origin;
        }
        
    }

    void ClickMrMousey()
    {
        Ray ray = playerCamera.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        RaycastHit hit;

        // Check if the ray hits any collider
        if (UnityEngine.Physics.Raycast(ray, out hit))
        {
            if((hit.collider.name == button_name))
            {
                if((UnityEngine.InputSystem.Mouse.current.leftButton.isPressed))
                {
                    
                    if(distance > -0.54f)
                    {
                        distance -= 0.02f;
                    }
                    else
                    {
                       if(button_press_1 == false)
                       {
                            button_press_1 = true;
                       }
                    }
                    this.obj.localPosition = new UnityEngine.Vector3(0,distance,0);
                }
                
            }
        }

        if(UnityEngine.InputSystem.Mouse.current.leftButton.isPressed == false)
        {
            if(distance < -0.5f)
            {
                distance += 0.01f;
            }
            else
            {
                if(button_press_1 == true)
                {
                    button_press_1 = false;
                }
            }
        }
    }
}
