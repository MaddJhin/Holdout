using UnityEngine;
using System.Collections;

public class AnimEffects : MonoBehaviour
{
    public GameObject particleObj;
    public EnemyUnitControl unitControl;
    private ParticleSystem actionFX;                 // Stores the instance of the explosion Particle System
    private AudioSource actionAudio;

    void Start()
    {
        unitControl = GetComponentInParent<EnemyUnitControl>();

        if (particleObj)
            actionFX = particleObj.GetComponent<ParticleSystem>();

        actionAudio = unitControl.GetComponent<AudioSource>();
    }

    // Audio and Visual effects for punching
    void SlamFX()
    {
        actionAudio.clip = unitControl.unitAudio[1];
        actionAudio.Play();
        actionFX.Play(true);
    }

    // Audio and Visual effects for punching
    void PunchEffects()
    {
        actionAudio.clip = unitControl.unitAudio[1];
        actionAudio.Play();
    }

    void ShootEffects()
    {
        actionAudio.clip = unitControl.unitAudio[1];
        actionAudio.Play();
        unitControl.projectile.gameObject.SetActive(true);
        unitControl.LaunchProjectile();
    }
}
