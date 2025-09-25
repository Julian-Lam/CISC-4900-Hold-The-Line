using UnityEngine;

public class ParticleDestroy : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public GameObject parent;
    public Transform parentTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        parentTransform = transform.root;
        parent = parentTransform.gameObject;
        Invoke("DestroyIfParticleStop", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DestroyIfParticleStop()
    {
        Destroy(gameObject);
    }
}
