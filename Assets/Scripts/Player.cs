using UnityEngine;

public class Player : MonoBehaviour,IMaskInfoProvider
{
    MaskCore playerMaskCore = new MaskCore();

    public Transform composableParent;

    public virtual MaskCore GetMaskInfo()
    {
        return playerMaskCore;
    }

    public void Awake()
    {
        composableParent = transform.Find("ComposableParent");
    }

    public void Start()
    {
        playerMaskCore.Parse("");
    }


    public void AddOneElement2Main(Element element)
    {
        playerMaskCore.AddOneElement2Main(element);
    }

    public bool TryRemoveOneElementFromMain(int typeId,Vector2 pos,float rot)
    {
        return playerMaskCore.TryRemoveOneElementFromMain(typeId, pos, rot);
    }
}
