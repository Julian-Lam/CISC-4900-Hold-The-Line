using UnityEngine;
using TMPro;
using UnityEngine.UI;
using StarterAssets;
using System.Linq;

public class BuyStation : MonoBehaviour, Interactable
{
    public GameObject[] itemsToSell;
    private Transform stockShelf;
    public Sprite emptySlotSprite;
    private GameObject store;

    private GameObject transactionStatusObject;
    private TextMeshProUGUI transactionStatusMsg;

    private Button exitButton;

    protected ThirdPersonController player;
    protected Inventory playerInventory;
    protected Character c;

    private float weaponsBoughtThisSession;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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

        store = player.buyCanvas;
        stockShelf = FindDescendants(store.transform, "StoreShelf");
        transactionStatusObject = FindDescendants(store.transform, "TransactionStatusParent").gameObject;
        transactionStatusMsg = FindDescendants(store.transform, "TransactionStatusText").GetComponent<TextMeshProUGUI>();
        exitButton = FindDescendants(store.transform, "ExitButton").GetComponent<Button>();
        FindExitButton();

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
        weaponsBoughtThisSession = 0;
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
                TransactionStatus("#00961D", "Thank you for your purchase: " + purchasedItem.itemName + " | NY$" + purchasedItem.itemValue.ToString("N2") + ".");

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

    public void AttemptPurchase(Weapon w)
    {
        //If player can afford weapon
        if (weaponsBoughtThisSession == 0)
        {
            if (c.CanAfford(w.weaponValue))
            {
                //Pay
                c.Pay(w.weaponValue);

                //Spawn weapon on top of buy station
                GameObject purchasedWeaponObject = Instantiate(w.gameObject, transform.position + new Vector3(0, 1, 0.5f), Quaternion.Euler(0, 90, 0));
                Weapon purchasedWeapon = purchasedWeaponObject.GetComponent<Weapon>();

                player.currentWeapon = purchasedWeapon;
                purchasedWeapon.Interact(player.gameObject);

                weaponsBoughtThisSession++;
                TransactionStatus("#00961D", "Thank you for your purchase: " + purchasedWeapon.weaponName + " | NY$" + purchasedWeapon.weaponValue.ToString("N2") + ".");
            }
            else
            {
                TransactionStatus("#960000", "You cannot afford this.");
            }
        }
        else
        {
            TransactionStatus("#960000", "You already bought a weapon, try again later.");
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
                if(itemsToSell[listIterator].TryGetComponent<Item>(out Item currentItem))
                {
                    currentItem = itemsToSell[listIterator].GetComponent<Item>();

                    //Replace slot name with item name
                    nameBox.text = currentItem.itemName + " | NY$: " + currentItem.itemValue.ToString("N2");

                    //Replace slots with item sprites
                    b.onClick.AddListener(() => AttemptPurchase(currentItem));
                    slotImage.sprite = currentItem.itemSprite;
                }else if (itemsToSell[listIterator].TryGetComponent<Weapon>(out Weapon currentWeapon))
                {
                    currentWeapon = itemsToSell[listIterator].GetComponent<Weapon>();

                    //Replace slot name with item name
                    string shortenedName = currentWeapon.weaponName.Length > 10 ? currentWeapon.weaponName.Substring(0, 10) : currentWeapon.weaponName;
                    nameBox.text = shortenedName + " | NY$: " + currentWeapon.weaponValue.ToString("N2");

                    //Replace slots with item sprites
                    b.onClick.AddListener(() => AttemptPurchase(currentWeapon));
                    slotImage.sprite = currentWeapon.shopSprite;
                }
            }
            listIterator++;
        }
    }

    public Transform FindDescendants(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            else if (FindDescendants(child, name) != null)
            {
                return FindDescendants(child, name);
            }
        }
        return null;
    }
}
