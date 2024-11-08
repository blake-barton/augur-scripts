using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEffects : MonoBehaviour
{
    // this is used by derived classes such as Outfit and Weapon to serialize damage, resistances, immunities, vulnerabilities, buffs, etc.
    public enum DamageType
    {
        BLUNT,
        PIERCING,
        SLASHING,
        ENERGY,
        FIRE,
        FROST,
        SHOCK,
        ACID,
        POISON,
        PSIONIC,
        BLEED,
        NONE
    }

    [Header("Damage Properties")]
    [SerializeField] List<DamageType> resistances = new List<DamageType>();
    [SerializeField] List<DamageType> immunities = new List<DamageType>();
    [SerializeField] List<DamageType> vulnerabilities = new List<DamageType>();

    // getter/setters
    public List<DamageType> Resistances { get => resistances; set => resistances = value; }
    public List<DamageType> Immunities { get => immunities; set => immunities = value; }
    public List<DamageType> Vulnerabilities { get => vulnerabilities; set => vulnerabilities = value; }

    public void ModifyIncomingDamage(List<float> incomingDamages, List<DamageType> incomingTypes)
    {
        /*  Example list appearance (sizes must be the same or I screwed up in serialization)
         *      Damages: [23.0, 10.0, 30.0]
         *      Types:   [FIRE, ACID, POISON]
        */ 

        for (int i = 0; i < incomingDamages.Count; i++)
        {
            if (immunities.Contains(incomingTypes[i]))
            {
                incomingDamages[i] = 0;                             // nullify immunities
            }
            else if (resistances.Contains(incomingTypes[i]))
            {
                incomingDamages[i] = incomingDamages[i] * 0.5f;     // halve resistances
            }
            else if (vulnerabilities.Contains(incomingTypes[i]))
            {
                incomingDamages[i] = incomingDamages[i] * 2f;       // double vulnerabilities
            }
        }
    }

    public bool CheckResistanceOrImmunity(DamageType damageType)
    {
        if (resistances.Contains(damageType) || immunities.Contains(damageType))
        {
            return true;
        }

        return false;
    }

    public bool IsImmune(DamageType damageType)
    {
        if (immunities.Contains(damageType))
        {
            return true;
        }

        return false;
    }
}
