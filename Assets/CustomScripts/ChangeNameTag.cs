using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChangeNameTag : MonoBehaviour
{

    public Dialogue characterDialogue;
    private TextMeshProUGUI nameTag;
    public string newName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nameTag = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (characterDialogue.timesInteractedWith > 0)
        {
            nameTag.text = newName;
        }
    }
}
