using UnityEngine;
using System.Collections;

public class BruteStun : MonoBehaviour {

    [SerializeField] public float damage = 5f;
    [SerializeField] public AudioClip attackSound;
    [SerializeField] public float duration = 3f;
    [SerializeField] public float radius = 5f;

    private Animator anim;
 //   private UnitStats targetHealth;
    public bool attacking;
    private float attackRange;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {

    }

    public void Slam(GameObject target)
    {
        attacking = true;

        AreaOfEffect aoe = new AreaOfEffect();
        aoe.AreaStun(target.transform.position, radius, damage, duration, gameObject);

        SlamEffects();
        attacking = false;
        Debug.Log("Slam!");
    }

    // Audio and Visual effects for punching
    void SlamEffects()
    {

    }
}
