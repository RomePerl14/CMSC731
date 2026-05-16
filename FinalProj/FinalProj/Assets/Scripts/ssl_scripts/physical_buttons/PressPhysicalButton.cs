using UnityEngine;
using UnityEngine.InputSystem;


public class PressPhysicalButton : MonoBehaviour
{
    public string button_name = "button";
    public GameObject sound;
    private float distance = -0.5f;
    public Camera playerCamera;
    private Transform obj;
    private bool button_press_1 = false;
    private bool button_press_2 = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        obj = this.GetComponent<Transform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // use the spring body to go down
        ClickMrMousey();
        this.obj.localPosition = new UnityEngine.Vector3(0,distance,0);
        if(button_press_1 == true && sound.activeSelf == true)
        {
            sound.SetActive(false);
        }
        else if(button_press_1 == false && sound.activeSelf == false)
        {
            sound.SetActive(true);
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
                       if(button_press_1 == false && button_press_2 == false)
                       {
                            button_press_1 = true;
                            button_press_2 = true;
                       }
                       if(button_press_1 == true && button_press_2 == false)
                       {
                            button_press_1 = false;
                            button_press_2 = true;
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
                if(button_press_2 == true)
                {
                    button_press_2 = false;
                }
            }
        }
    }
}
