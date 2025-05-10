using UnityEngine;

public class Door : Node
{
    public TimeGem.Shape requirement {get; private set;}
    public bool reqMet = false;

    public void SetRequirement(TimeGem.Shape _req){
        requirement = _req;
    }

    public override void ChangeGemType(TimeGem _gem)
    {   
        //if correct gem
        if(_gem != null && _gem.shape == requirement){ 
            // Debug.Log("Correct gem passed!");
            gem = _gem;
            reqMet = true;

            if(GetComponent<ParticleFX>() != null) GetComponent<ParticleFX>().Play();
            
            //sends success to room
            GetComponentInParent<Room>().ParameterSatisfied(gem.shape);
        }
        else{
            //If incorrect gem added, do nothing
            // Debug.Log("incorrect gem added");
        }
    }

    public override void ResetNode()
    {
        base.ResetNode();

        reqMet = false;
    }
}
