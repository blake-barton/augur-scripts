using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Poise : MonoBehaviour
{
    // config
    [Header("Config")]
    [SerializeField] Animator animator;
    [SerializeField] float poise = 100f;
    [SerializeField] GameplayAttribute maxPoise = new(100, 1, 0);
    [SerializeField] float poiseRegenTicksPerSecond = 1f;                   // health points regained per second
    [SerializeField] float poiseRegenPerTick = 1f;
    [SerializeField] float timeKnockedDown = 1f;

    // state
    [Header("State")]
    [SerializeField] bool regenEnabled = false;

    [Header("Effects")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] poiseBreakSounds;
    [SerializeField] [Range(0, 1)] float poiseBreakVolume;

    [Header("Collision")]
    Vector2 collidingObjectDirection;

    [Header("Player")]
    [SerializeField] Player player;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] ValueBar poiseBar;

    [Header("Enemy")]
    [SerializeField] Enemy enemy;
    [SerializeField] EnemyMovement enemyMovement;
    [SerializeField] EnemyEquipment enemyEquipment;

    [Header("Events")]
    public UnityEvent knockedDown;

    public void SetCollidingObjectDirection(Vector2 direction)
    {
        collidingObjectDirection = -direction;
    }

    // couroutines
    Coroutine poiseRegen;

    public float CurrentPoise { get => poise; }
    public GameplayAttribute MaxPoise { get => maxPoise; }
    public float PoiseRegenTicksPerSecond { get => poiseRegenTicksPerSecond; set => poiseRegenTicksPerSecond = value; }
    public float PoiseRegenPerTick { get => poiseRegenPerTick; set => poiseRegenPerTick = value; }

    // Start is called before the first frame update
    void Start()
    {
        SetBarValues();
        StartPoiseRegen();
    }

    public void StartPoiseRegen()
    {
        // start if not already active
        if (!regenEnabled)
        {
            poiseRegen = StartCoroutine(RegenPoise());
            regenEnabled = true;
        }
    }

    public void FreezePoiseRegen()
    {
        StopCoroutine(poiseRegen);
        regenEnabled = false;
    }

    IEnumerator RegenPoise()
    {
        // loop forever
        while (true)
        {
            // need more poise
            if (CurrentPoise < maxPoise.GetCurrentValue())
            {
                IncreasePoise(poiseRegenPerTick);
                yield return new WaitForSeconds(1f / poiseRegenTicksPerSecond);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void IncreasePoise(float quantity)
    {
        if (poise + quantity <= maxPoise.GetCurrentValue())
        {
            poise += quantity;
        }
        else
        {
            poise = maxPoise.GetCurrentValue();
        }

        // player stuff
        if (poiseBar)
        {
            poiseBar.SetValue(poise);
        }
    }

    public void DecreasePoise(float quantity)
    {
        // only subtract if there is poise to decrease
        if (poise > 0)
            poise -= quantity;

        // player stuff
        if (poiseBar)
        {
            poiseBar.SetValue(poise);
        }

        // knockdown
        if (poise <= 0)
        {
            // keep it at 0
            poise = 0;

            // enemy
            if (enemy)
            {
                if (!enemy.Character.Incapacitated)
                {
                    StartCoroutine(KnockdownAI());
                }
            }
            else
            {
                if (!player.Character.Incapacitated)
                {
                    StartCoroutine(KnockDownPlayer());
                }
            }
        }
    }

    IEnumerator KnockDownPlayer()
    {
        // set incapacitated
        player.Character.Incapacitated = true;

        // freeze poise regen
        FreezePoiseRegen();

        // stop movement
        playerMovement.FreezeMovement();

        // hide items
        playerMovement.HideEquipped();

        // stop dust trail
        playerMovement.StopDustTrail();

        // animate
        animator.SetBool("AnimKnockedDown", true);
        animator.SetFloat("AnimImpactX", collidingObjectDirection.x);
        animator.SetFloat("AnimImpactY", collidingObjectDirection.y);

        // wait for knockdown time
        yield return new WaitForSeconds(timeKnockedDown);

        // begin standing
        animator.SetBool("AnimKnockedDown", false);
        animator.SetBool("AnimStandingUp", true);
    }

    IEnumerator KnockdownAI()
    {
        // set incapacitated
        enemy.Character.Incapacitated = true;

        // can only be hit by projectiles
        gameObject.layer = enemy.KnockedDownLayer;

        // stop movement
        enemyMovement.FreezeMovement();

        // freeze poise regen
        FreezePoiseRegen();

        if (enemyEquipment && enemyEquipment.HasWeapons())
        {
            enemyEquipment.HideEquipped();
        }

        // animate
        animator.SetBool("AnimKnockedDown", true);
        animator.SetFloat("AnimImpactX", collidingObjectDirection.x);
        animator.SetFloat("AnimImpactY", collidingObjectDirection.y);

        // Event
        knockedDown.Invoke();

        // wait for knockdown time
        yield return new WaitForSeconds(timeKnockedDown);

        // begin standing
        animator.SetBool("AnimKnockedDown", false);
        animator.SetBool("AnimStandingUp", true);
    }

    public void RefillPoiseToMax()
    {
        poise = maxPoise.GetCurrentValue();

        // player stuff
        if (poiseBar)
        {
            poiseBar.SetValue(poise);
        }
    }

    public void SetMaxPoiseBase(float max)
    {
        if (max < 0)
        {
            maxPoise.BaseValue = 0;
        }
        else
        {
            maxPoise.BaseValue = max;
        }

        SetBarValues();
    }

    public void IncreaseMaxPoiseBase(float val, bool changeCurrentPoise)
    {
        maxPoise.BaseValue += val;

        if (changeCurrentPoise)
        {
            if (poise < maxPoise.GetCurrentValue())
            {
                poise += val;
            }
        }

        SetBarValues();
    }

    public void DecreaseMaxPoiseBase(float val, bool changeCurrentPoise)
    {
        maxPoise.BaseValue -= val;

        if (changeCurrentPoise)
        {
            if (poise > maxPoise.GetCurrentValue())
            {
                poise -= val;
            }
        }

        SetBarValues();
    }

    public void IncreaseMaxPoiseModifier(float val)
    {
        maxPoise.Modifier += val;

        if (poise > maxPoise.GetCurrentValue())
        {
            poise = MaxPoise.GetCurrentValue();
        }

        SetBarValues();
    }

    private void SetBarValues()
    {
        if (poiseBar)
        {
            poiseBar.SetMaxValue(maxPoise.GetCurrentValue());
            poiseBar.SetValue(CurrentPoise);
        }
    }
}
