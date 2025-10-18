using UnityEngine;
using TMPro;
using UnityEngine.UI;
using StarterAssets;

public class BuyStation : MonoBehaviour, Interactable
{
    public GameObject[] itemsToSell;
    public Transform stockShelf;
    public Sprite emptySlotSprite;
    public GameObject store;

    public GameObject transactionStatusObject;
    public TextMeshProUGUI transactionStatusMsg;

    protected ThirdPersonController player;
    protected Inventory playerInventory;
    protected Character c;

    public Button exitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transactionStatusObject.SetActive(false);
        store.SetActive(false);
        FindExitButton();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Interact(GameObject o)
    {
        player = o.GetComponent<ThirdPersonController>();
        c = o.GetComponent<Character>();
        playerInventory = o.GetComponent<Inventory>();
        DisplayStock();
        store.SetActive(true);
        transactionStatusObject.SetActive(false);
        Pause.isAnInterfaceActive = true;
        Pause.isInventoryOpen = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public string Description()
    {
        return "Use Buy Station";
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

    public void ExitBuyStation()
    {
        store.SetActive(false);
        transactionStatusObject.SetActive(false);
        Pause.isAnInterfaceActive = false;
        Pause.isInventoryOpen = false;
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void FindExitButton()
    {
        exitButton.onClick.AddListener(() => ExitBuyStation());
    }

    public void AttemptPurchase(Item i)
    {
        //If player can afford item
        if (c.CanAfford(i.itemValue))
        {
            //Check for inventory space
            if (playerInventory.CheckForSpace(i))
            {
                //Pay
                c.Pay(i.itemValue);

                //Create a clone of the item in the shelf
                GameObject purchasedItemObject = Instantiate(i.gameObject, playerInventory.transform.position, Quaternion.identity, playerInventory.transform);
                purchasedItemObject.gameObject.SetActive(false);
                Item purchasedItem = purchasedItemObject.GetComponent<Item>();
                TransactionStatus("#00961D", "Successfully bought " + purchasedItem.itemName + " for NY$" + purchasedItem.itemValue + ".");

                //Add item to inventory
                purchasedItem.StoreInInventory(playerInventory);
                playerInventory.AddItem(purchasedItem);
            }
        }
        else
        {
            TransactionStatus("#960000", "You cannot afford this.");
        }
    }

    public void TransactionStatus(string hexDecimal,string msg)
    {
        ColorUtility.TryParseHtmlString(hexDecimal, out Color col);
        Image img = transactionStatusObject.GetComponent<Image>();
        col.a = 125;
        img.color = col;
        transactionStatusMsg.text = msg;
        transactionStatusObject.SetActive(true);
    }

    public void DisplayStock()
    {
        int listIterator = 0;
        foreach (Transform slot in stockShelf)
        {
            //Get the slot's components
            Image slotImage = slot.GetComponent<Image>();

            Transform nameBoxTransform = slot.GetChild(0);
            TextMeshProUGUI nameBox = nameBoxTransform.GetComponent<TextMeshProUGUI>();

            Button b = slot.GetComponent<Button>();
            b.onClick.RemoveAllListeners();

            //If the number of item types reaches its max
            if (listIterator > itemsToSell.Length - 1)
            {
                slotImage.sprite = emptySlotSprite;
                nameBox.text = "Out Of Stock";
            }
            else
            {
                Item currentItem = itemsToSell[listIterator].GetComponent<Item>();

                //Replace slot name with item name
                nameBox.text = currentItem.itemName + " | NY$: " + currentItem.itemValue;

                //Replace slots with item sprites
                b.onClick.AddListener(() => AttemptPurchase(currentItem));
                slotImage.sprite = currentItem.itemSprite;
            }
            listIterator++;
        }
    }
}
