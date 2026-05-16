using UnityEngine;

public class GenerateRandomTowers : MonoBehaviour
{
    GameObject[] cubes = new GameObject[1000];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i=0;i<1000;i+=1)
        {
            cubes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubes[i].transform.localScale = new UnityEngine.Vector3(3,15,3);
            cubes[i].transform.position = new UnityEngine.Vector3(Random.Range(-200f, 200f), Random.Range(0f,20f), Random.Range(-200f, 200f));
        }
    }
}
