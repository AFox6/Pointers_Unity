using UnityEngine;

public class Mine : Node
{
    // [HideInInspector] public bool isUpdatable;
    protected override void Start()
    {
        base.Start();
        isUpdatable = false;
    }

    public override void ChangeGemType(TimeGem _gem){
        //do nothing b/c cannot override gem
    }
}