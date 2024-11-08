using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    [SerializeField] float drag = 5f;

    [Header("Cached References")]
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D rb2d;
    [SerializeField] Health health;

    public void TriggerKnockback(Vector2 direction, float force, float knockbackTime)
    {
        if (health.Alive)
        {
            StartCoroutine(ReceiveKnockback(direction, force, knockbackTime));
        }
    }

    IEnumerator ReceiveKnockback(Vector2 direction, float force, float knockbackTime)
    {
        // animate
        if (animator)
        {
            animator.SetBool("AnimHit", true);
        }

        // add drag to rb2d
        rb2d.drag = drag;

        // stop movement
        UtilActor.FreezeMovement(gameObject);
        UtilActor.SetHit(gameObject, true);

        // add force
        if (rb2d)
        {
            rb2d.AddForce(direction * force, ForceMode2D.Impulse);
        }

        // wait
        yield return new WaitForSeconds(knockbackTime);

        // stop animating
        if (animator)
        {
            animator.SetBool("AnimHit", false);
        }

        // remove drag
        rb2d.drag = 0;

        // enable movement
        UtilActor.SetHit(gameObject, false);
    }
}
