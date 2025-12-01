using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using System.Collections;
using UnityEngine.InputSystem;

public class Dialogue : MonoBehaviour, Interactable
{
    public string charName;
    public string firstTimeInteractionName;
    private bool dialogueLocked;
    public float timesInteractedWith;
    private int index;
    private GameObject playerHUD;
    private GameObject playerDialogueBox;
    private TextMeshProUGUI DBCharName;
    private TextMeshProUGUI DBCharText;
    private Image DBCharPFP;
    private Transform playerModel;
    
    public DialogueLine[] dialogueLines;

    public bool isHumanoidOrAnimal;

    private InputActionAsset userInput;

    private InputAction continueText;

    private Quaternion oldRotation;
    private Quaternion playerRotation;

    public static bool lockInput=false;

    public static Dialogue activeDialogue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        oldRotation = transform.rotation;
        timesInteractedWith = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (activeDialogue != this) return;

        if (userInput!=null && userInput.enabled && continueText.WasPressedThisFrame() && !lockInput)
        {
            NextLine();
        }

        if (playerModel != null && dialogueLocked)
        {
            LookAt(playerModel, transform);
            if (isHumanoidOrAnimal)
            {
                LookAt(transform, playerModel);
            }
        }
    }
    public void Interact(GameObject o)
    {
        StopAllCoroutines();

        if(activeDialogue!=null && activeDialogue != this)
        {
            activeDialogue.StopAllCoroutines();
            activeDialogue.ExitDialogue();
        }

        activeDialogue = this;

        dialogueLocked = true;
        if (!Pause.isAnInterfaceActive)
        {
            Pause.isAnInterfaceActive = true;
        }
        playerHUD = o.GetComponent<ThirdPersonController>().playerHUDs;
        playerDialogueBox = o.GetComponent<ThirdPersonController>().dialogueTextBox;
        DBCharName = FindDescendants(playerDialogueBox.transform, "CharacterNameText").GetComponent<TextMeshProUGUI>();
        DBCharText = FindDescendants(playerDialogueBox.transform, "DialogueText").GetComponent<TextMeshProUGUI>();
        DBCharPFP = FindDescendants(playerDialogueBox.transform, "CharacterPFP").GetComponent<Image>();
        playerModel = o.GetComponent<ThirdPersonController>().playerModel;
        playerRotation = playerModel.rotation;

        playerHUD.SetActive(false);
        playerDialogueBox.SetActive(true);

        userInput = playerDialogueBox.GetComponent<DialogueInputHolder>().userInput;

        userInput.FindActionMap("Dialogue").Enable();
        continueText = userInput.FindAction("Continue Text");

        NextLine();
    }

    //Description to give player when they are hovering over interactable
    public string Description()
    {
        if (timesInteractedWith > 0 || firstTimeInteractionName.Length == 0)
        {
            return charName;
        }
        else
        {
            return firstTimeInteractionName;
        }
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

    public void NextLine()
    {
        //Debug.Log("Going to Line: " + index);
        if (index < dialogueLines.Length)
        {
            if (dialogueLines[index].appearOnlyInFirstInteraction && timesInteractedWith > 0)
            {
                while (index<dialogueLines.Length)
                {
                    if (dialogueLines[index].appearOnlyInFirstInteraction && timesInteractedWith > 0)
                    {
                        index++;
                        continue;
                    }
                    break;
                }
                if (index>=dialogueLines.Length)
                {
                    ExitDialogue();
                }
                else
                {
                    SayLine();
                }
            }
            else
            {
                SayLine();
            }
            index++;
        }
        else
        {
            ExitDialogue();
        }
    }

    public void SayLine()
    {
        ResetText();
        StopAllCoroutines();
        lockInput = true;
        StartCoroutine(Type());
    }

    public void ExitDialogue()
    {
        //Debug.Log("Exiting Dialogue");
        StopAllCoroutines();
        timesInteractedWith++;
        DBCharName.text = "";
        DBCharText.text = "";
        DBCharPFP.sprite = null;
        playerDialogueBox.SetActive(false);
        playerHUD.SetActive(true);
        playerModel.localRotation = Quaternion.identity;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, oldRotation, 5f);
        playerModel = null;
        transform.rotation = oldRotation;
        index = 0;
        userInput.FindActionMap("Dialogue").Disable();
        dialogueLocked = false;
        Pause.isAnInterfaceActive = false;
        if (activeDialogue == this)
        {
            activeDialogue = null;
        }
        lockInput = true;
        StartCoroutine(WaitForReleaseKeys());
    }

    public void ResetText()
    {
        DBCharName.text = dialogueLines[index].speakerName;
        DBCharText.text = "";
        if (dialogueLines[index].speakerPFP != null)
        {
            DBCharPFP.gameObject.SetActive(true);
            DBCharPFP.sprite = dialogueLines[index].speakerPFP;
        }
        else
        {
            DBCharPFP.gameObject.SetActive(false);
        }
    }

    public IEnumerator WaitForReleaseKeys()
    {
        var inputs = userInput.FindActionMap("GameSystem");

        bool IsAnyKeyPressed()
        {
            foreach (var action in inputs.actions)
            {
                if (action.IsPressed())
                {
                    return true;
                }
            }
            return false;
        }
        
        while (IsAnyKeyPressed()) yield return null;

        yield return null;

        lockInput = false;
    }

    public IEnumerator Type()
    {
        foreach(char c in dialogueLines[index].dialogue.ToCharArray())
        {
            DBCharText.text += c;
            yield return new WaitForSeconds(0.001f);
        }

        lockInput = false;
    }

    public void LookAt(Transform whoIsLooking,Transform target)
    {
        Quaternion q = Quaternion.LookRotation((target.position - whoIsLooking.position).normalized);
        q.x = 0;
        q.z = 0;
        whoIsLooking.rotation = Quaternion.RotateTowards(whoIsLooking.rotation, q, 5f);
    }

    public Transform FindDescendants(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            else if (FindDescendants(child, name) != null)
            {
                return FindDescendants(child, name);
            }
        }
        return null;
    }
}
