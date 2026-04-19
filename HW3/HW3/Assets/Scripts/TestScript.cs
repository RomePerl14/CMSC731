using UnityEngine;

public class TestScript : MonoBehaviour
{
    UnityEngine.Vector3 balls = new UnityEngine.Vector3(0,0,0);
    UnityEngine.Vector3 dir = new UnityEngine.Vector3(0,0,1);
    public float dist;

    LineRenderer lineRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Set the material
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Set the color
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.green;

        // Set the width
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        // Set the number of vertices
        lineRenderer.positionCount = 2;

        // Set the positions of the vertices
        lineRenderer.SetPosition(0, this.transform.position);
        UnityEngine.Quaternion quat = this.transform.rotation;

        lineRenderer.SetPosition(1, (this.transform.position + (quat * new Vector3(0,0,1)))*dist);
    }
}
