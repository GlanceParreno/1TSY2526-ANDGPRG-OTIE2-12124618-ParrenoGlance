using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Canvas))]
public class EnemyHealthBar : MonoBehaviour
{
    public TD.Enemy enemy;
    public Image fillImage;
    public TMP_Text healthText;
    public Vector3 offset = new Vector3(0f, 1.6f, 0f);
    public bool faceCamera = true;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (fillImage == null) Debug.LogWarning("[EnemyHealthBar] fillImage unassigned.");
    }

    void Update()
    {
        if (enemy == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = enemy.transform.position + offset;
        if (faceCamera && cam != null) transform.forward = cam.transform.forward;

        int cur = enemy.CurrentHealth();
        int max = GetEnemyMaxHealthSafe(enemy);
        float percent = (max > 0) ? Mathf.Clamp01((float)cur / (float)max) : 0f;

        if (fillImage != null) fillImage.fillAmount = percent;
        if (healthText != null) healthText.text = $"{cur}";
    }

    int GetEnemyMaxHealthSafe(TD.Enemy e)
    {
        var t = e.GetType();
        var fi = t.GetField("maxHealth");
        if (fi != null) { object v = fi.GetValue(e); if (v is int) return (int)v; }
        var pi = t.GetProperty("maxHealth");
        if (pi != null) { object v = pi.GetValue(e); if (v is int) return (int)v; }
        return 1;
    }

    public void AttachTo(TD.Enemy e)
    {
        enemy = e;
    }
}
