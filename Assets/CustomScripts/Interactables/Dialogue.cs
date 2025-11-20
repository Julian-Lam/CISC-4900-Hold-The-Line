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
    public static bool dialogueLocked;
    public static bool isDialogueCooldown;
    private float timesInteractedWith;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        oldRotation = transform.rotation;
        timesInteractedWith = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (userInput!=null && userInput.enabled && continueText.WasPressedThisFrame())
        {
            NextLine();
        }

        if (playerModel!=null && dialogueLocked)
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
        dialogueLocked = true;
        timesInteractedWith++;
        if (!Pause.isAnInterfaceActive)
        {
            Pause.isAnInterfaceActive = true;
        }
        playerHUD = o.GetComponent<ThirdPersonController>().playerHUDs;
        playerDialogueBox = o.GetComponent<ThirdPersonController>().dialogueTextBox;
        DBCharName = playerDialogueBox.transform.Find("CharacterNameText").GetComponent<TextMeshProUGUI>();
        DBCharText = playerDialogueBox.transform.Find("DialogueText").GetComponent<TextMeshProUGUI>();
        DBCharPFP = playerDialogueBox.transform.Find("CharacterPFP").GetComponent<Image>();
        playerModel = o.GetComponent<ThirdPersonController>().playerModel;

        playerHUD.SetActive(false);
        playerDialogueBox.SetActive(true);

        userInput = playerDialogueBox.GetComponent<DialogueInputHolder>().userInput;

        userInput.FindActionMap("Dialogue").Enable();
        continueText = userInput.FindAction("Continue Text");

        isDialogueCooldown = true;

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

    //The bottom is only worth messing with if the interaction requires holding.

    public bool CanHoldInteract()
    {
        return false;
    }

    //If tap-interact, always return true
    public bool Release()
    {
        return true;
    }

    //If tap-interact, leave empty
    public void ReleaseAction()
    {

    }

    public void NextLine()
    {
        //Debug.Log("Going to Line: " + index);
        if (index < dialogueLines.Length)
        {
            if (dialogueLines[index].appearOnlyInFirstInteraction && timesInteractedWith > 1)
            {
                while (index<dialogueLines.Length)
                {
                    if (dialogueLines[index].appearOnlyInFirstInteraction && timesInteractedWith > 1)
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
        StartCoroutine(Type());
    }

    public void ExitDialogue()
    {
        //Debug.Log("Exiting Dialogue");
        playerDialogueBox.SetActive(false);
        playerHUD.SetActive(true);
        if (playerModel != null)
        {
            playerModel.localRotation = Quaternion.identity;
        }
        playerModel = null;
        transform.rotation = oldRotation;
        index = 0;
        StopAllCoroutines();
        StartCoroutine(CooldownDialogue());
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

    public IEnumerator Type()
    {
        foreach(char c in dialogueLines[index].dialogue.ToCharArray())
        {
            DBCharText.text += c;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public IEnumerator CooldownDialogue()
    {
        userInput.FindActionMap("Dialogue").Disable();
        yield return new WaitForSeconds(0.5f);
        isDialogueCooldown = false;
        dialogueLocked = false;
        Pause.isAnInterfaceActive = false;
    }

    public void LookAt(Transform whoIsLooking,Transform target)
    {
        Quaternion q = Quaternion.LookRotation((target.position - whoIsLooking.position).normalized);
        q.x = 0;
        q.z = 0;
        whoIsLooking.rotation = Quaternion.RotateTowards(whoIsLooking.rotation, q, 5f);
    }
}
