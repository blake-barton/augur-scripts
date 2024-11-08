using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageImmunityFlash : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float immunityTime = 1.25f;
    [SerializeField] float flashTime = 0.2f;
    [SerializeField] [Range(0, 32)] int vulnerableLayer;
    [SerializeField] [Range(0, 32)] int immuneLayer;

    Coroutine immunityFlash;

    public void TriggerImmunityFlash()
    {
        immunityFlash = StartCoroutine(ImmunityFlash());
    }

    IEnumerator ImmunityFlash()
    {
        gameObject.layer = immuneLayer;

        float endTime = Time.time + immunityTime;

        while (Time.time <= endTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(flashTime);
        }

        spriteRenderer.enabled = true;
        gameObject.layer = vulnerableLayer;
    }

    public void StopImmunityFlash()
    {
        if (immunityFlash != null)
        {
            StopCoroutine(immunityFlash);
        }

        spriteRenderer.enabled = true;
        gameObject.layer = vulnerableLayer;
    }
}
