using UnityEngine;

public class AirSupport : MonoBehaviour
{
    public float timeUntilDisappearance = 10.5f;
    public AudioClip planeSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioSource.PlayClipAtPoint(planeSound, transform.position, 0.75f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * 25 * Time.deltaTime);
        timeUntilDisappearance -= Time.deltaTime;
        if (timeUntilDisappearance <= 0)
        {
            Destroy(gameObject);
        }
    }
}
