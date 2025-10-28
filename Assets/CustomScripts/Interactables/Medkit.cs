using UnityEngine;

public class Medkit : Item
{
    public float healValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
        itemType = ItemType.Consumable;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public void HealCharacter(Character npc)
    {
        if (npc.health < npc.maxHealth)
        {
            npc.health += healValue;
            playerInventory.RemoveItem(this);
            Destroy(this);

        }
        else if (npc.health >= npc.maxHealth)
        {
            //If character is at full health, it will not be used.
            playerInventory.OnUseFail(this);
        }
    }

    override public void OnUseInventory()
    {
        HealCharacter(c);
    }
}
