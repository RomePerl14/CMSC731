using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    public Material highlighted;
    public Material unhighlighted;
    Renderer Mat;
    bool highlighting = false;
    int count = 0;
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
        count = 0;
    }
    public void UnHighlight()
    {
        highlighting = false;
        Mat.material = unhighlighted;
    }

    void Update()
    {
        if(highlighting == true && count == 10)
        {
            UnHighlight();
            count = 0;
        }
        else
        {
            count += 1;
        }

        if(count > 10)
        {
            count = 10;
        }
    }
}
