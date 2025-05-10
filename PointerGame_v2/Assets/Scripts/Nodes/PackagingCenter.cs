using UnityEngine;

public class PackagingCenter : Gate
{
    // [Header("Components")]
    // [Header("PC Info")]
    // [SerializeField] private 

    protected override void Start()
    {
        base.Start();
    }

    public override void ChangeGemType(TimeGem _gem)
    {
        base.ChangeGemType(_gem);

        if(goalCompleted){
            Debug.Log("won level!");
        }
    }
}
