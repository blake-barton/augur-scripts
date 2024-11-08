using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageThresholds : MonoBehaviour
{
    [SerializeField] GameplayAttribute bluntDT;
    [SerializeField] GameplayAttribute piercingDT;
    [SerializeField] GameplayAttribute slashingDT;
    [SerializeField] GameplayAttribute energyDT;
    [SerializeField] GameplayAttribute fireDT;
    [SerializeField] GameplayAttribute frostDT;
    [SerializeField] GameplayAttribute shockDT;
    [SerializeField] GameplayAttribute acidDT;
    [SerializeField] GameplayAttribute poisonDT;
    [SerializeField] GameplayAttribute psionicDT;

    public GameplayAttribute BluntDT { get => bluntDT; set => bluntDT = value; }
    public GameplayAttribute PiercingDT { get => piercingDT; set => piercingDT = value; }
    public GameplayAttribute SlashingDT { get => slashingDT; set => slashingDT = value; }
    public GameplayAttribute EnergyDT { get => energyDT; set => energyDT = value; }
    public GameplayAttribute FireDT { get => fireDT; set => fireDT = value; }
    public GameplayAttribute FrostDT { get => frostDT; set => frostDT = value; }
    public GameplayAttribute ShockDT { get => shockDT; set => shockDT = value; }
    public GameplayAttribute AcidDT { get => acidDT; set => acidDT = value; }
    public GameplayAttribute PoisonDT { get => poisonDT; set => poisonDT = value; }
    public GameplayAttribute PsionicDT { get => psionicDT; set => psionicDT = value; }

    public void ModifyIncomingDamage(List<float> incomingDamages, List<DamageEffects.DamageType> incomingTypes)
    {
        /*  Example list appearance (sizes must be the same or I screwed up in serialization)
     *      Damages: [23.0, 10.0, 30.0]
     *      Types:   [FIRE, ACID, POISON]
        */

        for (int i = 0; i < incomingDamages.Count; i++)
        {
            switch (incomingTypes[i])
            {
                case DamageEffects.DamageType.BLUNT:
                    incomingDamages[i] = Mathf.Max(0, incomingDamages[i] - BluntDT.GetCurrentValue(false));
                    break;
                case DamageEffects.DamageType.PIERCING:
                    incomingDamages[i] = Mathf.Max(0, incomingDamages[i] - PiercingDT.GetCurrentValue(false));
                    break;
                case DamageEffects.DamageType.SLASHING:
                    incomingDamages[i] = Mathf.Max(0, incomingDamages[i] - SlashingDT.GetCurrentValue(false));
                    break;
                case DamageEffects.DamageType.ENERGY:
                    incomingDamages[i] = Mathf.Max(0, incomingDamages[i] - EnergyDT.GetCurrentValue(false));
                    break;
                case DamageEffects.DamageType.FIRE:
                    incomingDamages[i] = Mathf.Max(0, incomingDamages[i] - FireDT.GetCurrentValue(false));
                    break;
                case DamageEffects.DamageType.FROST:
                    incomingDamages[i] = Mathf.Max(0, incomingDamages[i] - FrostDT.GetCurrentValue(false));
                    break;
                case DamageEffects.DamageType.SHOCK:
                    incomingDamages[i] = Mathf.Max(0, incomingDamages[i] - ShockDT.GetCurrentValue(false));
                    break;
                case DamageEffects.DamageType.ACID:
                    incomingDamages[i] = Mathf.Max(0, incomingDamages[i] - AcidDT.GetCurrentValue(false));
                    break;
                case DamageEffects.DamageType.POISON:
                    incomingDamages[i] = Mathf.Max(0, incomingDamages[i] - PoisonDT.GetCurrentValue(false));
                    break;
                case DamageEffects.DamageType.PSIONIC:
                    incomingDamages[i] = Mathf.Max(0, incomingDamages[i] - PsionicDT.GetCurrentValue(false));
                    break;
            }
        }
    }

    public void SetThresholds(float blunt = 0, float piercing = 0, float slashing = 0, float energy = 0, float fire = 0, float frost = 0, float shock = 0, float acid = 0, float poison = 0, float psionic = 0)
    {
        BluntDT.BaseValue = blunt;
        PiercingDT.BaseValue = piercing;
        SlashingDT.BaseValue = slashing;
        EnergyDT.BaseValue = energy;
        FireDT.BaseValue = fire;
        FrostDT.BaseValue = frost;
        ShockDT.BaseValue = shock;
        AcidDT.BaseValue = acid;
        PoisonDT.BaseValue = poison;
        PsionicDT.BaseValue = psionic;
    }
}
