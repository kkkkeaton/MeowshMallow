using UnityEngine;

public class ComposableMono : MonoBehaviour
{

    Composable composable;

    bool isInit = false;
    Vector2 pos;
    float rot;

    int genId;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(Composable composable, int genId,Vector2 pos,float rot)
    {
        this.composable = composable;
        this.genId = genId;
        this.pos = pos;
        this.rot = rot;
        isInit = true;
    }

    public int GetGenId()
    {
        return this.genId;
    }


    public void GetOriginalConfig(out Composable composable, out Vector2 pos, out float rot)
    {
        composable = this.composable;
        pos = this.pos;
        rot = this.rot;
    }
}
