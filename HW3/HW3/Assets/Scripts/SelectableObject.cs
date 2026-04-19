using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    public Material highlighted;
    public Material unhighlighted;
    Renderer Mat;
    bool highlighting = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Mat = this.GetComponent<Renderer>();
        Mat.material = unhighlighted;
    }

    public void Highlight()
    {
        highlighting = true;
        Mat.material = highlighted;
    }

    // Update is called once per frame
    void Update()
    {
        if(highlighting == true)
        {
            highlighting = false;
            Mat.material = unhighlighted;

        }
    }
}
