using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Magic : MonoBehaviour
{
    // config
    [SerializeField] List<Spell> spellList = new List<Spell>();
    [SerializeField] SpellDescriptions spellDescriptions;

    [Header("Config")]
    [SerializeField] float magicPoints = 100f;
    [SerializeField] float maxMagicPoints = 100f;
    [SerializeField] float mpTicksPerSecond = 1f;                   // magic points regained per second
    [SerializeField] float mpRegenPerTick = 1f;
    [SerializeField] float mpRegenPerSecond;

    // state
    [Header("State")]
    [SerializeField] bool regenEnabled = false;
    [SerializeField] bool burningEnabled = false;

    [Header("Bar")]
    [SerializeField] ValueBar magicBar;

    // events
    public UnityEvent mpDepleted;

    // couroutines
    Coroutine mpRegen;
    Coroutine mpBurn;

    public List<Spell> SpellList { get => spellList; set => spellList = value; }
    public float MagicPoints { get => magicPoints; }
    public float MaxMagicPoints { get => maxMagicPoints; }
    public float MpTicksPerSecond { get => mpTicksPerSecond; set => mpTicksPerSecond = value; }
    public float MpRegenPerTick { get => mpRegenPerTick; set => mpRegenPerTick = value; }
    public bool BurningEnabled { get => burningEnabled; set => burningEnabled = value; }

    // Start is called before the first frame update
    void Start()
    {
        SetBarValues();
        StartMPRegen();
    }

    public void SetRegenPerSecond(float regenPerSecond)
    {
        mpRegenPerTick = regenPerSecond / mpTicksPerSecond;
        mpRegenPerSecond = regenPerSecond;
    }

    public Spell AddSpell(Spell spell)
    {
        Spell newSpell = Object.Instantiate(spell);

        // set spell's effect description
        newSpell.EffectDescription = spellDescriptions.GetDescription(newSpell.SpellID);

        // add to spell list
        spellList.Add(newSpell);
        SortListByNameAscending();

        return newSpell;
    }

    public void SetMagicPoints(float mp)
    {
        magicPoints = mp;
        SetBarValues();
    }

    public void IncreaseMagicPoints(float quantity)
    {
        if (magicPoints + quantity <= maxMagicPoints)
        {
            magicPoints += quantity;
        }
        else
        {
            magicPoints = maxMagicPoints;
        }

        if (magicBar)
        {
            magicBar.SetValue(MagicPoints);
        }
    }

    public void DecreaseMagicPoints(float quantity)
    {
        if (magicPoints - quantity <= 0)
        {
            magicPoints = 0;

            mpDepleted.Invoke();
        }
        else
        {
            magicPoints -= quantity;
        }

        if (magicBar)
        {
            magicBar.SetValue(MagicPoints);
        }
    }

    public void IncreaseMagicByFractionOfMax(float fraction)
    {
        float val = maxMagicPoints * fraction;
        IncreaseMagicPoints(val);
    }

    public void SetMaxMagicPoints(float max)
    {
        if (max < 0)
        {
            maxMagicPoints = 0;
        }
        else
        {
            maxMagicPoints = max;
        }

        SetBarValues();
    }

    private void SortListByNameAscending()
    {
        spellList.Sort((x, y) => x.SpellName.CompareTo(y.SpellName));
    }

    public void StartMPRegen()
    {
        // start if not already active
        if (!regenEnabled)
        {
            mpRegen = StartCoroutine(RegenMP());
            regenEnabled = true;
        }
    }

    public void FreezeMPRegen()
    {
        StopCoroutine(mpRegen);
        regenEnabled = false;
    }

    IEnumerator RegenMP()
    {
        // loop forever
        while (true)
        {
            // need more magic points
            if (magicPoints < maxMagicPoints)
            {
                IncreaseMagicPoints(MpRegenPerTick);
                yield return new WaitForSeconds(1f / mpTicksPerSecond);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void StartMPBurn(float mpBurnedPerSecond)
    {
        if (!BurningEnabled)
        {
            mpBurn = StartCoroutine(BurnMP(mpBurnedPerSecond));
            BurningEnabled = true;
        }
    }

    public void StopMPBurn()
    {
        if (BurningEnabled)
        {
            StopCoroutine(mpBurn);
            BurningEnabled = false;

            StartMPRegen();
        }
    }

    IEnumerator BurnMP(float mpBurnedPerSecond)
    {
        FreezeMPRegen();

        float mpBurnPerTick = mpBurnedPerSecond / mpTicksPerSecond;

        // loop forever
        while (true)
        {
            // still have enough focus points
            if (magicPoints > 0)
            {
                DecreaseMagicPoints(mpBurnPerTick);
                yield return new WaitForSeconds(1f / mpTicksPerSecond);
            }
            else
            {
                // out of magic points, reenable regen
                BurningEnabled = false;

                // event
                mpDepleted.Invoke();

                StartMPRegen();
                yield break;
            }
        }
    }

    public Spell GetSpellEquippedMain()
    {
        foreach (Spell spell in spellList)
        {
            if (spell.EquippedMain)
            {
                return spell;
            }
        }

        // return null if no item in main hand
        return null;
    }

    public Spell GetSpellEquippedOff()
    {
        foreach (Spell spell in spellList)
        {
            if (spell.EquippedOff)
            {
                return spell;
            }
        }

        // return null if no item in off hand
        return null;
    }

    public int GetIndexOfMainHandSpell()
    {
        for (int i = 0; i < spellList.Count; i++)
        {
            if (spellList[i].EquippedMain)
            {
                return i;
            }
        }

        return -1;
    }

    public int GetIndexOfOffHandSpell()
    {
        for (int i = 0; i < spellList.Count; i++)
        {
            if (spellList[i].EquippedOff)
            {
                return i;
            }
        }

        return -1;
    }

    private void SetBarValues()
    {
        if (magicBar)
        {
            magicBar.SetMaxValue(maxMagicPoints);
            magicBar.SetValue(magicPoints);
        }
    }
}
