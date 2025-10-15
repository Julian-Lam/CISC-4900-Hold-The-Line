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
        Debug.Log("Used Item: "+i);
        i.OnUse();
        if (i.itemType == Item.ItemType.Consumable)
        {
            RemoveItem(i);
            Destroy(i);
        }
    }
    public void RemoveItem(Item i)
    {
        inventory.Remove(i);
        DisplayItems();
    }

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
    public void DisplayItems()
    {
        int listIterator = 0;
        foreach(Transform slot in inventoryUI.transform)
        {
            //Get the slot's components
            GameObject slotObject = slot.gameObject;
            Image slotImage = slotObject.GetComponent<Image>();

            Transform nameBoxTransform = slot.GetChild(0);
            TextMeshProUGUI nameBox = nameBoxTransform.GetComponent<TextMeshProUGUI>();

            Button b = slotObject.GetComponent<Button>();
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
                nameBox.text = currentItem.name;

                //Replace slots with item sprites
                b.onClick.AddListener(() => UseItem(currentItem));
                slotImage.sprite = currentItem.itemSprite;
            }
            //Go to next slot/item
            listIterator++;
        }
    }
}
