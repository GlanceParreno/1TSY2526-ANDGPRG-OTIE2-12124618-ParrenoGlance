using UnityEngine;
using System.Collections.Generic;

public static class SplashDamage
{
    public static void ApplySplash(Vector3 transformPosition, float radius, int damage,
                                   bool appliesBurn, int burnDPS, float burnDuration,
                                   bool appliesChill, float chillAmount, float chillDuration)
    {
        Collider[] hits = Physics.OverlapSphere(transformPosition, radius);
        List<TD.Enemy> found = new List<TD.Enemy>();
        foreach (var c in hits)
        {
            var e = c.GetComponent<TD.Enemy>();
            if (e != null)
            {

                if (damage > 0) e.TakeDamage(damage);


                if (appliesBurn) e.ApplyBurn(burnDPS, burnDuration);
                if (appliesChill) e.ApplyChill(chillAmount, chillDuration);
            }
        }
    }
}
