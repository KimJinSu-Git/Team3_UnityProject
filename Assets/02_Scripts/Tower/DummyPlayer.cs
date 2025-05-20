using UnityEngine;

public class DummyPlayer : MonoBehaviour, IDamageAble
{
    public int maxHealth = 500;
    private int currentHealth;

    public GameObject GameObject => this.gameObject;
    public Collider Collider { get; private set; }

    private void Start()
    {
        currentHealth = maxHealth;
        Collider = GetComponent<Collider>();

        Debug.Log("DummyUnit 등록됨");
        CombatSystem.Instance.RegisterCreature(Collider, this);
    }

    public void TakeDamage(int damage, bool showEffect)
    {
        currentHealth -= damage;
        Debug.Log($"DummyUnit 피격! 데미지: {damage}, 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("DummyUnit 파괴됨");
            Destroy(gameObject);
        }
    }
}
