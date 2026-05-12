using UnityEngine;

/// <summary>
/// 유닛이 발사하는 투사체.
/// </summary>
public class Projectile : MonoBehaviour
{
    private Transform target;
    private float speed = 10f;
    private float lifetime = 3f;

    public void Initialize(Transform targetTransform, float projectileSpeed)
    {
        target = targetTransform;
        speed = projectileSpeed;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            Destroy(gameObject);
        }
    }
}
