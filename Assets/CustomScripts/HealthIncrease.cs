using UnityEngine;

public class HealthIncrease : MonoBehaviour, Interactable
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
        c.increaseHealth(10);
    }

    public string Description()
    {
        return "Increase health by 10";
    }

}
