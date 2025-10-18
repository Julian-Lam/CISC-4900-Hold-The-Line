using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Inventory : MonoBehaviour
{

    [SerializeField] private List<Item> inventory = new List<Item>();
    public GameObject inventoryObject;
    private Transform invnTrns;
    public Image inventoryUI;
    public Sprite emptySlotSprite;
    public GameObject useFailTextbox;
    public TextMeshProUGUI useFailText;
    public float useFailTextCooldown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        invnTrns = inventoryObject.transform;
        DisplayItems();
        useFailTextbox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (useFailTextCooldown > 0)
        {
            useFailTextCooldown -= Time.deltaTime;
            useFailTextbox.SetActive(true);
        }
        else
        {
            useFailTextbox.SetActive(false);
        }
    }
    public void AddItem(Item i)
    {
        if (CheckForSpace(i))
        {
            inventory.Add(i);
            //Makes sure the items always move with the inventory's owner
            i.transform.SetParent(inventoryObject.transform);
            i.transform.position = invnTrns.position;
            i.gameObject.SetActive(false);
            DisplayItems();
        }
    }

    public bool CheckForSpace(Item i)
    {
        //Will only add if inventory is not full
        if (inventory.Count < 18)
        {
            float itemOfTypeCurrentlyHave = 0;

            //Check how many items are the same types as Item i
            foreach (Item currentItem in inventory)
            {
                if (currentItem.itemName == i.itemName)
                {
                    itemOfTypeCurrentlyHave++;
                }
            }

            //Will not add if there are already enough items in the inventory that are the same type as Item i
            if (i.maxAllowed > 0 && itemOfTypeCurrentlyHave < i.maxAllowed)
            {
                return true;
            }
            else
            {
                useFailText.text = "You cannot carry any more of this type of item.";
                useFailTextCooldown = 3;
                return false;
            }
        }
        else
        {
            useFailText.text = "Inventory full.";
            useFailTextCooldown = 3;
            return false;
        }
    }

    public void UseItem(Item i)
    {
        Debug.Log("Used Item: " + i);
        i.OnUseInventory();
    }
    public void RemoveItem(Item i)
    {
        inventory.Remove(i);
        DisplayItems();
    }

    public void OnDrop(Item i)
    {
        i.transform.SetParent(null);
        i.transform.position = invnTrns.position;
        i.gameObject.SetActive(true);
        RemoveItem(i);
    }

    //Find Item by name
    public Item FindItem(string name)
    {
        Item foundItem=null;
        
        //Look thru inventory list
        foreach(Item i in inventory)
        {
            if (name == i.itemName)
            {
                foundItem = i;
                break;
            }
        }

        return foundItem;
    }

    //Find item by item type: i.e. FindItem(Item.ItemType.Consumable);
    public Item FindItem(Item.ItemType type)
    {
        Item foundItem = null;

        //Look thru inventory list
        foreach (Item i in inventory)
        {
            if (type == i.itemType)
            {
                foundItem = i;
                break;
            }
        }

        return foundItem;
    }

    //Find item by item type: i.e. FindItem(typeof(Medkit));
    public T FindItem<T>() where T: Item
    {
        Item foundItem = null;

        //Look thru inventory list
        foreach (Item i in inventory)
        {
            if (i is T)
            {
                foundItem = i;
                break;
            }
        }

        return foundItem as T;
    }
    public void DisplayItems()
    {
        int listIterator = 0;
        foreach(Transform slot in inventoryUI.transform)
        {
            //Get the slot's components
            Image slotImage = slot.GetComponent<Image>();

            Transform nameBoxTransform = slot.GetChild(0);
            TextMeshProUGUI nameBox = nameBoxTransform.GetComponent<TextMeshProUGUI>();

            Button b = slot.GetComponent<Button>();
            b.onClick.RemoveAllListeners();
            //If iterator goes over list count
            if (listIterator > inventory.Count-1)
            {
                slotImage.sprite = emptySlotSprite;
                nameBox.text = "Empty";
            }
            else
            {
                Item currentItem = inventory[listIterator];

                //Replace slot name with item name
                nameBox.text = currentItem.itemName;

                //Replace slots with item sprites
                b.onClick.AddListener(() => UseItem(currentItem));
                slotImage.sprite = currentItem.itemSprite;
            }
            //Go to next slot/item
            listIterator++;
        }
    }

    //Disable buttons on pause
    public void ToggleButtons()
    {
        Button currentButton;
        foreach(Transform slot in inventoryObject.transform)
        {
            //Get button components
            currentButton = slot.GetComponent<Button>();
            currentButton.interactable = !currentButton.interactable;
        }
    }

    //If you cannot use the item for whatever reason
    public void OnUseFail(Item i)
    {
        if (i.useFail != null)
        {
            //Displays a text letting you know for a certain amount of time
            useFailText.text = i.useFail;
            useFailTextCooldown = 3;
        }
    }

    public void OnUseFail(string msg)
    {
        useFailText.text = msg;
        useFailTextCooldown = 3;
    }
}
