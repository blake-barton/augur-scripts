using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Augmentations : MonoBehaviour
{
    [SerializeField] int augmentationPoints = 0;
    [SerializeField] HashSet<Augmentation> augmentationList = new HashSet<Augmentation>();

    AttributeScores attributeScores;

    private void Awake()
    {
        attributeScores = GetComponent<AttributeScores>();
    }

    public int AugmentationPoints { get => augmentationPoints; set => augmentationPoints = value; }
    public HashSet<Augmentation> AugmentationList { get => augmentationList; }

    public void AddAugmentation(Augmentation augmentation)
    {
        AugmentationList.Add(augmentation);

        augmentation.Equip(this);
    }

    public bool CheckIfPurchasable(Augmentation augmentation)
    {
        // check level
        if (augmentation.LevelRequirement > attributeScores.level)
        {
            return false;
        }

        // check attributes
        foreach (Augmentation.AttributeRequirement attributeRequirement in augmentation.AttributeRequirements)
        {
            switch (attributeRequirement.attribute)
            {
                case AttributeScores.Attributes.STRENGTH:
                    if (attributeRequirement.requirement > attributeScores.Strength)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Attributes.PERCEPTION:
                    if (attributeRequirement.requirement > attributeScores.Perception)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Attributes.ENDURANCE:
                    if (attributeRequirement.requirement > attributeScores.Endurance)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Attributes.INTELLIGENCE:
                    if (attributeRequirement.requirement > attributeScores.Intelligence)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Attributes.AGILITY:
                    if (attributeRequirement.requirement > attributeScores.Agility)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Attributes.ABERRANCE:
                    if (attributeRequirement.requirement > attributeScores.Aberrance)
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }
        }

        // check skills
        foreach (Augmentation.SkillRequirement skillRequirement in augmentation.SkillRequirements)
        {
            switch (skillRequirement.skill)
            {
                case AttributeScores.Skills.ACROBATICS:
                    if (skillRequirement.requirement > attributeScores.acrobatics)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.BARTER:
                    if (skillRequirement.requirement > attributeScores.barter)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.BLADE:
                    if (skillRequirement.requirement > attributeScores.blade)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.BLUDGEONING:
                    if (skillRequirement.requirement > attributeScores.bludgeoning)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.ENKIISM:
                    if (skillRequirement.requirement > attributeScores.enkiism)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.EREBANCY:
                    if (skillRequirement.requirement > attributeScores.erebancy)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.EXPLOSIVES:
                    if (skillRequirement.requirement > attributeScores.explosives)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.GAIANISM:
                    if (skillRequirement.requirement > attributeScores.gaianism)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.HEAVY_WEAPONS:
                    if (skillRequirement.requirement > attributeScores.heavyWeapons)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.HERMETICISM:
                    if (skillRequirement.requirement > attributeScores.hermeticism)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.IKTOMANCY:
                    if (skillRequirement.requirement > attributeScores.iktomancy)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.PISTOLS:
                    if (skillRequirement.requirement > attributeScores.pistols)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.POLEARMS:
                    if (skillRequirement.requirement > attributeScores.polearms)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.RESHEPHISM:
                    if (skillRequirement.requirement > attributeScores.reshephism)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.RESOURCEFULNESS:
                    if (skillRequirement.requirement > attributeScores.resourcefulness)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.RIFLES:
                    if (skillRequirement.requirement > attributeScores.rifles)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.SLEIGHT_OF_HAND:
                    if (skillRequirement.requirement > attributeScores.sleightOfHand)
                    {
                        return false;
                    }
                    break;
                case AttributeScores.Skills.THROWING:
                    if (skillRequirement.requirement > attributeScores.throwing)
                    {
                        return false;
                    }
                    break;
                default:
                    break;
            }
        }

        return true;
    }
}
