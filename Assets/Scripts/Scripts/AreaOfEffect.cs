using UnityEngine;
using System.Collections;

public class AreaOfEffect : MonoBehaviour
{
    private int colliderIndex;
    private UnitStats cache;

    public void AreaStun(Vector3 center, float radius, float damage, float duration, GameObject source, int attackMask)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius, attackMask);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            cache = hitColliders[i].gameObject.GetComponent<UnitStats>();
            cache.TakeDamage(damage);

            if (!cache.stunImmunity)
                cache.ApplyStatus(UnitStats.statusEffects.stun, duration);
        }
    }

    public void AreaExplode(Vector3 center, float radius, float damage, GameObject source, int attackMask)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius, attackMask);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            cache = hitColliders[i].gameObject.GetComponent<UnitStats>();
            cache.TakeDamage(damage);
        }
    }

}
