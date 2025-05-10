using UnityEngine;

public class Display : Node
{    
    private TimeGem secondaryGem = null;
    private bool canCombineGems = false;
    
    public override void ChangeGemType(TimeGem _gem)
    {
        //if doesn't already have gem
        if(gem == originalGem){
            base.ChangeGemType(_gem);
        }

        //else if has new gem and this is diff
        else if(gem != originalGem && gem != _gem){
            secondaryGem = _gem;
            isUpdatable = false;
        }
    }

    public override void UpdateDisplay()
    {
        if(gemDisplay.isDisplayingGem && secondaryGem != null){
            canCombineGems = true;
        }

        if(canCombineGems){
            CombineGems();
        }

        base.UpdateDisplay();
    }

    //tries to combine gems if secondary gem isn't null 
    public void CombineGems(){
        if(secondaryGem != null){
            TimeGem combinedGem = ScriptableObject.CreateInstance<TimeGem>();
            combinedGem.gemName = gem.gemName + secondaryGem.gemName;
            combinedGem.name = "Combined Gem";

            combinedGem.image = gem.image;
            combinedGem.color = Color.Lerp(gem.color, secondaryGem.color, 0.5f); //is there a better way to combine colors?
            
            gem = combinedGem;
            secondaryGem = null;
            isUpdatable = true;
        }
    }

    public override void ResetNode()
    {
        canCombineGems = false;
        secondaryGem = null;
        isUpdatable = true;
        
        base.ResetNode();
    }
}
