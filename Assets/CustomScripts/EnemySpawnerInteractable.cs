using UnityEngine;

public class EnemySpawnerInteractable : MonoBehaviour, Interactable
{
    public EnemyCharacter[] enemiesToSpawn;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact(GameObject o)
    {
        float RandomX = Random.Range(-4, 4);
        float RandomZ = Random.Range(-4, 4);

        int index = Random.Range(0, enemiesToSpawn.Length);
        GameObject newEnemy = Instantiate(enemiesToSpawn[index].gameObject, transform.position + new Vector3(RandomX, 0, RandomZ), Quaternion.identity);
    }

    public string Description()
    {
        return "Spawn random enemy.";
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
}
