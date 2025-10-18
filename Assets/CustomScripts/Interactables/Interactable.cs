using UnityEngine;

public interface Interactable
{
    //GAMEOBJECT MUST HAVE RIGIDBODY

    //Action to do if interacted with
    void Interact(GameObject o)
    {

    }

    //Description to give player when they are hovering over interactable
    string Description();

    //The bottom is only worth messing with if the interaction requires holding.

    bool CanHoldInteract();

    //If tap-interact, always return true
    bool Release();

    //If tap-interact, leave empty
    void ReleaseAction();
}
