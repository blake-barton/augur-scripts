using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResistanceReaction : MonoBehaviour
{
    DamageEffects damageEffects;
    DamageEffects.DamageType currentResistance = DamageEffects.DamageType.NONE;

    private void Awake()
    {
        damageEffects = GetComponent<DamageEffects>();
    }

    /// <summary>
    /// Called by UnityEvent. Add the resistance of the received damage type.
    /// </summary>
    public void OnHit(DamageEffects.DamageType damageType)
    {
        if (damageType != currentResistance)
        {
            // Change resistance to new resistance
            if (currentResistance != DamageEffects.DamageType.NONE)
            {
                damageEffects.Resistances.Remove(currentResistance);
            }

            damageEffects.Resistances.Add(damageType);
            currentResistance = damageType;

            // Spawn animated icon
            Debug.Log("Changed resistance to " + damageType.ToString());
        }
    }
}
