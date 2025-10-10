using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Camera camera;
    private Transform parentTransform;
    private EnemyCharacter c;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera = Camera.main;
        parentTransform = transform.root;
        c = parentTransform.GetComponent<EnemyCharacter>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position-camera.transform.position);
    }
}
