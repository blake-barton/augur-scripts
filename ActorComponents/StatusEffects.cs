using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusEffects : MonoBehaviour
{
    [SerializeField] List<StatusEffect> statusEffects = new List<StatusEffect>();

    [Header("Damage Config")]
    [SerializeField] [Range(0, 1)] float statusEffectDamagePercentage = .1f;

    [Header("Buildup Status Effects")]
    [SerializeField] StatusEffect burningStatusEffect;
    [SerializeField] StatusEffect poisonedStatusEffect;
    [SerializeField] StatusEffect frostStatusEffect;
    [SerializeField] StatusEffect decayStatusEffect;
    [SerializeField] StatusEffect insanityStatusEffect;

    [Header("Thresholds")]
    [SerializeField] float burningThreshold = 100f;
    [SerializeField] float frostThreshold = 100f;
    [SerializeField] float decayThreshold;      // acid
    [SerializeField] float shockedThreshold;    // shock
    [SerializeField] float insanityThreshold;   // psionic
    [SerializeField] float poisonedThreshold;   // poison
    [SerializeField] float bleedThreshold;      // bleed
    [SerializeField] float bleedReduction;      // bleed damage reduction

    [Header("Current Buildup")]
    [SerializeField] float fireBuildup = 0;
    [SerializeField] float poisonBuildup = 0;
    [SerializeField] float frostBuildup = 0;
    [SerializeField] float decayBuildup;      // acid
    [SerializeField] float shockBuildup;      // shock
    [SerializeField] float insanityBuildup;   // psionic
    [SerializeField] float bleedBuildup;      // bleed

    [Header("Buildup Decrease Rates")]
    [SerializeField] float buildupDecreaseTicksPerSecond = 10;
    [SerializeField] float fireDecreasePerSecond = 20f;
    [SerializeField] float poisonDecreasePerSecond = 20f;
    [SerializeField] float frostDecreasePerSecond = 20f;
    [SerializeField] float bleedDecreasePerSecond = 20f;
    [SerializeField] float shockDecreasePerSecond = 20f;
    [SerializeField] float decayDecreasePerSecond = 20f;
    [SerializeField] float insanityDecreasePerSecond = 20f;

    [Header("Impact Effects")]
    [SerializeField] GameObject actorImpactEffect;
    [SerializeField] GameObject actorCritImpactEffect;
    [SerializeField] [Range(0, 31)] int enemyLayer = 9;
    [SerializeField] [Range(0, 31)] int playerLayer = 6;
    [SerializeField] GameObject wallImpactEffect;
    [SerializeField] [Range(0, 31)] int wallLayer = 10;

    [Header("Visual Effects")]
    [SerializeField] GameObject bleedingFX;
    [SerializeField] float timeToDestroyBleedingFX = 10f;
    [SerializeField] GameObject chainLightningPoint;
    [SerializeField] LayerMask chainLightningLayersToCollideWith;
    [SerializeField] float chainLightningVisualDuration = .2f;

    [Header("Sound Effects")]
    [SerializeField] AudioClip[] bloodLossSoundEffects;
    [SerializeField] [Range(0, 1)] float bloodLossSoundEffectVolume = 1f;
    [SerializeField] AudioClip[] shockedSoundEffects;
    [SerializeField] [Range(0, 1)] float shockedSoundEffectVolume = 1f;

    [Header("Floating Text")]
    [SerializeField] GameObject floatingText;

    [Header("State")]
    [SerializeField] bool decreasingFireBuildup;
    [SerializeField] bool decreasingPoisonBuildup;
    [SerializeField] bool decreasingFrostBuildup;
    [SerializeField] bool decreasingBleedBuildup;
    [SerializeField] bool decreasingShockBuildup;
    [SerializeField] bool decreasingDecayBuildup;
    [SerializeField] bool decreasingInsanityBuildup;
    [SerializeField] bool chainLightningEnabled;
    [SerializeField] bool chainLightningColliderHit;

    [Header("Player")]
    [SerializeField] Player player;
    [SerializeField] HUDAmmoText hudElements;
    PlayerMovement playerMovement;

    [Header("Enemy")]
    [SerializeField] Enemy enemy;
    EnemyMovement enemyMovement;

    // Instantiated status effects
    StatusEffect instantiatedBurningStatusEffect;
    StatusEffect instantiatedPoisonStatusEffect;
    StatusEffect instantiatedFrozenStatusEffect;
    StatusEffect instantiatedDecayStatusEffect;
    StatusEffect instantiatedInsanityStatusEffect;

    // chain lightning util
    Vector2 chainLightningFireDirection = new Vector2();
    Vector2 chainLightningHitPoint = new Vector2();
    float shockDamage;

    // cached references
    DamageEffects damageEffects;
    Health health;
    Poise poise;
    Animator animator;
    AudioSource audioSource;
    LineRenderer chainLightningLineRenderer;
    DamageDealer chainLightningDamageDealer;
    DamageThresholds damageThresholds;
    Character character;
    LineOfSight lineOfSight;

    public float FireBuildup
    {
        get => fireBuildup;
        set
        {
            fireBuildup = value;

            if (FireBuildup >= BurningThreshold && !statusEffects.Contains(instantiatedBurningStatusEffect))
            {
                if (!damageEffects.CheckResistanceOrImmunity(DamageEffects.DamageType.FIRE))
                {
                    instantiatedBurningStatusEffect = Instantiate(burningStatusEffect);

                    AddStatusEffect(instantiatedBurningStatusEffect, gameObject);
                }
            }
            else if (FireBuildup < BurningThreshold && statusEffects.Contains(instantiatedBurningStatusEffect) && IsAlive())
            {
                RemoveStatusEffect(instantiatedBurningStatusEffect);
            }

            // decrease
            if (FireBuildup > 0 && !decreasingFireBuildup)
            {
                StartCoroutine(DecreaseFireBuildup());
            }
        }
    }

    public float PoisonBuildup
    {
        get => poisonBuildup;
        set
        {
            poisonBuildup = value;

            if (PoisonBuildup >= PoisonedThreshold && !statusEffects.Contains(instantiatedPoisonStatusEffect))
            {
                if (!damageEffects.CheckResistanceOrImmunity(DamageEffects.DamageType.POISON))
                {
                    instantiatedPoisonStatusEffect = Instantiate(poisonedStatusEffect);

                    AddStatusEffect(instantiatedPoisonStatusEffect, gameObject);
                }
            }
            else if (PoisonBuildup < PoisonedThreshold && statusEffects.Contains(instantiatedPoisonStatusEffect) && IsAlive())
            {
                RemoveStatusEffect(instantiatedPoisonStatusEffect);
            }

            // decrease
            if (PoisonBuildup > 0 && !decreasingPoisonBuildup)
            {
                StartCoroutine(DecreasePoisonBuildup());
            }
        }
    }

    public float FrostBuildup
    { 
        get => frostBuildup; 
        set
        {
            frostBuildup = value;

            if (FrostBuildup >= FrostThreshold && !statusEffects.Contains(instantiatedFrozenStatusEffect))
            {
                if (!damageEffects.CheckResistanceOrImmunity(DamageEffects.DamageType.FROST))
                {
                    instantiatedFrozenStatusEffect = Instantiate(frostStatusEffect);

                    AddStatusEffect(instantiatedFrozenStatusEffect, gameObject);
                }
            }
            else if (FrostBuildup < FrostThreshold && statusEffects.Contains(instantiatedFrozenStatusEffect) && IsAlive())
            {
                RemoveStatusEffect(instantiatedFrozenStatusEffect);
            }

            // decrease
            if (frostBuildup > 0 && !decreasingFrostBuildup)
            {
                StartCoroutine(DecreaseFrostBuildup());
            }
        }
    }

    public float BleedBuildup
    {
        get => bleedBuildup;
        set
        {
            bleedBuildup = value;

            if (BleedBuildup >= BleedThreshold)
            {
                BloodLoss();
            }

            // decrease
            if (bleedBuildup > 0 && !decreasingBleedBuildup)
            {
                StartCoroutine(DecreaseBleedBuildup());
            }
        }
    }

    public float ShockBuildup
    {
        get => shockBuildup;
        set
        {
            shockBuildup = value;

            if (ShockBuildup >= ShockedThreshold)
            {
                Shocked();
            }

            // decrease
            if (shockBuildup > 0 && !decreasingShockBuildup)
            {
                StartCoroutine(DecreaseShockBuildup());
            }
        }
    }

    public float DecayBuildup
    {
        get => decayBuildup;
        set
        {
            decayBuildup = value;

            if (DecayBuildup >= DecayThreshold && !statusEffects.Contains(instantiatedDecayStatusEffect))
            {
                if (!damageEffects.CheckResistanceOrImmunity(DamageEffects.DamageType.ACID))
                {
                    instantiatedDecayStatusEffect = Instantiate(decayStatusEffect);

                    AddStatusEffect(instantiatedDecayStatusEffect, gameObject);
                }
            }
            else if (DecayBuildup < DecayThreshold && statusEffects.Contains(instantiatedDecayStatusEffect) && IsAlive())
            {
                RemoveStatusEffect(instantiatedDecayStatusEffect);
            }

            // decrease
            if (decayBuildup > 0 && !decreasingDecayBuildup)
            {
                StartCoroutine(DecreaseDecayBuildup());
            }
        }
    }

    public float InsanityBuildup
    {
        get => insanityBuildup;
        set
        {
            insanityBuildup = value;

            if (InsanityBuildup >= InsanityThreshold && !statusEffects.Contains(instantiatedInsanityStatusEffect))
            {
                if (!damageEffects.CheckResistanceOrImmunity(DamageEffects.DamageType.PSIONIC) && insanityStatusEffect)
                {
                    instantiatedInsanityStatusEffect = Instantiate(insanityStatusEffect);

                    AddStatusEffect(instantiatedInsanityStatusEffect, gameObject);
                }
            }
            else if (InsanityBuildup < InsanityThreshold && statusEffects.Contains(instantiatedInsanityStatusEffect) && IsAlive())
            {
                RemoveStatusEffect(instantiatedInsanityStatusEffect);
            }

            // decrease
            if (InsanityBuildup > 0 && !decreasingInsanityBuildup)
            {
                StartCoroutine(DecreaseInsanityBuildup());
            }
        }
    }

    public float BurningThreshold { get => burningThreshold; set => burningThreshold = value; }
    public float FrostThreshold { get => frostThreshold; set => frostThreshold = value; }
    public float DecayThreshold { get => decayThreshold; set => decayThreshold = value; }
    public float ShockedThreshold { get => shockedThreshold; set => shockedThreshold = value; }
    public float InsanityThreshold { get => insanityThreshold; set => insanityThreshold = value; }
    public float PoisonedThreshold { get => poisonedThreshold; set => poisonedThreshold = value; }
    public float BleedThreshold { get => bleedThreshold; set => bleedThreshold = value; }
    public float BleedReduction { get => bleedReduction; set => bleedReduction = value; }
    public Animator ActorAnimator { get => animator; set => animator = value; }
    public PlayerMovement ActorPlayerMovement { get => playerMovement; }
    public DamageThresholds ActorDamageThresholds { get => damageThresholds; }
    public Character ActorCharacter { get => character; set => character = value; }
    public Player ActorPlayer { get => player; set => player = value; }
    public LineOfSight ActorLineOfSight { get => lineOfSight; set => lineOfSight = value; }
    public Enemy ActorEnemy { get => enemy; set => enemy = value; }

    // Start is called before the first frame update
    void Awake()
    {
        damageEffects = GetComponent<DamageEffects>();
        health = GetComponent<Health>();
        poise = GetComponent<Poise>();
        ActorAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        ActorCharacter = GetComponent<Character>();
        lineOfSight = GetComponent<LineOfSight>();

        if (chainLightningPoint)
        {
            chainLightningDamageDealer = chainLightningPoint.GetComponent<DamageDealer>();
            chainLightningLineRenderer = chainLightningPoint.GetComponent<LineRenderer>();
        }

        damageThresholds = GetComponent<DamageThresholds>();

        if (ActorPlayer)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
        else if (ActorEnemy)
        {
            enemyMovement = GetComponent<EnemyMovement>();
        }
    }

    private void Update()
    {
        if (chainLightningEnabled)
        {
            FireChainLightning();
        }
    }

    public void AddStatusEffect(StatusEffect statusEffect, Object initiator)
    {
        // Don't apply status effects to dead characters
        if (health && health.Alive)
        {
            statusEffect.Initiator = initiator;

            statusEffects.Add(statusEffect);
            statusEffect.Apply(this);

            // UI
            if (hudElements)
            {
                hudElements.AddStatusEffectIcon(statusEffect);
            }
        }
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        statusEffect.Remove(this);
        statusEffects.Remove(statusEffect);

        // UI
        if (hudElements)
        {
            hudElements.RemoveStatusEffectIcon(statusEffect);
        }
    }

    public void RemoveStatusEffectsOfInitiator(Object initiator)
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (statusEffects[i].Initiator == initiator)
            {
                // Stop status effect and remove from list
                RemoveStatusEffect(statusEffects[i]);
            }
        }
    }

    public void IncreaseBuildup(List<float> damages, List<DamageEffects.DamageType> damageTypes, float _statusEffectDamagePercentage, float _bleedBuildup)
    {
        statusEffectDamagePercentage = _statusEffectDamagePercentage;
        BleedBuildup += _bleedBuildup; 

        float effectDamage;

        for (int i = 0; i < damages.Count; i++)
        {
            effectDamage = damages[i] * statusEffectDamagePercentage;

            switch (damageTypes[i])
            {
                case DamageEffects.DamageType.FIRE:
                    // build up fire
                    FireBuildup += effectDamage;
                    break;
                case DamageEffects.DamageType.POISON:
                    // build up poison
                    PoisonBuildup += effectDamage;
                    break;
                case DamageEffects.DamageType.FROST:
                    // build up frost
                    FrostBuildup += effectDamage;
                    break;
                case DamageEffects.DamageType.SHOCK:
                    // build up shock
                    shockDamage = effectDamage;
                    ShockBuildup += effectDamage;
                    break;
                case DamageEffects.DamageType.ACID:
                    // build up frost
                    DecayBuildup += effectDamage;
                    break;
                case DamageEffects.DamageType.PSIONIC:
                    // build up frost
                    InsanityBuildup += effectDamage;
                    break;
            }
        }
    }

    void BloodLoss()
    {
        BleedBuildup = 0;

        float fraction = (100f - BleedReduction) / 100f;

        // Deal percentage damage
        float damage = health.DecreaseHealthByFractionOfMax(fraction, DamageEffects.DamageType.BLEED);

        SpawnFloatingText(damage, DamageEffects.DamageType.BLEED);

        // visuals
        var spawnedBlood = Instantiate(bleedingFX, transform.position, Quaternion.identity);
        Destroy(spawnedBlood, timeToDestroyBleedingFX);

        // audio
        if (audioSource && bloodLossSoundEffects.Length > 0)
        {
            PlayRandomSound(bloodLossSoundEffects, bloodLossSoundEffectVolume);
        }
    }

    void Shocked()
    {
        ShockBuildup = 0;

        // find nearest enemy
        Transform closestEnemyTransform = FindClosestEnemy();

        if (closestEnemyTransform && chainLightningPoint && chainLightningDamageDealer && chainLightningLineRenderer)
        {
            // transfer shock damage to damage dealer
            CopyDamageListsToDamageDealer(chainLightningDamageDealer, new List<float> { shockDamage }, new List<DamageEffects.DamageType> { DamageEffects.DamageType.SHOCK });

            // audio
            if (audioSource && shockedSoundEffects.Length > 0)
            {
                PlayRandomSound(shockedSoundEffects, shockedSoundEffectVolume);
            }

            StartCoroutine(RayCastLightning(closestEnemyTransform.position));
        }
    }

    IEnumerator RayCastLightning(Vector3 target)
    {
        chainLightningEnabled = true;

        // calculate direction to nearest enemy
        chainLightningFireDirection = (target - chainLightningPoint.transform.position).normalized;

        // turn off enemy layer to let ray out
        if (ActorEnemy)
            gameObject.layer = 0;

        // cast a ray towards the enemy
        RaycastHit2D hit = Physics2D.Raycast(transform.position, chainLightningFireDirection, Mathf.Infinity, layerMask: chainLightningLayersToCollideWith);

        // reset layer
        if (ActorEnemy)
            gameObject.layer = enemyLayer;

        // if hits something
        if (hit.collider != null)
        {
            // track where the collision occurred
            chainLightningColliderHit = true;
            chainLightningHitPoint = hit.point;

            // deal damage
            chainLightningDamageDealer.CauseLaserDamage(hit.collider, gameObject, null);

            // spawn impact effect
            SpawnImpactEffect(hit);
        }
        else
        {
            chainLightningDamageDealer.ClearLists();
            chainLightningColliderHit = false;
        }

        yield return new WaitForSeconds(chainLightningVisualDuration);

        chainLightningEnabled = false;
        chainLightningColliderHit = false;

        chainLightningLineRenderer.positionCount = 0;
    }

    void FireChainLightning()
    {
        // if hits something
        if (chainLightningColliderHit)
        {
            // draw the laser
            DrawChainLightning(chainLightningPoint.transform.position, chainLightningHitPoint);
        }
        else
        {
            // draw ray to an arbitrary distance
            DrawChainLightning(chainLightningPoint.transform.position, chainLightningFireDirection * 100f);
        }
    }

    void DrawChainLightning(Vector2 startPos, Vector2 endPos)
    {
        chainLightningLineRenderer.positionCount = 2;
        chainLightningLineRenderer.SetPosition(0, startPos);
        chainLightningLineRenderer.SetPosition(1, endPos);
    }

    /* --- DECREASE BUILDUP COROUTINES --- */

    IEnumerator DecreaseFireBuildup()
    {
        decreasingFireBuildup = true;

        float fireDecreasePerTick = fireDecreasePerSecond / buildupDecreaseTicksPerSecond;

        // loop forever
        while (true)
        {
            // still have enough focus points
            if (FireBuildup > 0)
            {
                FireBuildup -= fireDecreasePerTick;
                if (FireBuildup < 0)
                    FireBuildup = 0;

                yield return new WaitForSeconds(1f / buildupDecreaseTicksPerSecond);
            }
            else
            {
                // stop decrease
                decreasingFireBuildup = false;
                yield break;
            }
        }
    }

    IEnumerator DecreasePoisonBuildup()
    {
        decreasingPoisonBuildup = true;

        float poisonDecreasePerTick = poisonDecreasePerSecond / buildupDecreaseTicksPerSecond;

        // loop forever
        while (true)
        {
            // still have enough focus points
            if (PoisonBuildup > 0)
            {
                PoisonBuildup -= poisonDecreasePerTick;
                if (PoisonBuildup < 0)
                    PoisonBuildup = 0;

                yield return new WaitForSeconds(1f / buildupDecreaseTicksPerSecond);
            }
            else
            {
                // stop decrease
                decreasingPoisonBuildup = false;
                yield break;
            }
        }
    }

    IEnumerator DecreaseFrostBuildup()
    {
        decreasingFrostBuildup = true;

        float frostDecreasePerTick = frostDecreasePerSecond / buildupDecreaseTicksPerSecond;

        // loop forever
        while (true)
        {
            // still have enough focus points
            if (FrostBuildup > 0)
            {
                FrostBuildup -= frostDecreasePerTick;
                if (FrostBuildup < 0)
                    FrostBuildup = 0;

                yield return new WaitForSeconds(1f / buildupDecreaseTicksPerSecond);
            }
            else
            {
                // stop decrease
                decreasingFrostBuildup = false;
                yield break;
            }
        }
    }

    IEnumerator DecreaseBleedBuildup()
    {
        decreasingBleedBuildup = true;

        float bleedDecreasePerTick = bleedDecreasePerSecond / buildupDecreaseTicksPerSecond;

        // loop forever
        while (true)
        {
            // still have enough focus points
            if (BleedBuildup > 0)
            {
                BleedBuildup -= bleedDecreasePerTick;
                if (BleedBuildup < 0)
                    BleedBuildup = 0;

                yield return new WaitForSeconds(1f / buildupDecreaseTicksPerSecond);
            }
            else
            {
                // stop decrease
                decreasingBleedBuildup = false;
                yield break;
            }
        }
    }

    IEnumerator DecreaseShockBuildup()
    {
        decreasingShockBuildup = true;

        float shockDecreasePerTick = shockDecreasePerSecond / buildupDecreaseTicksPerSecond;

        // loop forever
        while (true)
        {
            // still have enough focus points
            if (ShockBuildup > 0)
            {
                ShockBuildup -= shockDecreasePerTick;
                if (ShockBuildup < 0)
                    ShockBuildup = 0;

                yield return new WaitForSeconds(1f / buildupDecreaseTicksPerSecond);
            }
            else
            {
                // stop decrease
                decreasingShockBuildup = false;
                yield break;
            }
        }
    }

    IEnumerator DecreaseDecayBuildup()
    {
        decreasingDecayBuildup = true;

        float decayDecreasePerTick = decayDecreasePerSecond / buildupDecreaseTicksPerSecond;

        // loop forever
        while (true)
        {
            // still have enough decay points
            if (DecayBuildup > 0)
            {
                DecayBuildup -= decayDecreasePerTick;
                if (DecayBuildup < 0)
                    DecayBuildup = 0;

                yield return new WaitForSeconds(1f / buildupDecreaseTicksPerSecond);
            }
            else
            {
                // stop decrease
                decreasingDecayBuildup = false;
                yield break;
            }
        }
    }

    IEnumerator DecreaseInsanityBuildup()
    {
        decreasingInsanityBuildup = true;

        float insanityDecreasePerTick = insanityDecreasePerSecond / buildupDecreaseTicksPerSecond;

        // loop forever
        while (true)
        {
            // still have enough decay points
            if (InsanityBuildup > 0)
            {
                InsanityBuildup -= insanityDecreasePerTick;
                if (InsanityBuildup < 0)
                    InsanityBuildup = 0;

                yield return new WaitForSeconds(1f / buildupDecreaseTicksPerSecond);
            }
            else
            {
                // stop decrease
                decreasingInsanityBuildup = false;
                yield break;
            }
        }
    }

    /* ----- CURE FUNCTIONS ----- */
    public void CureAllStatusEffects()
    {
        if (IsBurning())
        {
            RemoveStatusEffect(instantiatedBurningStatusEffect);
        }

        if (IsPoisoned())
        {
            RemoveStatusEffect(instantiatedPoisonStatusEffect);
        }

        // don't cure dead frozen people
        if (IsFrozen() && IsAlive())
        {
            RemoveStatusEffect(instantiatedFrozenStatusEffect);
        }

        if (IsDecayed())
        {
            RemoveStatusEffect(instantiatedDecayStatusEffect);
        }

        if (IsInsane())
        {
            RemoveStatusEffect(instantiatedInsanityStatusEffect);
        }
    }

    /* --- STATE GETTERS --- */
    bool IsAlive()
    {
        if (ActorPlayer)
        {
            return ActorPlayer.Health.Alive;
        }
        else if (ActorEnemy)
        {
            return ActorEnemy.Health.Alive;
        }

        return false;
    }

    public bool IsBurning()
    {
        return statusEffects.Contains(instantiatedBurningStatusEffect);
    }

    public bool IsPoisoned()
    {
        return statusEffects.Contains(instantiatedPoisonStatusEffect);
    }

    public bool IsFrozen()
    {
        return statusEffects.Contains(instantiatedFrozenStatusEffect);
    }

    public bool IsDecayed()
    {
        return statusEffects.Contains(instantiatedDecayStatusEffect);
    }

    public bool IsInsane()
    {
        return statusEffects.Contains(instantiatedInsanityStatusEffect);
    }

    /* --- UTILITY --- */
    public void SpawnFloatingText(float damage, DamageEffects.DamageType damageType)
    {
        if (floatingText)
        {
            GameObject damageText = Instantiate(floatingText, health.transform.position, Quaternion.identity);
            damageText.GetComponent<FloatingText>().SetParameters(damage, damageType);
        }
    }

    private void PlayRandomSound(AudioClip[] clips, float volume)
    {
        int randomIndex = Random.Range(0, clips.Length);
        audioSource.PlayOneShot(clips[randomIndex], volume);
    }

    private void SpawnImpactEffect(RaycastHit2D hit)
    {
        if ((hit.collider.gameObject.layer == enemyLayer || hit.collider.gameObject.layer == playerLayer) && actorImpactEffect)
        {
            // hit enemy/player
            if (chainLightningDamageDealer.CritSuccess)
            {
                Instantiate(actorCritImpactEffect, chainLightningHitPoint, Quaternion.identity);
            }
            else
            {
                Instantiate(actorImpactEffect, chainLightningHitPoint, Quaternion.identity);
            }
        }
        else if ((hit.collider.gameObject.layer == wallLayer) && wallImpactEffect)
        {
            // hit wall
            Vector2 n = hit.normal.normalized;

            float angle = Mathf.Atan2(n.y, n.x) * Mathf.Rad2Deg - 90f;

            var newObject = Instantiate(wallImpactEffect, chainLightningHitPoint, Quaternion.identity);
            newObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    Transform FindClosestEnemy()
    {
        var enemies = FindObjectsOfType<Enemy>();

        float minDistance = float.MaxValue;
        Enemy minDistanceEnemy = null;

        foreach (Enemy e in enemies)
        {
            if (e.Health.Alive && e != ActorEnemy && e.GetComponent<Health>().CurrentHealth > 0)
            {
                float enemyDistance = Vector3.Distance(transform.position, e.transform.position);

                if (enemyDistance < minDistance)
                {
                    minDistance = enemyDistance;
                    minDistanceEnemy = e;
                }
            }
        }

        // free fire if no enemy target
        if (minDistanceEnemy)
        {
            return minDistanceEnemy.transform;
        }
        else
        {
            return null;
        }
    }

    public void CopyDamageListsToDamageDealer(DamageDealer dealer, List<float> damages, List<DamageEffects.DamageType> damageTypes)
    {
        dealer.CopyDamageList(damages);
        dealer.CopyDamageTypeList(damageTypes);

        // status effects
        dealer.SetStatusEffectDamagePercentage(statusEffectDamagePercentage);
    }
}
