using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeScores : MonoBehaviour
{
    public int level = 1;

    [Header("Attributes Utility")]
    [SerializeField] int totalAttributePoints = 0;
    [SerializeField] int unusedPoints = 0;

    [Header("Skills Utility")]
    public int totalTagsAvailable = 3;
    public int unusedTags = 3;
    public int tagBonus = 15;

    [Header("Attribute Scores")]
    [SerializeField] int strength = 0;
    [SerializeField] int perception = 0;
    [SerializeField] int endurance = 0;
    [SerializeField] int intelligence = 0;
    [SerializeField] int agility = 0;
    [SerializeField] int aberrance = 0;

    [Header("Skills")]
    public int acrobatics = 0;
    public int barter = 0;
    public int blade = 0;
    public int bludgeoning = 0;
    public int enkiism = 0;
    public int erebancy = 0;
    public int explosives = 0;
    public int gaianism = 0;
    public int heavyWeapons = 0;
    public int hermeticism = 0;
    public int iktomancy = 0;
    public int pistols = 0;
    public int polearms = 0;
    public int reshephism = 0;
    public int resourcefulness = 0;
    public int rifles = 0;
    public int sleightOfHand = 0;
    public int throwing = 0;

    [Header("Strength Modifiers")]
    [SerializeField] float equipLoadBase = 20f;
    [SerializeField] float equipLoadPerStrengthPoint = 1f;

    [Header("Perception Modifiers")]
    [SerializeField] float maxFocusPointsBase = 100f;
    [SerializeField] float focusPointsPerPerceptionPoint = 10f;
    [SerializeField] int critChanceBase = 0;
    [SerializeField] int critPercentPerPerceptionPoint = 1;
    [SerializeField] float maxFocusPointsPerLevelPerPerceptionPoint = 2f;
    public float maxFocusPointsPerLevel;

    [Header("Endurance Modifiers")]
    [SerializeField] float maxHitPointsBase = 100f;
    [SerializeField] float hitPointsPerEndurancePoint = 10f;
    [SerializeField] float naturalPoiseBase = 50f;
    [SerializeField] float poisePerEndurancePoint = 5f;
    [SerializeField] float maxHitPointsPerLevelPerEndurancePoint = 2f;
    public float maxHitPointsPerLevel;

    [Header("Intelligence Modifiers")]
    [SerializeField] float maxMagicPointsBase = 100f;
    [SerializeField] float magicPointsPerIntPoint = 1f;
    public static float skillPointDiscountPerIntPoint = 0.025f;
    [SerializeField] float maxMagicPointsPerLevelPerIntelligencePoint = 2f;
    public float maxMagicPointsPerLevel;

    [Header("Agility Modifiers")]
    [SerializeField] float focusPointsPerSecondBase = 1f;
    [SerializeField] float focusPointsPerSecondPerAgilityPoint = 1f;

    [Header("Aberrance Modifiers")]
    [SerializeField] float magicPointsPerSecondBase = 1f;
    [SerializeField] float magicPointsPerSecondPerAberrancePoint = 1f;
    [SerializeField] float critDamageMultiplierBase = 2f;
    [SerializeField] float critDamageMultiplierPerAberrancePoint = .2f;
    public float critDamageMultiplier;
    // psionic tremor modifiers

    [Header("Acrobatics Modifiers")]
    [SerializeField] float baseMovementSpeed = 5f;
    [SerializeField] float moveSpeedPerAcrobaticsPoint = .5f;

    [Header("Barter Modifiers")]
    [SerializeField] float minSellMultiplier = 0.5f;
    [SerializeField] float maxSellMultiplier = 1.5f;
    [SerializeField] float minBuyMultiplier = 0.5f;
    [SerializeField] float maxBuyMultiplier = 2f;
    public float sellMultiplier;
    public float buyMultiplier;

    [Header("Magic Modifer Config")]
    [SerializeField] float minSpellCostMod = 0.5f;
    [SerializeField] float maxSpellCostMod = 1.5f;

    [Header("Enkiism Modifiers")]
    [SerializeField] float enkiismModifierBase = 1f;
    [SerializeField] float enkiismModPerPoint = .02f;
    public float enkiismMod;
    public float enkiismChargeMod;
    public float enkiismCostMod;

    [Header("Erebancy Modifiers")]
    [SerializeField] float erebancyModifierBase = 1f;
    [SerializeField] float erebancyModPerPoint = .02f;
    public float erebancyMod;
    public float erebancyChargeMod;
    public float erebancyCostMod;

    [Header("Gaianism Modifiers")]
    [SerializeField] float gaianismModifierBase = 1f;
    [SerializeField] float gaianismModPerPoint = .02f;
    public float gaianismMod;
    public float gaianismChargeMod;
    public float gaianismCostMod;

    [Header("Hermeticism Modifiers")]
    [SerializeField] float hermeticismModifierBase = 1f;
    [SerializeField] float hermeticismModPerPoint = .02f;
    public float hermeticismMod;
    public float hermeticismChargeMod;
    public float hermeticismCostMod;

    [Header("Iktomancy Modifiers")]
    [SerializeField] float iktomancyModifierBase = 1f;
    [SerializeField] float iktomancyModPerPoint = .02f;
    public float iktomancyMod;
    public float iktomancyChargeMod;
    public float iktomancyCostMod;

    [Header("Reshephism Modifiers")]
    [SerializeField] float reshephismModifierBase = 1f;
    [SerializeField] float reshephismModPerPoint = .02f;
    public float reshephismMod;
    public float reshephismChargeMod;
    public float reshephismCostMod;

    [Header("Resourcefulness Modifiers")]
    [SerializeField] float consumableModiferBase = 0f;
    [SerializeField] float consumableModPerResourcefulnessPoint = .15f;
    public float consumableMod;
    public int itemDiscoveryBase = 100;
    [SerializeField] int itemDiscoveryPerResourcefulness = 3;
    public int itemDiscovery = 100;

    [Header("Sleight of Hand Modifiers")]
    [SerializeField] float reloadSpeedMultiplierBase = 1f;
    [SerializeField] float reloadSpeedMultiplierPerSOHPOint = .02f;
    public float reloadSpeedMultiplier;
    [SerializeField] [Range(0, 1)] float sweetSpotBonusPercentageBase = 0f;
    [SerializeField] [Range(0, 0.01f)] float sweetSpotBonusPercentagePerSOHPoint = 0.005f;
    public float sweetSpotBonusPercentage;

    [Header("Ranged Spread Modifiers")]
    [SerializeField] float heavyWeaponsSpreadModPerPoint = .04f;
    public float heavyWeaponsSpreadMod;
    [SerializeField] float pistolsSpreadModPerPoint = .04f;
    public float pistolsSpreadMod;
    [SerializeField] float riflesSpreadModPerPoint = .04f;
    public float riflesSpreadMod;

    [Header("Ranged Charge Time Modifiers")]
    [SerializeField] float minChargeMod = 0.5f;
    [SerializeField] float maxChargeMod = 1.5f;
    public float heavyWeaponsChargeMod;
    public float pistolsChargeMod;
    public float riflesChargeMod;
    public float throwingChargeMod;

    [Header("Melee Charge Time Modifiers")]
    public float bludgeoningChargeMod;
    public float bladeChargeMod;
    public float polearmsChargeMod;

    [Header("Chances")]
    public int critChance;

    public enum Attributes
    {
        STRENGTH,
        PERCEPTION,
        ENDURANCE,
        INTELLIGENCE,
        AGILITY,
        ABERRANCE
    }

    public enum Skills
    {
        ACROBATICS,
        BARTER,
        BLADE,
        BLUDGEONING,
        ENKIISM,
        EREBANCY,
        EXPLOSIVES,
        GAIANISM,
        HEAVY_WEAPONS,
        HERMETICISM,
        IKTOMANCY,
        PISTOLS,
        POLEARMS,
        RESHEPHISM,
        RESOURCEFULNESS,
        RIFLES,
        SLEIGHT_OF_HAND,
        THROWING
    }

    bool[] proficiencies = new bool[18];

    // cache
    SpellDescriptions spellDescriptions;
    Equipment equipment;
    Focus focus;
    Health health;
    PlayerMovement playerMovement;
    Magic magic;
    Poise poise;

    public int TotalAttributePoints { get => totalAttributePoints; set => totalAttributePoints = value; }
    public int UnusedPoints { get => unusedPoints; set => unusedPoints = value; }
    public int Strength { get => strength; set => strength = value; }
    public int Perception { get => perception; set => perception = value; }
    public int Endurance { get => endurance; set => endurance = value; }
    public int Intelligence { get => intelligence; set => intelligence = value; }
    public int Agility { get => agility; set => agility = value; }
    public int Aberrance { get => aberrance; set => aberrance = value; }
    public bool[] Proficiencies { get => proficiencies; set => proficiencies = value; }

    private void Awake()
    {
        spellDescriptions = FindObjectOfType<SpellDescriptions>();
        equipment = GetComponent<Equipment>();
        focus = GetComponent<Focus>();
        health = GetComponent<Health>();
        playerMovement = GetComponent<PlayerMovement>();
        magic = GetComponent<Magic>();
        poise = GetComponent<Poise>();
    }

    public void SetAttributes(int _strength, int _perception, int _endurance, int _intelligence, int _agility, int _aberrance, int _pointsAvailable, bool increaseStats = true)
    {
        Strength = _strength;
        Perception = _perception;
        Endurance = _endurance;
        Intelligence = _intelligence;
        Agility = _agility;
        Aberrance = _aberrance;

        unusedPoints = _pointsAvailable;

        // calculate starting skills
        ComputeStartingSkills();

        // now calculate everything that attributes affect
        SetStrengthEffects();
        SetPerceptionEffects(increaseStats);
        SetEnduranceEffects(increaseStats, increaseStats);
        SetIntelligenceEffects(increaseStats);
        SetAgilityEffects();
        SetAberranceEffects();
    }

    public void SetSkills(int _acrobatics, int _barter, int _blade, int _bludgeoning, int _enkiism, int _erebancy, int _explosives, int _gaianism, int _heavyWeapons, int _hermeticism, int _iktomancy, int _pistols, int _polearms, int _reshephism, int _resourcefulness, int _rifles, int _sleightOfHand, int _thrown)
    {
        acrobatics = _acrobatics;
        barter = _barter;
        blade = _blade;
        bludgeoning = _bludgeoning;
        enkiism = _enkiism;
        erebancy = _erebancy;
        explosives = _explosives;
        gaianism = _gaianism;
        heavyWeapons = _heavyWeapons;
        hermeticism = _hermeticism;
        iktomancy = _iktomancy;
        pistols = _pistols;
        polearms = _polearms;
        reshephism = _reshephism;
        resourcefulness = _resourcefulness;
        rifles = _rifles;
        sleightOfHand = _sleightOfHand;
        throwing = _thrown;

        // skill effects
        SetSkillEffects();
    }

    private void ComputeStartingSkills()
    {
        acrobatics = StartingSkillEquation(agility);
        barter = StartingSkillEquation(intelligence);
        blade = StartingSkillEquation(strength);
        bludgeoning = StartingSkillEquation(strength);
        enkiism = StartingSkillEquation(intelligence);
        erebancy = StartingSkillEquation(aberrance);
        explosives = StartingSkillEquation(perception);
        gaianism = StartingSkillEquation(aberrance);
        heavyWeapons = StartingSkillEquation(strength);
        hermeticism = StartingSkillEquation(intelligence);
        iktomancy = StartingSkillEquation(aberrance);
        pistols = StartingSkillEquation(agility);
        polearms = StartingSkillEquation(strength);
        reshephism = StartingSkillEquation(intelligence);
        resourcefulness = StartingSkillEquation(endurance);
        rifles = StartingSkillEquation(perception);
        sleightOfHand = StartingSkillEquation(agility);
        throwing = StartingSkillEquation(strength);

        SetSkillEffects();
    }

    private int StartingSkillEquation(int attr)
    {
        return 2 + (2 * attr);
    }

    void SetStrengthEffects()
    {
        // equip load
        equipment.MaxEquipLoad = equipLoadBase + equipLoadPerStrengthPoint * strength;
    }

    void SetPerceptionEffects(bool increaseCurrentFocus)
    {
        // max focus points
        float oldFocus = focus.MaxFocusPoints;
        focus.SetMaxFocusPoints(maxFocusPointsBase + focusPointsPerPerceptionPoint * perception);
        if (increaseCurrentFocus)
            focus.IncreaseFocusPoints(focus.MaxFocusPoints - oldFocus);

        // critical hit chance
        critChance = critChanceBase + critPercentPerPerceptionPoint * perception;

        maxFocusPointsPerLevel = maxFocusPointsPerLevelPerPerceptionPoint * perception;
    }

    void SetEnduranceEffects(bool increaseCurrentHealth, bool increaseCurrentPoise)
    {
        // max hit points
        float oldHealth = health.MaxHealth.GetCurrentValue();
        health.SetMaxHealthBase(maxHitPointsBase + hitPointsPerEndurancePoint * endurance);
        if (increaseCurrentHealth)
            health.IncreaseHealth(health.MaxHealth.GetCurrentValue() - oldHealth);

        // natural poise
        float oldPoise = poise.MaxPoise.GetCurrentValue();
        poise.SetMaxPoiseBase(naturalPoiseBase + poisePerEndurancePoint * endurance);
        if (increaseCurrentPoise)
            poise.IncreasePoise(poise.MaxPoise.GetCurrentValue() - oldPoise);

        maxHitPointsPerLevel = maxHitPointsPerLevelPerEndurancePoint * Endurance;
    }

    void SetIntelligenceEffects(bool increaseCurrentMagic)
    {
        // max magic points
        float oldMagic = magic.MaxMagicPoints;
        magic.SetMaxMagicPoints(maxMagicPointsBase + magicPointsPerIntPoint * intelligence);
        if (increaseCurrentMagic)
            magic.IncreaseMagicPoints(magic.MaxMagicPoints - oldMagic);

        maxMagicPointsPerLevel = maxMagicPointsPerLevelPerIntelligencePoint * intelligence;
    }

    void SetAgilityEffects()
    {
        // focus recharge speed
        focus.SetRegenPerSecond(focusPointsPerSecondBase + focusPointsPerSecondPerAgilityPoint * agility);
    }

    void SetAberranceEffects()
    {
        // psionic tremor modifiers

        // magic point refresh
        magic.SetRegenPerSecond(magicPointsPerSecondBase + magicPointsPerSecondPerAberrancePoint * aberrance);

        // crit damage
        critDamageMultiplier = critDamageMultiplierBase + critDamageMultiplierPerAberrancePoint * aberrance;
    }

    public void SetProficiency(Skills skill)
    {
        proficiencies[(int)skill] = true;

        switch (skill)
        {
            case Skills.ACROBATICS:
                acrobatics += tagBonus;
                break;
            case Skills.BARTER:
                barter += tagBonus;
                break;
            case Skills.BLADE:
                blade += tagBonus;
                break;
            case Skills.BLUDGEONING:
                bludgeoning += tagBonus;
                break;
            case Skills.ENKIISM:
                enkiism += tagBonus;
                break;
            case Skills.EREBANCY:
                erebancy += tagBonus;
                break;
            case Skills.EXPLOSIVES:
                explosives += tagBonus;
                break;
            case Skills.GAIANISM:
                gaianism += tagBonus;
                break;
            case Skills.HEAVY_WEAPONS:
                heavyWeapons += tagBonus;
                break;
            case Skills.HERMETICISM:
                hermeticism += tagBonus;
                break;
            case Skills.IKTOMANCY:
                iktomancy += tagBonus;
                break;
            case Skills.PISTOLS:
                pistols += tagBonus;
                break;
            case Skills.POLEARMS:
                polearms += tagBonus;
                break;
            case Skills.RESHEPHISM:
                reshephism += tagBonus;
                break;
            case Skills.RESOURCEFULNESS:
                resourcefulness += tagBonus;
                break;
            case Skills.RIFLES:
                rifles += tagBonus;
                break;
            case Skills.SLEIGHT_OF_HAND:
                sleightOfHand += tagBonus;
                break;
            case Skills.THROWING:
                throwing += tagBonus;
                break;
        }

        SetSkillEffects();
    }

    void SetSkillEffects()
    {
        SetAcrobaticsEffects();
        SetSleightOfHandEffects();
        SetSpellCastingSkillEffects();
        SetResourcefulnessEffects();
        SetRangedSkillSpreadMods();
        SetChargeMods();
        SetBarterEffects();

        // now set spell descriptions
        spellDescriptions.PushDescriptions(this);
    }

    void SetAcrobaticsEffects()
    {
        playerMovement.InitialMoveSpeed = baseMovementSpeed + moveSpeedPerAcrobaticsPoint * acrobatics;
        playerMovement.MoveSpeed = baseMovementSpeed + moveSpeedPerAcrobaticsPoint * acrobatics;
    }

    void SetBarterEffects()
    {
        sellMultiplier = Mathf.Lerp(minSellMultiplier, maxSellMultiplier, barter / 100f);
        buyMultiplier = Mathf.Lerp(minBuyMultiplier, maxBuyMultiplier, 1f - (barter / 100f));
    }

    void SetSleightOfHandEffects()
    {
        // reload speed
        reloadSpeedMultiplier = reloadSpeedMultiplierBase + reloadSpeedMultiplierPerSOHPOint * sleightOfHand;

        // cast speed


        // sweet spot sizes
        sweetSpotBonusPercentage = sweetSpotBonusPercentageBase + sweetSpotBonusPercentagePerSOHPoint * sleightOfHand;
    }

    void SetRangedSkillSpreadMods()
    {
        heavyWeaponsSpreadMod = heavyWeaponsSpreadModPerPoint * heavyWeapons;
        pistolsSpreadMod = pistolsSpreadModPerPoint * pistols;
        riflesSpreadMod = riflesSpreadModPerPoint * rifles;
    }

    void SetChargeMods()
    {
        // ranged
        heavyWeaponsChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (heavyWeapons / 100f));
        pistolsChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (pistols / 100f));
        riflesChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (rifles / 100f));
        throwingChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (throwing / 100f));

        // melee
        bludgeoningChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (bludgeoning / 100f));
        bladeChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (blade / 100f));
        polearmsChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (polearms / 100f));

        // spells
        enkiismChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (enkiism / 100f));
        erebancyChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (erebancy / 100f));
        gaianismChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (gaianism / 100f));
        hermeticismChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (hermeticism / 100f));
        iktomancyChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (iktomancy / 100f));
        reshephismChargeMod = Mathf.Lerp(minChargeMod, maxChargeMod, 1f - (reshephism / 100f));
    }

    void SetResourcefulnessEffects()
    {
        // consumable mod
        consumableMod = consumableModiferBase + consumableModPerResourcefulnessPoint * resourcefulness;

        // item discovery
        itemDiscovery = itemDiscoveryBase + itemDiscoveryPerResourcefulness * resourcefulness;
    }

    void SetSpellCastingSkillEffects()
    {
        // damage and effects modifiers
        enkiismMod = enkiismModifierBase + enkiismModPerPoint * enkiism;
        erebancyMod = erebancyModifierBase + erebancyModPerPoint * erebancy;
        gaianismMod = gaianismModifierBase + gaianismModPerPoint * gaianism;
        hermeticismMod = hermeticismModifierBase + hermeticismModPerPoint * hermeticism;
        iktomancyMod = iktomancyModifierBase + iktomancyModPerPoint * iktomancy;
        reshephismMod = reshephismModifierBase + reshephismModPerPoint * reshephism;

        // cost modifiers
        enkiismCostMod = Mathf.Lerp(minSpellCostMod, maxSpellCostMod, 1f - (enkiism / 100f));
        erebancyCostMod = Mathf.Lerp(minSpellCostMod, maxSpellCostMod, 1f - (erebancy / 100f));
        gaianismCostMod = Mathf.Lerp(minSpellCostMod, maxSpellCostMod, 1f - (gaianism / 100f));
        hermeticismCostMod = Mathf.Lerp(minSpellCostMod, maxSpellCostMod, 1f - (hermeticism / 100f));
        iktomancyCostMod = Mathf.Lerp(minSpellCostMod, maxSpellCostMod, 1f - (iktomancy / 100f));
        reshephismCostMod = Mathf.Lerp(minSpellCostMod, maxSpellCostMod, 1f - (reshephism / 100f));
    }

    public int GetSkillValue(Skills skill)
    {
        return skill switch
        {
            Skills.ACROBATICS => acrobatics,
            Skills.BARTER => barter,
            Skills.BLADE => blade,
            Skills.BLUDGEONING => bludgeoning,
            Skills.ENKIISM => enkiism,
            Skills.EREBANCY => erebancy,
            Skills.EXPLOSIVES => explosives,
            Skills.GAIANISM => gaianism,
            Skills.HEAVY_WEAPONS => heavyWeapons,
            Skills.HERMETICISM => hermeticism,
            Skills.IKTOMANCY => iktomancy,
            Skills.PISTOLS => pistols,
            Skills.POLEARMS => polearms,
            Skills.RESHEPHISM => reshephism,
            Skills.RESOURCEFULNESS => resourcefulness,
            Skills.RIFLES => rifles,
            Skills.SLEIGHT_OF_HAND => sleightOfHand,
            Skills.THROWING => throwing,
            _ => -1,
        };
    }

    public static int GetLevelCost(int level, int intelligence = 0)
    {
        int x = level - 1;
        float discount = 1f - skillPointDiscountPerIntPoint * intelligence;

        // no real logic to this might wanna mess with it a bit more
        return (int)((( 0.15f * Mathf.Pow(x, 3f)) + 3f * Mathf.Pow(x, 2f) + x * 110f) * discount);
    }

    public void LevelUp(int newLevel, int cost, int _acrobatics, int _barter, int _blade, int _bludgeoning, int _enkiism, int _erebancy, int _explosives, int _gaianism, int _heavyWeapons, int _hermeticism, int _iktomancy, int _pistols, int _polearms, int _reshephism, int _resourcefulness, int _rifles, int _sleightOfHand, int _throwing)
    {
        // set new level
        int levelDiff = newLevel - level;
        level = newLevel;

        // remove currency
        GetComponent<Crypto>().SubtractCurrency(cost);

        // increase max health
        float maxHealthIncrease = levelDiff * maxHitPointsPerLevel;
        health.SetMaxHealthBase(health.MaxHealth.GetCurrentValue() + maxHealthIncrease);
        health.IncreaseHealth(maxHealthIncrease);

        // increase max magic points
        float maxMagicIncrease = levelDiff * maxMagicPointsPerLevel;
        magic.SetMaxMagicPoints(magic.MaxMagicPoints + maxMagicIncrease);
        magic.IncreaseMagicPoints(maxMagicIncrease);

        // increase max focus points
        float maxFocusIncrease = levelDiff * maxFocusPointsPerLevel;
        focus.SetMaxFocusPoints(focus.MaxFocusPoints + maxFocusIncrease);
        focus.IncreaseFocusPoints(maxFocusIncrease);

        // calculate skill values
        SetSkills(_acrobatics, _barter, _blade, _bludgeoning, _enkiism, _erebancy, _explosives, _gaianism, _heavyWeapons, _hermeticism, _iktomancy, _pistols, _polearms, _reshephism, _resourcefulness, _rifles, _sleightOfHand, _throwing);
    }

    public float GetSpellChargeModifier(Skills skill)
    {
        return skill switch
        {
            Skills.ENKIISM => enkiismChargeMod,
            Skills.EREBANCY => erebancyChargeMod,
            Skills.GAIANISM => gaianismChargeMod,
            Skills.HERMETICISM => hermeticismChargeMod,
            Skills.IKTOMANCY => iktomancyChargeMod,
            Skills.RESHEPHISM => reshephismChargeMod,
            _ => -1,
        };
    }

    public float GetSpellChargeModifier(Spell.School school)
    {
        return school switch
        {
            Spell.School.ENKIISM => enkiismChargeMod,
            Spell.School.EREBANCY => erebancyChargeMod,
            Spell.School.GAIANISM => gaianismChargeMod,
            Spell.School.HERMETICISM => hermeticismChargeMod,
            Spell.School.IKTOMANCY => iktomancyChargeMod,
            Spell.School.RESHEPHISM => reshephismChargeMod,
            _ => -1,
        };
    }

    public float GetSpellCostModifier(Spell.School school)
    {
        return school switch
        {
            Spell.School.ENKIISM => enkiismCostMod,
            Spell.School.EREBANCY => erebancyCostMod,
            Spell.School.GAIANISM => gaianismCostMod,
            Spell.School.HERMETICISM => hermeticismCostMod,
            Spell.School.IKTOMANCY => iktomancyCostMod,
            Spell.School.RESHEPHISM => reshephismCostMod,
            _ => -1,
        };
    }

    public static string GetSkillAsString(Skills skill)
    {
        return skill switch
        {
            Skills.ACROBATICS => "Acrobatics",
            Skills.BARTER => "Barter",
            Skills.BLADE => "Blade",
            Skills.BLUDGEONING => "Bludgeoning",
            Skills.ENKIISM => "Enkiism",
            Skills.EREBANCY => "Erebancy",
            Skills.EXPLOSIVES => "Explosives",
            Skills.GAIANISM => "Gaianism",
            Skills.HEAVY_WEAPONS => "Heavy Weapons",
            Skills.HERMETICISM => "Hermeticism",
            Skills.IKTOMANCY => "Iktomancy",
            Skills.PISTOLS => "Pistols",
            Skills.POLEARMS => "Polearms",
            Skills.RESHEPHISM => "Reshephism",
            Skills.RESOURCEFULNESS => "Resourcefulness",
            Skills.RIFLES => "Rifles",
            Skills.SLEIGHT_OF_HAND => "Sleight of Hand",
            Skills.THROWING => "Throwing",
            _ => "",
        };
    }

    public static string GetAttributeAsStringAbbreviated(Attributes attribute)
    {
        return attribute switch
        {
            Attributes.STRENGTH => "STR",
            Attributes.PERCEPTION => "PER",
            Attributes.ENDURANCE => "END",
            Attributes.INTELLIGENCE => "INT",
            Attributes.AGILITY => "AGI",
            Attributes.ABERRANCE => "ABR",
            _ => "",
        };

    }
}