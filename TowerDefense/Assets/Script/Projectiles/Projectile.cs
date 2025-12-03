using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public float maxLifeTime = 6f;

    TD.Enemy target;
    int damage;

    bool appliesBurn;
    int burnDPS;
    float burnDuration;

    bool appliesChill;
    float chillAmount;
    float chillDuration;

    bool hasSplash;
    float splashRadius;
    int splashDamage;

    float lifeTimer = 0f;

    public void Initialize(
        TD.Enemy target,
        int damage,
        bool appliesBurn, int burnDPS, float burnDuration,
        bool appliesChill, float chillAmount, float chillDuration,
        bool hasSplash, float splashRadius, int splashDamage,
        bool pooled = false)
    {
        this.target = target;
        this.damage = damage;
        this.appliesBurn = appliesBurn;
        this.burnDPS = burnDPS;
        this.burnDuration = burnDuration;
        this.appliesChill = appliesChill;
        this.chillAmount = chillAmount;
        this.chillDuration = chillDuration;
        this.hasSplash = hasSplash;
        this.splashRadius = splashRadius;
        this.splashDamage = splashDamage;

        lifeTimer = 0f;
        gameObject.SetActive(true);
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.transform.position - transform.position);
        float distThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distThisFrame)
        {
            OnHit();
            return;
        }

        transform.Translate(dir.normalized * distThisFrame, Space.World);
        transform.LookAt(target.transform);
    }

    void OnHit()
    {
        if (hasSplash)
        {
            SplashDamage.ApplySplash(transform.position, splashRadius, splashDamage,
                appliesBurn, burnDPS, burnDuration,
                appliesChill, chillAmount, chillDuration);
        }
        else
        {
            if (target != null)
            {
                target.TakeDamage(damage);
                if (appliesBurn) target.ApplyBurn(burnDPS, burnDuration);
                if (appliesChill) target.ApplyChill(chillAmount, chillDuration);
            }
        }

        Destroy(gameObject);
    }

    void OnDisable() { target = null; }
}
