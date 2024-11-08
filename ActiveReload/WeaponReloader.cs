using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponReloader : MonoBehaviour
{
    // config
    [SerializeField] int clipSize;
    [SerializeField] int ammoInClip;
    [SerializeField] float reloadTime;
    [SerializeField] bool isReloading = false;
    [SerializeField] Item.AmmoType ammoType;

    [Header("Active Reload")]
    [SerializeField] GameObject reloadBar;
    [SerializeField] [Range(0, 1)] float sweetSpotStartPercentage;      // sweetSpotStart < sweetSpotEnd < normalSpotEnd
    [SerializeField] [Range(0, 1)] float sweetSpotEndPercentage;
    [SerializeField] [Range(0, 1)] float normalSpotEndPercentage;
    [SerializeField] float sweetSpotSpeedFactor;                        // how many times faster should the timer go
    [SerializeField] float sweetSpotDamageMultiplier;
    [SerializeField] bool sweetSpotHit = false;
    [SerializeField] int bonusDamageRoundsLoaded = 0;
    [SerializeField] bool useBasicReload = false;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip reloadAudio;
    [SerializeField] [Range(0, 1)] float reloadVolume = .5f;
    [SerializeField] AudioClip jamAudio;
    [SerializeField] [Range(0, 1)] float jamVolume = .5f;
    [SerializeField] AudioClip sweetSpotAudio;
    [SerializeField] [Range(0, 1)] float sweetSpotVolume = .5f;
    [SerializeField] AudioClip normalSpotAudio;
    [SerializeField] [Range(0, 1)] float normalSpotAudioVolume = 1f;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    Sprite startingSprite;
    
    // cache
    EquippedWeapon equippedWeapon;
    AmmoCounter ammoCounter;
    RangedWeapon rangedWeapon;
    Canvas canvas;
    Player player;

    // coroutines
    Coroutine activeReloadCoroutine;

    // util
    GameObject reloadBarInstance;
    ActiveReloadBackground activeReloadBar;

    // used to communicate what happened when the player hits the reload key again during the active reload
    public enum ActiveReloadState
    {
        SWEETSPOT,
        NORMALSPOT,
        JAMMED,
        FULLSLIDE
    }
    private ActiveReloadState reloadState = ActiveReloadState.FULLSLIDE;

    public int BonusDamageRoundsLoaded { get => bonusDamageRoundsLoaded; set => bonusDamageRoundsLoaded = value; }
    public float SweetSpotDamageMultiplier { get => sweetSpotDamageMultiplier; set => sweetSpotDamageMultiplier = value; }
    public int ClipSize { get => clipSize; set => clipSize = value; }
    public int AmmoInClip { get => ammoInClip; set => ammoInClip = value; }
    public float ReloadTime { get => reloadTime; set => reloadTime = value; }
    public bool IsReloading { get => isReloading; set => isReloading = value; }
    public ActiveReloadState ReloadState { get => reloadState; set => reloadState = value; }

    // Start is called before the first frame update
    void Start()
    {
        equippedWeapon = GetComponent<EquippedWeapon>();
        ammoCounter = equippedWeapon.owner.GetComponent<AmmoCounter>();
        rangedWeapon = (RangedWeapon)equippedWeapon.ItemData;
        canvas = GameObject.FindGameObjectWithTag("GameCanvas").GetComponent<Canvas>();
        player = FindObjectOfType<Player>();

        ClipSize = rangedWeapon.ClipSize;
        AmmoInClip = rangedWeapon.AmmoInClip;
        ammoType = rangedWeapon.AmmunitionType;
        ReloadTime = rangedWeapon.ReloadTime;

        if (spriteRenderer)
            startingSprite = spriteRenderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (useBasicReload)
        {
            BasicReloadAutomatically();
        }
        else
        {
            ReloadAutomatically();
        }
    }

    private void ReloadAutomatically()
    {
        // automatically reload when clip empty
        if (AmmoInClip <= 0 && ammoCounter.GetAmmoCountByType(ammoType) > 0 && !IsReloading)
        {
            if (equippedWeapon.owner.GetComponent<Player>())
            {
                TriggerActiveReload();
            }
        }
    }

    private void BasicReloadAutomatically()
    {
        // automatically reload when clip empty
        if (AmmoInClip <= 0 && ammoCounter.GetAmmoCountByType(ammoType) > 0 && !IsReloading)
        {
            if (equippedWeapon.owner.GetComponent<Player>())
            {
                BasicReload();
            }
        }
    }

    public void BasicReload()
    {
        // get ammo in reserves
        if (ammoCounter.GetAmmoCountByType(ammoType) > 0)
        {
            // remove (ClipSize - AmmoInClip) from the reserves (if possible) and add the amount removed to ammoInClip
            int pulledAmmo = ammoCounter.PullAmmoFromReserves(ammoType, ClipSize - AmmoInClip);
            AmmoInClip += pulledAmmo;

            // bonus damage
            if (sweetSpotHit)
            {
                BonusDamageRoundsLoaded = pulledAmmo;
                sweetSpotHit = false;
            }
            else
            {
                BonusDamageRoundsLoaded = 0;
            }
        }
    }

    public void TriggerActiveReload()
    {
        activeReloadCoroutine = StartCoroutine(ActiveReload());
    }

    // used by player
    IEnumerator ActiveReload()
    {
        // disable weapon actions
        IsReloading = true;
        player.IsBusy = true;

        // animate
        if (animator)
        {
            animator.SetBool("AnimIsReloading", true);
        }

        bool sweetSpotSound = false;
        bool normalSpotSound = false;

        // play sound
        if (audioSource)
        {
            audioSource.clip = reloadAudio;
            audioSource.volume = reloadVolume;
            audioSource.loop = false;
            audioSource.Play();
        }

        // instantiate a reload bar with a different offset depending on whether this weapon is main or offhand
        reloadBarInstance = Instantiate(reloadBar, canvas.transform);
        activeReloadBar = reloadBarInstance.GetComponent<ActiveReloadBackground>();

        // initiate and get result back from slider
        activeReloadBar.SetParameters(rangedWeapon.EquippedMain, ReloadTime, sweetSpotStartPercentage, sweetSpotEndPercentage, normalSpotEndPercentage, this);

        // if the slider is allowed to finish, do nothing
        for (float timer = ReloadTime; timer >= 0; timer -= Time.deltaTime)
        {
            if (ReloadState == ActiveReloadState.SWEETSPOT)
            {
                // speed up reload
                timer /= sweetSpotSpeedFactor;

                // increase damage of reloaded ammo
                sweetSpotHit = true;
                sweetSpotSound = true;
            }
            else if (ReloadState == ActiveReloadState.NORMALSPOT)
            {
                // speed up reload
                timer /= sweetSpotSpeedFactor;

                normalSpotSound = true;
            }
            else if (ReloadState == ActiveReloadState.JAMMED)
            {
                // increase time to finish reload
                timer = ReloadTime;
                ReloadState = ActiveReloadState.FULLSLIDE;

                // animate
                if (animator)
                {
                    animator.SetBool("AnimIsJammed", true);
                }

                // sound
                if (audioSource)
                {
                    audioSource.Stop();
                    audioSource.clip = jamAudio;
                    audioSource.volume = jamVolume;
                    audioSource.Play();

                    // display status message
                    equippedWeapon.StatusMessageManager.DisplayMessage("JAMMED");
                }
            }

            yield return null;
        }

        // complete the reload as done in BasicReload()
        BasicReload();

        // ReEnable weapon firing
        ReloadState = ActiveReloadState.FULLSLIDE;
        IsReloading = false;
        player.IsBusy = false;

        // stop animation
        if (animator)
        {
            animator.SetBool("AnimIsReloading", false);
            animator.SetBool("AnimIsJammed", false);
        }

        // stop sound
        if (audioSource)
        {
            audioSource.Stop();
        }

        // spot sounds
        if (audioSource && sweetSpotSound)
        {
            // sweet spot
            audioSource.PlayOneShot(sweetSpotAudio, sweetSpotVolume);
            sweetSpotSound = false;

            // display status message
            equippedWeapon.StatusMessageManager.DisplayMessage("PERFECT");
        }
        else if (audioSource && normalSpotSound)
        {
            // normal spot
            audioSource.PlayOneShot(normalSpotAudio, normalSpotAudioVolume);
            normalSpotSound = false;

            // display status message
            equippedWeapon.StatusMessageManager.DisplayMessage("GOOD");
        }

        // Destroy the reload bar
        Destroy(reloadBarInstance);
    }

    public void DecreaseAmmoInClip(int quantity)
    {
        if (AmmoInClip - quantity >= 0)
        {
            AmmoInClip -= quantity;
        }
        else
        {
            AmmoInClip = 0;
        }
    }

    // stops the reload midway, preventing ammo from being loaded
    public void CancelReload()
    {
        StopAllCoroutines();

        // ReEnable weapon firing
        ReloadState = ActiveReloadState.FULLSLIDE;
        IsReloading = false;
        player.IsBusy = false;

        // stop animation
        if (animator)
        {
            animator.SetBool("AnimIsReloading", false);
            animator.SetBool("AnimIsJammed", false);
        }

        // reset sprite
        spriteRenderer.sprite = startingSprite;

        // stop sound
        if (audioSource)
        {
            audioSource.Stop();
        }

        // Destroy the reload bar
        if (reloadBarInstance)
            Destroy(reloadBarInstance);
    }

    public void ReceiveActiveReloadInput()
    {
        if (activeReloadBar)
        {
            ReloadState = activeReloadBar.FinishReload();
        }
    }
}
