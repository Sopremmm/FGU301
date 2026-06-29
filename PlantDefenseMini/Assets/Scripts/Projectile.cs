using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f; // Tốc độ bay của projectile
    [SerializeField] private float hitDistance = 0.05f; // Khoảng cách tính là chạm target

    private UnitController target; // Target projectile đang bay tới
    private int damage; // Sát thương projectile gây ra

    public void Init(UnitController target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null || target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.transform.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.transform.position) <= hitDistance)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
