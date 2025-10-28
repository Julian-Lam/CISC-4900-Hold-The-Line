using UnityEngine;
using StarterAssets;

public class Item : MonoBehaviour, Interactable
{

    public enum ItemType
    {
        QuestItem,
        Static,
        Money,
        Consumable
    }

    public ItemType itemType;
    public Sprite itemSprite;
    public string itemName;
    public float itemValue;
    protected ThirdPersonController player;
    protected Inventory playerInventory;
    protected Character c;
    public string useFail;
    public float weight; //Used for drop chance
    public float destroyTimer;

    //Limit of item of this type that can be carried. Set to zero if you want to remove limits.
    //ABSOLUTLY DO NOT CHANGE PREFABS THAT ARE IN THE SCENE
    public float maxAllowed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    virtual public void Start()
    {
        GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        destroyTimer = 60f;
    }

    // Update is called once per frame
    virtual public void Update()
    {
        destroyTimer -= Time.deltaTime;

        if (destroyTimer <= 0)
        {
            DestroyIfTimedOut();
        }
    }

    public void Interact(GameObject o)
    {
        //Get info about interactor/player
        player = o.GetComponent<ThirdPersonController>();
        playerInventory = o.GetComponent<Inventory>();
        c = o.GetComponent<Character>();

        //If money, add value to player currency, else store in inventory
        if(itemType == ItemType.Money)
        {
            c.currency += itemValue;
            Destroy(gameObject);
        }
        else
        {
            playerInventory.AddItem(this);
        }
    }

    public string Description()
    {
        if (itemType == ItemType.Money)
        {
            return "Pick Up: NY$" + itemValue;
        }
        else
        {
            return "Pick Up: " + itemName;
        }
    }

    public bool CanHoldInteract()
    {
        return false;
    }

    public bool Release()
    {
        return true;
    }

    public void ReleaseAction()
    {

    }

    public void StoreInInventory(Inventory inv)
    {
        player = inv.GetComponent<ThirdPersonController>();
        playerInventory = inv;
        c = inv.GetComponent<Character>();
    }

    public void StoreInInventory(Character character)
    {
        player = character.GetComponent<ThirdPersonController>();
        playerInventory = character.GetComponent<Inventory>();
        c = character;
    }

    virtual public void OnUseInventory()
    {

    }

    public void DestroyIfTimedOut()
    {
        if (itemType != ItemType.QuestItem && transform.parent==null)
        {
            Destroy(gameObject);
        }
    }
}
