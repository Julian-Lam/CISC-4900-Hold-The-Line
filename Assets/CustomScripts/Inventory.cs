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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        invnTrns = inventoryObject.transform;
        DisplayItems();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void AddItem(Item i)
    {
        //Will only add if inventory is not full
        if (inventory.Count < 18)
        {
            inventory.Add(i);
            //Makes sure the items always move with the inventory's owner
            i.transform.SetParent(inventoryObject.transform);
            i.transform.position = invnTrns.position;
            i.gameObject.SetActive(false);
            DisplayItems();
        }
        else
        {
            Debug.Log("Inventory full");
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
                slotImage.sprite = null;
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
}
