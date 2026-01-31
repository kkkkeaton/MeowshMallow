using UnityEngine;

public class ComposableMono : MonoBehaviour
{

    Composable composable;

    bool isInit = false;

    long genId;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(Composable composable, long genId)
    {
        this.composable = composable;
        this.genId = genId;
        isInit = true;
    }
}
