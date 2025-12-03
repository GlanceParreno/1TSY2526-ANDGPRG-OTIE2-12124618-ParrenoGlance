
using UnityEngine;
using System.Collections;

namespace TD
{
    [RequireComponent(typeof(Collider))]
    public class CoreBase : MonoBehaviour
    {
        [Header("Feedback")]
        public ParticleSystem hitVfx;
        public AudioClip hitSfx;
        public float flashDuration = 0.12f;


        Renderer[] renderers;
        Color[] originalColors;

        void Awake()
        {

            renderers = GetComponentsInChildren<Renderer>(true);
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {

                try { originalColors[i] = renderers[i].material.color; }
                catch { originalColors[i] = Color.white; }
            }


            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;


            TD.UI.UIManagerTMP.Instance?.UpdateLives(TD.GameManager.Instance != null ? TD.GameManager.Instance.coreLives : 0);
        }

        void OnTriggerEnter(Collider other)
        {
            var enemy = other.GetComponent<Enemy>();
            if (enemy == null) return;

            int damage = enemy.isBoss ? Mathf.Max(enemy.damageToCore * 2, 10) : enemy.damageToCore;


            TD.GameManager.Instance.CoreTakeDamage(damage);


            TD.GameManager.Instance.NotifyEnemyDeath(enemy);


            Destroy(other.gameObject);

            PlayHitEffects();
        }

        void PlayHitEffects()
        {
            if (hitVfx != null)
            {
                var fx = Instantiate(hitVfx, transform.position, Quaternion.identity);
                fx.Play();
                Destroy(fx.gameObject, 3f);
            }

            if (hitSfx != null)
            {
                AudioSource.PlayClipAtPoint(hitSfx, transform.position);
            }

            StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                try { renderers[i].material.color = Color.red; } catch { }
            }

            yield return new WaitForSeconds(flashDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                try { renderers[i].material.color = originalColors[i]; } catch { }
            }
        }
    }
}
