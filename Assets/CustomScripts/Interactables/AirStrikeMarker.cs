using UnityEngine;

public class AirStrikeMarker : Item
{

    public float splashDamage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //It will not be possible to use a defibrillator on yourself. Attempting to use it from your inventory will always result in a fail.
    public override void OnUseInventory()
    {
        base.OnUseInventory();
        playerInventory.OnUseFail(this);
    }
}
