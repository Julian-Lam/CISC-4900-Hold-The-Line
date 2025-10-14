using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Camera cam;
    private Transform parentTransform;
    private EnemyCharacter c;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
        parentTransform = transform.root;
        c = parentTransform.GetComponent<EnemyCharacter>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position-cam.transform.position);
    }
}
