using UnityEngine;

public class Defibrillator : Item
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    //It will not be possible to use a defibrillator on yourself. Attempting to use it from your inventory will always result in a fail.
    public override void OnUseInventory()
    {
        base.OnUseInventory();
        playerInventory.OnUseFail(this);
    }
}
