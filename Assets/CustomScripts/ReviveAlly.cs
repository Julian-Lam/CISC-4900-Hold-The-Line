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

    public GameObject reviveBarParent;
    public Image reviveBar;

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
    }

    public void Interact(GameObject o)
    {
        if (this.enabled)
        {
            player = o.GetComponent<ThirdPersonController>();

            if (status == "Revive")
            {
                if (c.health < c.maxHealth)
                {
                    reviveBarParent.SetActive(true);
                }
                currentPoints++;
            }
            else if (status == "Heal")
            {
                Heal();
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
            CancelRevive();
            c.Revive();
            this.enabled = false;
        }
    }

    public void Heal()
    {
        if (c.health <= c.maxHealth || c.health > 0)
        {
            c.increaseHealth(10f);
        }
    }

    public void ManageBars()
    {
        reviveBar.fillAmount = currentPoints / pointsNeededToRevive;
    }

}
