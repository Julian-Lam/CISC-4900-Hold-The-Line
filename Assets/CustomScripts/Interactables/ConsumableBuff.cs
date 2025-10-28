using UnityEngine;

public class ConsumableBuff : Item
{
    public enum BuffType
    {
        Invincibility,
        SpeedBuff,
    }

    public BuffType buffType;

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

    public override void OnUseInventory()
    {
        switch (buffType)
        {
            case BuffType.Invincibility:
                c.invincibilityTimer += 8;
                break;
            case BuffType.SpeedBuff:
                c.stamina = c.maxStamina;
                c.infiniteStaminaTimer += 8;
                break;
        }
        playerInventory.RemoveItem(this);
        Destroy(this);
    }
}
