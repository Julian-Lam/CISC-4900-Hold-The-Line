using UnityEngine;

public class Medkit : Item
{
    public float healValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemType = ItemType.Consumable;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HealCharacter(Character npc)
    {
        if (npc.health < npc.maxHealth)
        {
            npc.health += healValue;
            playerInventory.RemoveItem(this);
            Destroy(this);
        }
    }

    override public void OnUseInventory()
    {
        HealCharacter(c);
    }
}
