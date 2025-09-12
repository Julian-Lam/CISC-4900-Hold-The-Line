using UnityEngine;

public class HealthDecrease : MonoBehaviour, Interactable
{
    private Character c;
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
        c = o.GetComponent<Character>();
        c.decreaseHealthAndArmor(5,10);
    }

    public string Description()
    {
        return "Decrease health by 10 or armor by 5";
    }

}
