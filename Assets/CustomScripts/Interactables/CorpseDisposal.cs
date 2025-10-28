using UnityEngine;

public class CorpseDisposal : MonoBehaviour, Interactable
{
    private EnemyCharacter deadEnemy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        deadEnemy = GetComponent<EnemyCharacter>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Interact(GameObject o)
    {
        EnemyCharacter.enemyCorpseList.Remove(deadEnemy);
        deadEnemy.DropLoot();
        Destroy(gameObject);
    }

    public string Description()
    {
        return "Loot corpse";
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
