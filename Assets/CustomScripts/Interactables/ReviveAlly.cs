using UnityEngine;
using StarterAssets;
using System.Collections;
using UnityEngine.UI;

public class ReviveAlly : MonoBehaviour, Interactable
{
    public float pointsNeededToRevive=300;
    public float currentPoints=0;

    private AlliedCharacter c;
    private ThirdPersonController player;
    private Inventory playerInventory;

    private Item defib;
    private Medkit medkit;

    public GameObject reviveBarParent;
    public Image reviveBar;

    private float reviveCooldown;
    //public Camera reviverCamera;

    public string status;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        c = GetComponent<AlliedCharacter>();
        reviveBarParent.SetActive(false);
    }

    void Awake()
    {
        reviveBarParent.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(c.health>0 && c.health < c.maxHealth)
        {
            status = "Heal";
        }
        else if (c.health <= 0)
        {
            status = "Revive";
            CheckForNearbyAlly();
            ManageBars();
        }

        if (reviveCooldown > 0)
        {
            reviveCooldown -= Time.deltaTime;
        }
    }

    public void Interact(GameObject o)
    {
        if (this.enabled)
        {
            player = o.GetComponent<ThirdPersonController>();
            playerInventory = player.GetComponent<Inventory>();
            defib = playerInventory.FindItem("Defibrillator");
            medkit = playerInventory.FindItem<Medkit>();

            if (status == "Revive")
            {

                if (defib!=null)
                {
                    if (c.health < c.maxHealth)
                    {
                        reviveBarParent.SetActive(true);
                    }
                    currentPoints++;
                }
                else
                {
                    playerInventory.OnUseFail("You do not have a Hefibrillator.");
                }
            }
            else if (status == "Heal" && reviveCooldown<=0)
            {
                if (medkit != null)
                {
                    medkit.HealCharacter(c);
                }
                else
                {
                    playerInventory.OnUseFail("You do not have a Medic Bag.");
                }
            }
        }
    }

    public string Description()
    {
        if (status == "Revive")
        {
            return "Revive";
        }
        else if (status == "Heal")
        {
            return "Heal";
        }
        else
        {
            return null;
        }
    }

    public bool CanHoldInteract()
    {
        if (status == "Revive")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Release()
    {
        if (status == "Revive")
        {
            //Debug.Log("IsReviving and bool release is: " + IsDoneReviving());
            CheckPoints();
            return IsDoneReviving();
        }
        else
        {
            return true;
        }
    }

    public void ReleaseAction()
    {
        CancelRevive();
    }

    //If reviver somehow gets away from downed, stop the revive
    public void CheckForNearbyAlly()
    {
        if (c.distanceFromPlayer > 2)
        {
            CancelRevive();
        }
    }

    public void CancelRevive()
    {
        reviveBarParent.SetActive(false);
        currentPoints = 0;
    }

    public bool IsDoneReviving()
    {
        return currentPoints >= pointsNeededToRevive;
    }

    public void CheckPoints()
    {
        if (IsDoneReviving())
        {
            playerInventory.RemoveItem(defib);
            CancelRevive();
            c.Revive();
            reviveCooldown = 1;
            this.enabled = false;
        }
    }

    public void ManageBars()
    {
        reviveBar.fillAmount = currentPoints / pointsNeededToRevive;
    }

}
