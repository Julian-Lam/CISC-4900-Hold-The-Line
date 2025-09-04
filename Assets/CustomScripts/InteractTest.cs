using UnityEngine;

public class InteractTest : MonoBehaviour, Interactable
{
    
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
        Debug.Log(o + " interacted with me!");
    }
}
