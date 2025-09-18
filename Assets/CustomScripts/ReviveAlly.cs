using UnityEngine;
using StarterAssets;
using UnityEngine.UI;

public class ReviveAlly : MonoBehaviour, Interactable
{
    public float pointsNeededToRevive=300;
    public float currentPoints=0;

    public bool isReviving;

    private AlliedCharacter c;
    private ThirdPersonController player;

    public GameObject reviveBarParent;
    public Image reviveBar;

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
        if (isReviving)
        {
            currentPoints++;
            CheckPoints();
        }
        CheckForNearbyAlly();
        ManageBars();
    }

    public void Interact(GameObject o)
    {
        if (this.enabled)
        {
            player = o.GetComponent<ThirdPersonController>();
            if (!isReviving)
            {
                isReviving = true;
            }
            else
            {
                CancelRevive();
            }
        }
    }

    public string Description()
    {
        if (!isReviving)
        {
            return "Revive";
        }
        else
        {
            return "Cancel Revive";
        }
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
        isReviving = false;
        currentPoints = 0;
    }

    public void CheckPoints()
    {
        if (currentPoints >= pointsNeededToRevive)
        {
            c.Revive();
            c.health = c.maxHealth*0.75f;
            CancelRevive();
            this.enabled = false;
        }
    }

    public void ManageBars()
    {
        if (isReviving)
        {
            reviveBarParent.SetActive(true);
            reviveBar.fillAmount = currentPoints / pointsNeededToRevive;
        }
        else
        {
            reviveBarParent.SetActive(false);
        }
    }

}
