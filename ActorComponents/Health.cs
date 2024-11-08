using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DamagedEvent : UnityEvent<DamageEffects.DamageType> {}

public class Health : MonoBehaviour
{
    // config
    [Header("Config")]
    [SerializeField] float currentHealth = 100f;
    [SerializeField] GameplayAttribute maxHealth = new(100, 1, 0);
    [SerializeField] float hpTicksPerSecond = 1f;                   // health points regained per second
    [SerializeField] float hpRegenPerTick = 1f;
    [SerializeField] float hpRegenPerSecond;
    [SerializeField] bool regenOnStart = false;
    [SerializeField] [Range(0, 1)] float severeDamagePercentageOfMax = 0.3f;

    // state
    [Header("State")]
    [SerializeField] bool alive = true;
    [SerializeField] bool regenEnabled = false;

    [Header("Player")]
    [SerializeField] ValueBar healthBarValue;
    [SerializeField] ShakeBehavior shaker;
    [SerializeField] float shakeDuration = .5f;
    [SerializeField] float shakeMagnitudeMultiplier = .25f;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] impactSounds;
    [SerializeField] [Range(0, 1)] float impactVolume;

    [Header("Post-Hit Immunity")]
    [SerializeField] DamageImmunityFlash damageImmunityFlash;

    [Header("Health Bar")]
    [SerializeField] GameObject healthBarTemplate;
    [SerializeField] bool startWithHealthBarHidden = false;

    Canvas gameCanvas;

    // instantiated objects
    GameObject healthBarGameObject;
    UnitHealthBar healthBar;

    // events
    public UnityEvent damaged;
    public UnityEvent died;
    public UnityEvent restoredHealthToMax;
    public UnityEvent severelyDamaged;
    public UnityEvent healedSevereDamage;

    public DamagedEvent damagedWithType;

    // couroutines
    Coroutine hpRegen;

    public float CurrentHealth { get => currentHealth; }
    public GameplayAttribute MaxHealth { get => maxHealth; }
    public float HpTicksPerSecond { get => hpTicksPerSecond; set => hpTicksPerSecond = value; }
    public float HpRegenPerTick { get => hpRegenPerTick; set => hpRegenPerTick = value; }
    public float HpRegenPerSecond { get => hpRegenPerSecond; }
    public bool Alive { get => alive; set => alive = value; }

    private void Awake()
    {
        if (healthBarTemplate)
        {
            gameCanvas = GameObject.FindGameObjectWithTag("GameCanvas").GetComponent<Canvas>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SetBarValues();

        if (regenOnStart)
        {
            StartHPRegen();
        }

        // health bar above unit
        if (healthBarTemplate)
        {
            healthBarGameObject = Instantiate(healthBarTemplate, gameCanvas.transform);
            healthBarGameObject.GetComponent<UnitHealthBar>().SetParameters(transform, MaxHealth.GetCurrentValue(), CurrentHealth);
            healthBarValue = healthBarGameObject.GetComponent<ValueBar>();
            healthBar = healthBarGameObject.GetComponent<UnitHealthBar>();

            if (startWithHealthBarHidden)
            {
                healthBarGameObject.SetActive(false);
            }
        }
    }

    public void SetRegenPerSecond(float regenPerSecond)
    {
        hpRegenPerTick = regenPerSecond / hpTicksPerSecond;
        hpRegenPerSecond = regenPerSecond;
    }

    public void IncreaseRegenPerSecond(float regenPerSecond)
    {
        SetRegenPerSecond(hpRegenPerSecond + regenPerSecond);
    }

    public void DecreaseRegenPerSecond(float regenPerSecond)
    {
        SetRegenPerSecond(hpRegenPerSecond - regenPerSecond);
    }

    public void StartHPRegen()
    {
        // start if not already active
        if (!regenEnabled)
        {
            hpRegen = StartCoroutine(RegenHP());
            regenEnabled = true;
        }
    }

    public void FreezeHPRegen()
    {
        StopCoroutine(hpRegen);
        regenEnabled = false;
    }

    IEnumerator RegenHP()
    {
        // loop forever
        while (true)
        {
            // need more focus points
            if (CurrentHealth < MaxHealth.GetCurrentValue())
            {
                IncreaseHealth(hpRegenPerTick);
                yield return new WaitForSeconds(1f / hpTicksPerSecond);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void SetCurrentHealth(float hp)
    {
        currentHealth = hp;
        SetBarValues();
    }

    public void IncreaseHealth(float quantity)
    {
        if (CurrentHealth + quantity <= MaxHealth.GetCurrentValue())
        {
            currentHealth += quantity;
        }
        else
        {
            currentHealth = MaxHealth.GetCurrentValue();
        }
        
        // invoke events
        if (CurrentHealth >= MaxHealth.GetCurrentValue())
        {
            restoredHealthToMax.Invoke();

            if (healthBar)
            {
                healthBar.SetFullHealthColor();
            }
        }
        else if (CurrentHealth >= MaxHealth.GetCurrentValue() * severeDamagePercentageOfMax)
        {
            healedSevereDamage.Invoke();

            if (healthBar)
            {
                healthBar.SetDamagedColor();
            }
        }

        // player stuff
        if (healthBarValue)
        {
            healthBarValue.SetValue(CurrentHealth);
        }

        // Resurrection stuff
        if (!Alive && CurrentHealth > 0)
        {
            Alive = true;
        }
    }

    /* Returns true if died */
    public bool DecreaseHealth(float quantity, DamageEffects.DamageType damageType, bool playHitSound = true, bool shakeScreen = true, bool performImmunityFlash = true)
    {
        if (audioSource && CurrentHealth >= -5 && playHitSound)
        {
            PlayRandomSound(impactSounds, impactVolume);
        }

        currentHealth -= quantity;

        // player stuff
        if (healthBarValue)
        {
            healthBarValue.SetValue(CurrentHealth);

            if (shaker && shakeScreen)
            {
                shaker.ShakeCamera(quantity * shakeMagnitudeMultiplier, shakeDuration); // shake screen when player is damaged
            }

            if (damageImmunityFlash && performImmunityFlash)
            {
                damageImmunityFlash.TriggerImmunityFlash();
            }
        }

        damaged.Invoke();
        damagedWithType.Invoke(damageType);

        if (healthBar)
        {
            healthBar.SetDamagedColor();
        }

        // check if severely damaged
        if (CurrentHealth <= MaxHealth.GetCurrentValue() * severeDamagePercentageOfMax)
        {
            severelyDamaged.Invoke();

            if (healthBar)
            {
                healthBar.SetSeverelyDamagedColor();
            }
        }

        if (CurrentHealth <= 0 && Alive)
        {
            Alive = false;

            died.Invoke();

            return true;
        }

        return false;
    }

    public void IncreaseHealthByFractionOfMax(float fraction)
    {
        float val = MaxHealth.GetCurrentValue() * fraction;
        IncreaseHealth(val);
    }

    public float DecreaseHealthByFractionOfMax(float fraction, DamageEffects.DamageType damageType)
    {
        float val = MaxHealth.GetCurrentValue() * fraction;
        DecreaseHealth(val, damageType);

        return val;
    }

    public void SetMaxHealthBase(float max)
    {
        if (max < 0)
        {
            MaxHealth.BaseValue = 0;
        }
        else
        {
            MaxHealth.BaseValue = max;
        }

        ClampCurrentHealthToMax();

        SetBarValues();
    }

    public void IncreaseMaxHealthModifier(float mod)
    {
        MaxHealth.Modifier += mod;

        ClampCurrentHealthToMax();

        SetBarValues();
    }

    // Does NOT send full health events
    private void ClampCurrentHealthToMax()
    {
        if (CurrentHealth > MaxHealth.GetCurrentValue())
        {
            SetCurrentHealth(MaxHealth.GetCurrentValue());
        }
    }

    private void SetBarValues()
    {
        if (healthBarValue)
        {
            healthBarValue.SetMaxValue(MaxHealth.GetCurrentValue());
            healthBarValue.SetValue(CurrentHealth);
        }
    }

    public void HideHealthBar()
    {
        if (healthBar && healthBar.gameObject.activeInHierarchy)
        {
            healthBar.gameObject.SetActive(false);
        }
    }

    public void ShowHealthBar()
    {
        if (healthBar && !healthBar.gameObject.activeInHierarchy)
        {
            healthBar.gameObject.SetActive(true);
        }
    }

    private void PlayRandomSound(AudioClip[] clips, float volume)
    {
        int randomIndex = Random.Range(0, clips.Length);
        audioSource.PlayOneShot(clips[randomIndex], volume);
    }
}
