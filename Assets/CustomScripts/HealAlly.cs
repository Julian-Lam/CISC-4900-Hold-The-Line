using UnityEngine;
using StarterAssets;

public class HealAlly : MonoBehaviour, Interactable
{
    private AlliedCharacter c;
    private ThirdPersonController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        c = GetComponent<AlliedCharacter>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact(GameObject o)
    {
        player = o.GetComponent<ThirdPersonController>();
        if (this.enabled)
        {
            Heal();
        }
    }

    public string Description()
    {
        return "Heal Ally";
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

    public void Heal()
    {
        if (c.health>=c.maxHealth || c.health <= 0)
        {
            c.increaseHealth(5f);
        }
    }
}
