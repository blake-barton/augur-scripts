using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Focus : MonoBehaviour
{
    public Misc implantItem;
    public FocusImplant focusImplant;

    // config
    [Header("Config")]
    [SerializeField] float focusPoints = 100f;
    [SerializeField] float maxFocusPoints = 100f;
    [SerializeField] float fpTicksPerSecond = 1f;                   // magic points regained per second
    [SerializeField] float fpRegenPerTick = 1f;
    [SerializeField] float fpRegenPerSecond;

    // state
    [Header("State")]
    [SerializeField] bool regenEnabled = false;
    [SerializeField] bool implantActivated = false;

    [Header("Bar")]
    [SerializeField] ValueBar focusBar;

    [Header("Sound")]
    [SerializeField] AudioSource audioSource;
    public AudioClip activateSound;
    [Range(0, 1)] public float activateVolume = .5f;
    public AudioClip deactivateSound;
    [Range(0, 1)] public float deactivateVolume = .5f;

    // cache
    Player player;
    Inventory inventory;
    TabMenuManager tabMenuManager;
    WeaponWheelManager weaponWheelManager;
    HUDAmmoText hudManager;

    // couroutines
    Coroutine fpRegen;
    Coroutine fpBurn;

    public float FocusPoints { get => focusPoints; }
    public float MaxFocusPoints { get => maxFocusPoints; }
    public float FpTicksPerSecond { get => fpTicksPerSecond; set => fpTicksPerSecond = value; }
    public float FpRegenPerTick { get => fpRegenPerTick; set => fpRegenPerTick = value; }

    private void Awake()
    {
        player = GetComponent<Player>();
        inventory = GetComponent<Inventory>();
        tabMenuManager = FindObjectOfType<TabMenuManager>();
        weaponWheelManager = FindObjectOfType<WeaponWheelManager>();
        hudManager = FindObjectOfType<HUDAmmoText>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetBarValues();
        StartFPRegen();
    }

    public void EquipImplant(int itemIndex)
    {
        Misc implant = (Misc)inventory.Items[itemIndex].First;

        if (!focusImplant)
        {
            // no implant equipped. Equip this one.
            implant.usable.Use();
            implantItem = implant;
            implant.Equipped = true;

            // Update UI
            hudManager.SetFocusImplantIcon(implant);
        }
        else
        {
            // implant exists
            if (implant.ItemID == implantItem.ItemID)
            {
                UnequipImplant();
            }
            else
            {
                // replacing an implant
                implantItem.Equipped = false;        // unequip current implant
                implant.usable.Use();                // equip new implant
                implantItem = implant;               // set it as the equipped implant
                implant.Equipped = true;             // set new as equipped

                // Update UI
                hudManager.SetFocusImplantIcon(implant);
            }
        }
    }

    void UnequipImplant()
    {
        implantItem.Equipped = false;
        implantItem = null;
        focusImplant = null;

        // Update UI
        hudManager.ClearFocusImplantIcon();
    }

    public void OnInputFocusImplant(InputAction.CallbackContext context)
    {
        if (!tabMenuManager.InMenu && !player.InDialogue && !weaponWheelManager.WheelDisplayed)
        {
            if (focusImplant)
            {
                if (!implantActivated && !player.Character.Incapacitated)
                {
                    FreezeFPRegen();
                    focusImplant.Activate();
                    fpBurn = StartCoroutine(BurnFP());
                    implantActivated = true;

                    // sound
                    audioSource.PlayOneShot(activateSound, activateVolume);
                }
                else if (implantActivated)
                {
                    focusImplant.Deactivate();

                    if (fpBurn != null)
                        StopCoroutine(fpBurn);

                    StartFPRegen();
                    implantActivated = false;

                    // sound
                    audioSource.PlayOneShot(deactivateSound, deactivateVolume);
                }
            }
        }
    }

    // Invoked by player.isIncapacitated event
    public void DeactivateImplant()
    {
        if (implantActivated)
        {
            focusImplant.Deactivate();

            if (fpBurn != null)
            {
                StopCoroutine(fpBurn);
            }

            StartFPRegen();
            implantActivated = false;

            // sound
            audioSource.PlayOneShot(deactivateSound, deactivateVolume);
        }
    }

    public void SetRegenPerSecond(float regenPerSecond)
    {
        fpRegenPerTick = regenPerSecond / fpTicksPerSecond;
        fpRegenPerSecond = regenPerSecond;
    }

    public void SetFocusPoints(float fp)
    {
        focusPoints = fp;
        SetBarValues();
    }

    public void IncreaseFocusPoints(float quantity)
    {
        if (focusPoints + quantity <= maxFocusPoints)
        {
            focusPoints += quantity;
        }
        else
        {
            focusPoints = maxFocusPoints;
        }

        if (focusBar)
        {
            focusBar.SetValue(focusPoints);
        }
    }

    public void DecreaseFocusPoints(float quantity)
    {
        if (focusPoints - quantity < 0)
        {
            focusPoints = 0;
        }
        else
        {
            focusPoints -= quantity;
        }

        if (focusBar)
        {
            focusBar.SetValue(focusPoints);
        }
    }

    public void IncreaseFocusByFractionOfMax(float fraction)
    {
        float val = maxFocusPoints * fraction;
        IncreaseFocusPoints(val);
    }

    public void SetMaxFocusPoints(float max)
    {
        if (max < 0)
        {
            maxFocusPoints = 0;
        }
        else
        {
            maxFocusPoints = max;
        }

        SetBarValues();
    }

    public void StartFPRegen()
    {
        // start if not already active
        if (!regenEnabled)
        {
            fpRegen = StartCoroutine(RegenFP());
            regenEnabled = true;
        }
    }

    public void FreezeFPRegen()
    {
        if (fpRegen != null)
        {
            StopCoroutine(fpRegen);
            regenEnabled = false;
        }
    }

    IEnumerator RegenFP()
    {
        // loop forever
        while (true)
        {
            // need more focus points
            if (focusPoints < maxFocusPoints)
            {
                IncreaseFocusPoints(fpRegenPerTick);
                yield return new WaitForSeconds(1f / fpTicksPerSecond);
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator BurnFP()
    {
        float fpBurnPerTick = focusImplant.fpBurnedPerSecond / fpTicksPerSecond;

        // loop forever
        while (true)
        {
            // still have enough focus points
            if (focusPoints > 0)
            {
                DecreaseFocusPoints(fpBurnPerTick);
                yield return new WaitForSeconds(1f / fpTicksPerSecond);
            }
            else
            {
                // out of focus points, deactivate implant and reenable regen
                focusImplant.Deactivate();
                implantActivated = false;
                StartFPRegen();

                // sound
                audioSource.PlayOneShot(deactivateSound, deactivateVolume);

                yield break;
            }
        }
    }

    private void SetBarValues()
    {
        if (focusBar)
        {
            focusBar.SetMaxValue(maxFocusPoints);
            focusBar.SetValue(FocusPoints);
        }
    }
}
