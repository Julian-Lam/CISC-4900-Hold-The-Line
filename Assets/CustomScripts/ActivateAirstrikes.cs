using UnityEngine;

public class ActivateAirstrikes : MonoBehaviour
{
    public static bool areAirstrikesActive = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        areAirstrikesActive = true;
        Pause.airStrikeCooldown = 10;
    }
}
