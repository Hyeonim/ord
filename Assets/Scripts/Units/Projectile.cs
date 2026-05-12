using UnityEngine;

/// <summary>
/// 유닛이 발사하는 투사체.
/// 타겟을 추적하며 도달 시 데미지를 준다.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("설정")]
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 5f;

    private Transform target;
    private Vector3 lastTargetPos;

    public void Initialize(Transform target, float damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        this.lastTargetPos = target != null ? target.position : transform.position + transform.forward * 10f;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        Vector3 targetPos = target != null ? target.position : lastTargetPos;

        if (target != null)
            lastTargetPos = target.position;

        Vector3 direction = (targetPos - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(targetPos);

        // 도달 체크
        if (Vector3.Distance(transform.position, targetPos) < 0.3f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (target != null)
        {
            // 적에게 데미지
            EnemyController enemy = target.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // 스토리 보스에게 데미지
            StoryBoss boss = target.GetComponent<StoryBoss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}
