using UnityEngine;
using System.Collections;

public class AnimEffects : MonoBehaviour
{
    public GameObject particleObj;
    private ParticleSystem actionFX;                 // Stores the instance of the explosion Particle System

    void Start()
    {
        actionFX = particleObj.GetComponent<ParticleSystem>();
    }

    // Audio and Visual effects for selfDestruct
    void DestructEffects()
    {
    }

    // Audio and Visual effects for punching
    void SlamFX()
    {
        Debug.Log("Playing Slam FX");
        actionFX.Play(true);
    }

    // Audio and Visual effects for punching
    void PunchEffects()
    {

    }
}
