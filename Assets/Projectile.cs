using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
{
    bool activated = false;
    Vector3 targetLoc;  
    Vector3 origin;
    float step;

    void Update()
    {
        if (activated)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetLoc, step);

            if (transform.position == targetLoc)
            {
                gameObject.SetActive(false);
                transform.position = origin;
            }

        }
    }

    public void FireProjectile(Vector3 projectileTarget, float projectileSpeed, Vector3 projectileOrigin)
    {
        activated = true;
        targetLoc = projectileTarget;
        step = projectileSpeed;
        origin = projectileOrigin;
    }
}
