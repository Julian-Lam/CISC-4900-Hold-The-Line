using UnityEngine;

public interface Interactable
{
    //GAMEOBJECT MUST HAVE RIGIDBODY
    void Interact(GameObject o)
    {

    }
    string Description();

    //The bottom is only worth messing with if the interaction requires holding.

    bool CanHoldInteract();

    bool Release();

    void ReleaseAction();
}
