using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using System.Collections;

public class StartingText : MonoBehaviour
{

    public string textToDisplay;
    public GameObject displayObject;
    public TextMeshProUGUI textBox;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Type());
    }

    // Update is called once per frame
    void Update()
    {
        if (Pause.isAnInterfaceActive)
        {
            displayObject.SetActive(false);
            StopAllCoroutines();
        }
    }

    public IEnumerator Type()
    {
        foreach (char c in textToDisplay.ToCharArray())
        {
            textBox.text += c;
            yield return new WaitForSeconds(0.001f);
        }
        StartCoroutine(DeleteText());
    }

    public IEnumerator DeleteText()
    {
        yield return new WaitForSeconds(3f);

        while(textBox.text.Length>0)
        {
            textBox.text = textBox.text.Remove(textBox.text.Length - 1);
            yield return new WaitForSeconds(0.001f);
        }

        displayObject.SetActive(false);
    }
}
