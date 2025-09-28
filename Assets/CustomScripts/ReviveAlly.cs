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

    public Camera reviverCamera;

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
        }
        
        
        CheckPoints();
        CheckForNearbyAlly();
        ManageBars();

        if (player != null)
        {
            GameObject playerObj = player.gameObject;
            Transform reviverRoot = playerObj.transform;
            Transform cameraTransform = reviverRoot.Find("MainCamera");
            reviverCamera = cameraTransform.gameObject.GetComponent<Camera>();

            Ray r = new Ray(reviverCamera.transform.position, reviverCamera.transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, 8.0f))
            {
                Debug.DrawRay(reviverCamera.transform.position, reviverCamera.transform.TransformDirection(Vector3.forward) * 8f, Color.red);
                if (hit.collider.gameObject != gameObject)
                {
                    CancelRevive();
                }
            }
            else
            {
                CancelRevive();
            }
        }
    }

    public void Interact(GameObject o)
    {
        if (this.enabled)
        {
            player = o.GetComponent<ThirdPersonController>();

            if (status == "Revive")
            {
                reviveBarParent.SetActive(true);
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

    public bool release = false;

    public bool Release()
    {
        return release;
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
        release = false;
    }

    public void CheckPoints()
    {
        if (currentPoints >= pointsNeededToRevive)
        {
            c.Revive();
            release = true;
            CancelRevive();
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
