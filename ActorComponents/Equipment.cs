using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Equipment : MonoBehaviour
{
    // weapons
    [Header("Equipped Weapons")]
    [SerializeField] GameObject mainHand;
    [SerializeField] GameObject offHand;

    // outfits
    [Header("Outfit")]
    [SerializeField] DamageThresholds damageThresholds;
    [SerializeField] Poise poise;
    [SerializeField] RuntimeAnimatorController defaultOutfitAnimatorController;
    [SerializeField] Sprite defaultSprite;
    [SerializeField] Sprite defaultMainArm;
    [SerializeField] Sprite defaultOffArm;
    [SerializeField] Outfit equippedOutfit;        // use this to see if an outfit is already equipped

    [Header("Quick Items")]
    [SerializeField] Equippable[] quickEquippables = new Equippable[8];

    // location to spawn weapons
    [Header("Weapon Locations")]
    [SerializeField] string mainHandTransformName;
    [SerializeField] string offHandTransformName;
    [SerializeField] Transform mainHandTransform;
    [SerializeField] Transform offHandTransform;
    [SerializeField] GameObject mainHandArm;
    [SerializeField] GameObject offHandArm;

    [Header("Weight Ratio")]
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] float currentEquipLoad = 0;
    [SerializeField] float maxEquipLoad = 25;

    [Header("Audio")]
    [SerializeField] AudioSource uiSounds;
    [SerializeField] AudioClip weaponEquipSound;
    [SerializeField] float weaponEquipVolume = .5f;
    [SerializeField] AudioClip weaponUnequipSound;
    [SerializeField] float weaponUnequipVolume = .5f;
    [SerializeField] AudioClip spellEquipSound;
    [SerializeField] float spellEquipVolume = .5f;
    [SerializeField] AudioClip spellUnequipSound;
    [SerializeField] float spellUnequipVolume = .5f;
    [SerializeField] AudioClip outfitEquipSound;
    [SerializeField] float outfitEquipVolume = .5f;
    [SerializeField] AudioClip outfitUnequipSound;
    [SerializeField] float outfitUnequipVolume = .5f;

    // cached references
    [Header("Player Components")]
    [SerializeField] Player player;
    [SerializeField] Inventory inventory;
    [SerializeField] AttributeScores attributeScores;
    [SerializeField] StatusEffects statusEffects;

    WeaponWheelManager weaponWheelManager;
    HUDAmmoText hudAmmoText;

    // Equipped item components
    WeaponScript mainhandWeaponScript;
    WeaponScript offhandWeaponScript;

    public float CurrentEquipLoad { get => currentEquipLoad; }
    public float MaxEquipLoad { get => maxEquipLoad; set => maxEquipLoad = value; }
    public float EquipLoadRatio { get => currentEquipLoad / maxEquipLoad; }
    public Equippable[] QuickEquippables { get => quickEquippables; }
    public GameObject MainHand { get => mainHand; set => mainHand = value; }
    public GameObject OffHand { get => offHand; set => offHand = value; }
    public GameObject MainHandArm { get => mainHandArm; set => mainHandArm = value; }
    public GameObject OffHandArm { get => offHandArm; set => offHandArm = value; }
    public Outfit EquippedOutfit { get => equippedOutfit; set => equippedOutfit = value; }

    private void Awake()
    {
        weaponWheelManager = FindObjectOfType<WeaponWheelManager>();
        hudAmmoText = FindObjectOfType<HUDAmmoText>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // get the child transforms
        mainHandTransform = gameObject.transform.Find(mainHandTransformName);
        offHandTransform = gameObject.transform.Find(offHandTransformName);
    }

    public void EquipMainHandWeapon(int itemIndex, bool playEquipSound = true)
    {
        Weapon weapon = (Weapon)inventory.Items[itemIndex].First;

        // if already equipped, unequip
        if (weapon.EquippedMain)
        {
            UnequipMainHandWeapon(itemIndex);
        }
        else
        {
            // if equipped in the offhand, unequip there
            if (weapon.EquippedOff)
            {
                UnequipOffHandWeapon(itemIndex);
            }

            // if another weapon is equipped in the main hand, unequip it
            if (MainHand)
            {
                if (MainHand.GetComponent<EquippedWeapon>())
                {
                    // weapon
                    UnequipMainHandWeapon(inventory.GetIndexOfMainHandWeapon());
                }
                else
                {
                    // spell
                    UnequipMainHandSpell();
                }
            }

            // set the weapon in the inventory as equipped
            weapon.EquippedMain = true;

            // update weapon stats
            DamageCalculation.ModifyWeaponStatsBasedOnAttributes(weapon, attributeScores);

            // instantiate in the main hand a game object with the correct item data
            MainHand = Instantiate(weapon.equippedWeapon, mainHandTransform.position, Quaternion.identity, mainHandTransform);

            // give the item in the main hand the correct data
            MainHand.GetComponent<EquippedWeapon>().itemPair = inventory.Items[itemIndex];
            MainHand.GetComponent<EquippedWeapon>().ItemData = inventory.Items[itemIndex].First;

            // increase equip load
            IncreaseEquipLoad(weapon.Weight);

            // set the weapon's owner
            MainHand.GetComponent<EquippedWeapon>().owner = gameObject;

            // track weapon script
            mainhandWeaponScript = MainHand.GetComponent<WeaponScript>();

            // OnEquip status effects
            if (weapon.OnEquipStatusEffects.Count > 0)
            {
                AddStatusEffectsOnEquip(weapon);
            }

            // if owner is player, update the HUD
            if (player)
            {
                if (MainHand.GetComponent<EquippedWeapon>().ItemData is RangedWeapon rangedWeapon && !rangedWeapon.IsThermal)
                {
                    // thermal weapons don't have ammo text
                    hudAmmoText.SetMainHandRangedWeapon(MainHand);
                }
                else
                {
                    hudAmmoText.SetMainHandMeleeWeapon(MainHand);
                }
            }

            // sound
            if (playEquipSound)
                uiSounds.PlayOneShot(weaponEquipSound, weaponEquipVolume);
        }
    }

    public void EquipMainHandWeapon(Equippable equippable, bool playEquipSound = true)
    {
        Weapon weapon = (Weapon)equippable.item;

        // if already equipped, unequip
        if (weapon.EquippedMain)
        {
            UnequipMainHandWeapon(equippable);
        }
        else
        {
            // if equipped in the offhand, unequip there
            if (weapon.EquippedOff)
            {
                UnequipOffHandWeapon(equippable);
            }

            // if another weapon is equipped in the main hand, unequip it
            if (MainHand)
            {
                if (MainHand.GetComponent<EquippedWeapon>())
                {
                    // weapon
                    UnequipMainHandWeapon(inventory.GetIndexOfMainHandWeapon());
                }
                else
                {
                    // spell
                    UnequipMainHandSpell();
                }
            }

            // set the weapon in the inventory as equipped
            weapon.EquippedMain = true;

            // update weapon stats
            DamageCalculation.ModifyWeaponStatsBasedOnAttributes(weapon, attributeScores);

            // instantiate in the main hand a game object with the correct item data
            MainHand = Instantiate(weapon.equippedWeapon, mainHandTransform.position, Quaternion.identity, mainHandTransform);

            // give the item in the main hand the correct data
            MainHand.GetComponent<EquippedWeapon>().itemPair = inventory.GetPairOfItemEquippedMain(equippable.item);
            MainHand.GetComponent<EquippedWeapon>().ItemData = equippable.item;

            // increase equip load
            IncreaseEquipLoad(weapon.Weight);

            // set the weapon's owner
            MainHand.GetComponent<EquippedWeapon>().owner = gameObject;

            // track weapon script
            mainhandWeaponScript = MainHand.GetComponent<WeaponScript>();

            // OnEquip status effects
            if (weapon.OnEquipStatusEffects.Count > 0)
            {
                AddStatusEffectsOnEquip(weapon);
            }

            // if owner is player, update the HUD
            if (player)
            {
                if (MainHand.GetComponent<EquippedWeapon>().ItemData is RangedWeapon rangedWeapon && !rangedWeapon.IsThermal)
                {
                    hudAmmoText.SetMainHandRangedWeapon(MainHand);
                }
                else
                {
                    hudAmmoText.SetMainHandMeleeWeapon(MainHand);
                }
            }

            // sound
            if (playEquipSound)
                uiSounds.PlayOneShot(weaponEquipSound, weaponEquipVolume);
        }
    }

    public void EquipOffHandWeapon(int itemIndex, bool playEquipSound = true)
    {
        Weapon weapon = (Weapon)inventory.Items[itemIndex].First;

        // if already equipped, unequip
        if (weapon.EquippedOff)
        {
            UnequipOffHandWeapon(itemIndex);
        }
        else
        {
            // if equipped in the main hand, unequip there
            if (weapon.EquippedMain)
            {
                UnequipMainHandWeapon(itemIndex);
            }

            // if another weapon is equipped in the off hand, unequip it
            if (OffHand)
            {
                if (OffHand.GetComponent<EquippedWeapon>())
                {
                    // weapon
                    UnequipOffHandWeapon(inventory.GetIndexOfOffHandWeapon());
                }
                else
                {
                    // spell
                    UnequipOffHandSpell();
                }
            }

            // set the weapon in the inventory as equipped
            weapon.EquippedOff = true;

            // update weapon stats
            DamageCalculation.ModifyWeaponStatsBasedOnAttributes(weapon, attributeScores);

            // instantiate in the main hand a game object with the correct item data
            OffHand = Instantiate(weapon.equippedWeapon, offHandTransform.position, Quaternion.identity, offHandTransform);

            // give the item in the main hand the correct data
            OffHand.GetComponent<EquippedWeapon>().itemPair = inventory.Items[itemIndex];
            OffHand.GetComponent<EquippedWeapon>().ItemData = inventory.Items[itemIndex].First;

            // increase equip load
            IncreaseEquipLoad(weapon.Weight);

            // set the weapon's owner
            OffHand.GetComponent<EquippedWeapon>().owner = gameObject;

            // track weapon script
            offhandWeaponScript = OffHand.GetComponent<WeaponScript>();

            // OnEquip status effects
            if (weapon.OnEquipStatusEffects.Count > 0)
            {
                AddStatusEffectsOnEquip(weapon);
            }

            // if owner is player, update the HUD
            if (player)
            {
                if (OffHand.GetComponent<EquippedWeapon>().ItemData is RangedWeapon rangedWeapon && !rangedWeapon.IsThermal)
                {
                    hudAmmoText.SetOffHandRangedWeapon(OffHand);
                }
                else
                {
                    hudAmmoText.SetOffHandMeleeWeapon(OffHand);
                }
            }

            // sound
            if (playEquipSound)
                uiSounds.PlayOneShot(weaponEquipSound, weaponEquipVolume);
        }
    }

    public void EquipOffHandWeapon(Equippable equippable, bool playEquipSound = true)
    {
        Weapon weapon = (Weapon)equippable.item;

        // if already equipped, unequip
        if (weapon.EquippedOff)
        {
            UnequipOffHandWeapon(equippable);
        }
        else
        {
            // if equipped in the main hand, unequip there
            if (weapon.EquippedMain)
            {
                UnequipMainHandWeapon(equippable);
            }

            // if another weapon is equipped in the off hand, unequip it
            if (OffHand)
            {
                if (OffHand.GetComponent<EquippedWeapon>())
                {
                    // weapon
                    UnequipOffHandWeapon(inventory.GetIndexOfOffHandWeapon());
                }
                else
                {
                    // spell
                    UnequipOffHandSpell();
                }
            }

            // set the weapon in the inventory as equipped
            weapon.EquippedOff = true;

            // update weapon stats
            DamageCalculation.ModifyWeaponStatsBasedOnAttributes(weapon, attributeScores);

            // instantiate in the main hand a game object with the correct item data
            OffHand = Instantiate(weapon.equippedWeapon, offHandTransform.position, Quaternion.identity, offHandTransform);

            // give the item in the main hand the correct data
            OffHand.GetComponent<EquippedWeapon>().itemPair = inventory.GetPairOfItemEquippedOff(equippable.item);
            OffHand.GetComponent<EquippedWeapon>().ItemData = equippable.item;

            // increase equip load
            IncreaseEquipLoad(weapon.Weight);

            // set the weapon's owner
            OffHand.GetComponent<EquippedWeapon>().owner = gameObject;

            // track weapon script
            offhandWeaponScript = OffHand.GetComponent<WeaponScript>();

            // OnEquip status effects
            if (weapon.OnEquipStatusEffects.Count > 0)
            {
                AddStatusEffectsOnEquip(weapon);
            }

            // if owner is player, update the HUD
            if (player)
            {
                if (OffHand.GetComponent<EquippedWeapon>().ItemData is RangedWeapon rangedWeapon && !rangedWeapon.IsThermal)
                {
                    hudAmmoText.SetOffHandRangedWeapon(OffHand);
                }
                else
                {
                    hudAmmoText.SetOffHandMeleeWeapon(OffHand);
                }
            }

            // sound
            if (playEquipSound)
                uiSounds.PlayOneShot(weaponEquipSound, weaponEquipVolume);
        }
    }

    public void UnequipMainHandWeapon(int itemIndex, bool playUnequipSound = true)
    {
        Weapon weapon = (Weapon)inventory.Items[itemIndex].First;

        DamageCalculation.ResetWeaponStats(weapon, attributeScores);

        // if ranged weapon, update ammo count in inventory
        if (weapon is RangedWeapon rangedWeapon)
        {
            if (!rangedWeapon.IsThrowingWeapon && !rangedWeapon.IsThermal)
                rangedWeapon.AmmoInClip = MainHand.GetComponent<WeaponReloader>().AmmoInClip;
        }

        // Remove OnEquip status effects
        if (weapon.OnEquipStatusEffects.Count > 0)
        {
            RemoveStatusEffectsOnUnequip(weapon);
        }

        // set equipped to false
        ((Weapon)inventory.Items[itemIndex].First).EquippedMain = false;

        // clear HUD if owner is player
        // if owner is player, update the HUD
        if (player)
        {
            hudAmmoText.ClearMainHand();
        }

        // decrease equip load
        DecreaseEquipLoad(weapon.Weight);

        CancelMainhandActions();

        // release main hand weapon script
        mainhandWeaponScript = null;

        // destroy main hand
        Destroy(MainHand.transform.parent.gameObject);
        MainHand = null;

        // sound
        if (playUnequipSound)
            uiSounds.PlayOneShot(weaponUnequipSound, weaponUnequipVolume);
    }

    public void UnequipMainHandWeapon(Equippable equippable, bool playUnequipSound = true)
    {
        DamageCalculation.ResetWeaponStats((Weapon)equippable.item, attributeScores);

        // if ranged weapon, update ammo count in inventory
        if (equippable.item is RangedWeapon rangedWeapon)
        {
            if (!rangedWeapon.IsThrowingWeapon && !rangedWeapon.IsThermal)
                rangedWeapon.AmmoInClip = MainHand.GetComponent<WeaponReloader>().AmmoInClip;
        }

        // Remove OnEquip status effects
        if (equippable.item.OnEquipStatusEffects.Count > 0)
        {
            RemoveStatusEffectsOnUnequip(equippable.item);
        }

        // set equipped to false
        equippable.item.EquippedMain = false;

        // clear HUD if owner is player
        // if owner is player, update the HUD
        if (player)
        {
            hudAmmoText.ClearMainHand();
        }

        // decrease equip load
        DecreaseEquipLoad(equippable.item.Weight);

        CancelMainhandActions();

        // release main hand weapon script
        mainhandWeaponScript = null;

        // destroy main hand
        Destroy(MainHand.transform.parent.gameObject);
        MainHand = null;

        // sound
        if (playUnequipSound)
            uiSounds.PlayOneShot(weaponUnequipSound, weaponUnequipVolume);
    }

    public void UnequipOffHandWeapon(int itemIndex, bool playUnequipSound = true)
    {
        Weapon weapon = (Weapon)inventory.Items[itemIndex].First;

        DamageCalculation.ResetWeaponStats(weapon, attributeScores);

        // if ranged weapon, update ammo count in inventory
        if (weapon is RangedWeapon rangedWeapon)
        {
            if (!rangedWeapon.IsThrowingWeapon && !rangedWeapon.IsThermal)
                rangedWeapon.AmmoInClip = OffHand.GetComponent<WeaponReloader>().AmmoInClip;
        }

        // Remove OnEquip status effects
        if (weapon.OnEquipStatusEffects.Count > 0)
        {
            RemoveStatusEffectsOnUnequip(weapon);
        }

        // set equipped to false
        weapon.EquippedOff = false;

        // if owner is player, update the HUD
        if (player)
        {
            hudAmmoText.ClearOffHand();
        }

        // decrease equip load
        DecreaseEquipLoad(weapon.Weight);

        CancelOffhandActions();

        // release off hand weapon script
        offhandWeaponScript = null;

        // destroy offhand
        Destroy(OffHand.transform.parent.gameObject);
        OffHand = null;

        // sound
        if (playUnequipSound)
            uiSounds.PlayOneShot(weaponUnequipSound, weaponUnequipVolume);
    }

    public void UnequipOffHandWeapon(Equippable equippable, bool playUnequipSound = true)
    {
        DamageCalculation.ResetWeaponStats((Weapon)equippable.item, attributeScores);

        // if ranged weapon, update ammo count in inventory
        if (equippable.item is RangedWeapon weapon)
        {
            if (!weapon.IsThrowingWeapon && !weapon.IsThermal)
                weapon.AmmoInClip = OffHand.GetComponent<WeaponReloader>().AmmoInClip;
        }

        // Remove OnEquip status effects
        if (equippable.item.OnEquipStatusEffects.Count > 0)
        {
            RemoveStatusEffectsOnUnequip(equippable.item);
        }

        // set equipped to false
        equippable.item.EquippedOff = false;

        // clear HUD if owner is player
        // if owner is player, update the HUD
        if (player)
        {
            hudAmmoText.ClearOffHand();
        }

        // decrease equip load
        DecreaseEquipLoad(equippable.item.Weight);

        CancelOffhandActions();

        // release off hand weapon script
        offhandWeaponScript = null;

        // destroy main hand
        Destroy(OffHand.transform.parent.gameObject);
        OffHand = null;

        // sound
        if (playUnequipSound)
            uiSounds.PlayOneShot(weaponUnequipSound, weaponUnequipVolume);
    }

    // spells can be equipped in both hands (they aren't physical objects) so we can skip some checks
    public void EquipMainHandSpell(Spell spell, bool playEquipSound = true)
    {
        bool sameSpell = false;

        // unequip whatever's in the main hand
        if (MainHand)
        {
            if (MainHand.GetComponent<EquippedWeapon>())
            {
                // weapon
                UnequipMainHandWeapon(inventory.GetIndexOfMainHandWeapon());
            }
            else if (MainHand.GetComponent<EquippedSpell>().Spell.SpellID == spell.SpellID)
            {
                // this is the same spell
                UnequipMainHandSpell(playEquipSound);
                sameSpell = true;
            }
            else
            {
                // spell
                UnequipMainHandSpell(playEquipSound);
            }
        }

        if (!sameSpell)
        {
            // equip if not same spell
            MainHand = Instantiate(spell.equippedSpell, mainHandTransform);
            spell.EquippedMain = true;

            MainHand.GetComponent<EquippedSpell>().Owner = gameObject;
            MainHand.GetComponent<EquippedSpell>().isMainHand = true;
            MainHand.GetComponent<EquippedSpell>().Spell = spell;

            // track weapon script
            mainhandWeaponScript = MainHand.GetComponent<WeaponScript>();

            // set UI
            hudAmmoText.SetMainHandSpell(spell);

            // sound
            if (playEquipSound)
                uiSounds.PlayOneShot(spellEquipSound, spellEquipVolume);
        }
    }

    public void EquipOffHandSpell(Spell spell, bool playEquipSound = true)
    {
        bool sameSpell = false;

        // unequip whatever's in the main hand
        if (OffHand)
        {
            if (OffHand.GetComponent<EquippedWeapon>())
            {
                // weapon
                UnequipOffHandWeapon(inventory.GetIndexOfOffHandWeapon());
            }
            else if (OffHand.GetComponent<EquippedSpell>().Spell.SpellID == spell.SpellID)
            {
                // this is the same spell
                UnequipOffHandSpell(playEquipSound);
                sameSpell = true;
            }
            else
            {
                // spell
                UnequipOffHandSpell(playEquipSound);
            }
        }

        if (!sameSpell)
        {
            // equip if not same spell
            OffHand = Instantiate(spell.equippedSpell, offHandTransform);
            spell.EquippedOff = true;

            OffHand.GetComponent<EquippedSpell>().Owner = gameObject;
            OffHand.GetComponent<EquippedSpell>().isMainHand = false;
            OffHand.GetComponent<EquippedSpell>().Spell = spell;

            // track weapon script
            offhandWeaponScript = OffHand.GetComponent<WeaponScript>();

            // set UI
            hudAmmoText.SetOffHandSpell(spell);

            // sound
            if (playEquipSound)
                uiSounds.PlayOneShot(spellEquipSound, spellEquipVolume);
        }
    }

    void UnequipMainHandSpell(bool playUnequipSound = true)
    {
        EquippedSpell equippedSpell = MainHand.GetComponent<EquippedSpell>();
        equippedSpell.Spell.EquippedMain = false;

        CancelMainhandActions();

        // null weapon script
        mainhandWeaponScript = null;

        Destroy(MainHand.transform.parent.gameObject);
        MainHand = null;

        // clear hud
        hudAmmoText.ClearMainHand();

        // sound
        if (playUnequipSound)
            uiSounds.PlayOneShot(spellUnequipSound, spellUnequipVolume);
    }

    void UnequipOffHandSpell(bool playUnequipSound = true)
    {
        EquippedSpell equippedSpell = OffHand.GetComponent<EquippedSpell>();
        equippedSpell.Spell.EquippedOff = false;

        CancelOffhandActions();

        // null weapon script
        offhandWeaponScript = null;

        Destroy(OffHand.transform.parent.gameObject);
        OffHand = null;

        // clear hud
        hudAmmoText.ClearOffHand();

        // sound
        if (playUnequipSound)
            uiSounds.PlayOneShot(spellUnequipSound, spellUnequipVolume);
    }

    // unequips whatever is in both hands
    public void UnequipHands(bool playUnequipSound = true)
    {
        if (MainHand)
        {
            EquippedWeapon mainHandEquippedWeapon = MainHand.GetComponent<EquippedWeapon>();
            if (mainHandEquippedWeapon)
            {
                Equippable newWeapon = new Equippable(mainHandEquippedWeapon.itemPair.First);
                UnequipMainHandWeapon(newWeapon, playUnequipSound);
            }
            else
            {
                EquippedSpell mainHandEquippedSpell = MainHand.GetComponent<EquippedSpell>();
                EquipMainHandSpell(mainHandEquippedSpell.Spell, playUnequipSound);
            }
        }

        if (OffHand)
        {
            EquippedWeapon offHandEquippedWeapon = OffHand.GetComponent<EquippedWeapon>();
            if (offHandEquippedWeapon)
            {
                Equippable newWeapon = new Equippable(offHandEquippedWeapon.itemPair.First);
                UnequipOffHandWeapon(newWeapon, playUnequipSound);
            }
            else
            {
                EquippedSpell offHandEquippedSpell = OffHand.GetComponent<EquippedSpell>();
                EquipOffHandSpell(offHandEquippedSpell.Spell, playUnequipSound);
            }
        }
    }

    public void EquipOutfit(int itemIndex, bool playEquipSound = true)
    {
        Outfit outfit = (Outfit)inventory.Items[itemIndex].First;

        if (!EquippedOutfit)
        {
            // no outfit equipped. Equip this one.
            outfit.usable.Use();
            EquippedOutfit = outfit;
            outfit.Equipped = true;

            // increase equip load
            IncreaseEquipLoad(outfit.Weight);

            // increase poise
            poise.IncreaseMaxPoiseBase(outfit.Poise, false);

            // change DT
            damageThresholds.SetThresholds(outfit.BluntDT, outfit.PiercingDT, outfit.SlashingDT, outfit.EnergyDT, outfit.FireDT, outfit.FrostDT, outfit.ShockDT, outfit.AcidDT, outfit.PoisonDT, outfit.PsionicDT);

            // set thresholds
            AddOutfitStatusEffectThresholds(outfit);

            // OnEquip status effects
            if (outfit.OnEquipStatusEffects.Count > 0)
            {
                AddStatusEffectsOnEquip(outfit);
            }

            // sound
            if (playEquipSound)
                uiSounds.PlayOneShot(outfitEquipSound, outfitEquipVolume);
        }
        else
        {
            // outfit exists
            if (outfit.ItemID == EquippedOutfit.ItemID)
            {
                UnequipOutfit();
            }
            else
            {
                // replacing an outfit
                // decrease equip load
                DecreaseEquipLoad(EquippedOutfit.Weight);

                // decrease poise
                poise.DecreaseMaxPoiseBase(EquippedOutfit.Poise, true);

                EquippedOutfit.Equipped = false;        // unequip current outfit
                outfit.usable.Use();                    // put new outfit on
                EquippedOutfit = outfit;                // set it as the equipped outfit
                outfit.Equipped = true;                 // set new as equipped

                // increase equip load
                IncreaseEquipLoad(outfit.Weight);

                // increase poise
                poise.IncreaseMaxPoiseBase(outfit.Poise, false);

                // change DT
                damageThresholds.SetThresholds(outfit.BluntDT, outfit.PiercingDT, outfit.SlashingDT, outfit.EnergyDT, outfit.FireDT, outfit.FrostDT, outfit.ShockDT, outfit.AcidDT, outfit.PoisonDT, outfit.PsionicDT);

                // set thresholds
                AddOutfitStatusEffectThresholds(outfit);

                // OnEquip status effects
                if (outfit.OnEquipStatusEffects.Count > 0)
                {
                    AddStatusEffectsOnEquip(outfit);
                }

                // sound
                if (playEquipSound)
                    uiSounds.PlayOneShot(outfitEquipSound, outfitEquipVolume);
            }
        }
    }

    public void UnequipOutfit(bool playUnequipSound = true)
    {
        // remove OnEquip status effects
        if (EquippedOutfit.OnEquipStatusEffects.Count > 0)
        {
            RemoveStatusEffectsOnUnequip(EquippedOutfit);
        }

        // decrease equip load
        DecreaseEquipLoad(EquippedOutfit.Weight);

        // decrease poise
        poise.DecreaseMaxPoiseBase(EquippedOutfit.Poise, true);

        // zero out DT
        damageThresholds.SetThresholds();

        // decrease thresholds
        RemoveOutfitStatusEffectThresholds(EquippedOutfit);

        // outfit already equipped, unequip
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
        EquippedOutfit.Equipped = false;
        EquippedOutfit = null;

        // reset stats
        OutfitSetup.InitializeNoOutfit();

        // reset arms
        SpriteRenderer mainArm = MainHandArm.GetComponent<SpriteRenderer>();
        SpriteRenderer offArm = OffHandArm.GetComponent<SpriteRenderer>();
        mainArm.sprite = defaultMainArm;
        offArm.sprite = defaultOffArm;

        // reset animation
        GetComponent<Animator>().runtimeAnimatorController = defaultOutfitAnimatorController;

        // sound
        if (playUnequipSound)
            uiSounds.PlayOneShot(outfitUnequipSound, outfitUnequipVolume);
    }

    private void AddOutfitStatusEffectThresholds(Outfit outfit)
    {
        statusEffects.BurningThreshold += outfit.BurningThreshold;
        statusEffects.FrostThreshold += outfit.FreezingThreshold;
        statusEffects.DecayThreshold += outfit.DecayThreshold;
        statusEffects.ShockedThreshold += outfit.ShockedThreshold;
        statusEffects.PoisonedThreshold += outfit.PoisonedThreshold;
        statusEffects.BleedThreshold += outfit.BleedThreshold;

        statusEffects.BleedReduction += outfit.BleedReduction;
    }

    private void RemoveOutfitStatusEffectThresholds(Outfit outfit)
    {
        statusEffects.BurningThreshold -= outfit.BurningThreshold;
        statusEffects.FrostThreshold -= outfit.FreezingThreshold;
        statusEffects.DecayThreshold -= outfit.DecayThreshold;
        statusEffects.ShockedThreshold -= outfit.ShockedThreshold;
        statusEffects.PoisonedThreshold -= outfit.PoisonedThreshold;
        statusEffects.BleedThreshold -= outfit.BleedThreshold;

        statusEffects.BleedReduction -= outfit.BleedReduction;
    }

    private void IncreaseEquipLoad(float newWeight)
    {
        currentEquipLoad += newWeight;
        playerMovement.UpdateMovementWithEquipLoad(EquipLoadRatio);
    }

    private void DecreaseEquipLoad(float oldWeight)
    {
        currentEquipLoad -= oldWeight;
        playerMovement.UpdateMovementWithEquipLoad(EquipLoadRatio);
    }

    public void PlaceEquippable(Item item, int index)
    {
        Equippable equippable = new Equippable(item);

        int checkDup = GetItemIndexFromQuickEquippables(item);

        // check if this equippable is already in the list
        if (checkDup != -1)
        {
            // item exists in quick equippables -- clear its old slot
            ClearQuickWeaponSlot(checkDup);
        }

        item.QuickEquippedIndex = index;

        QuickEquippables[index] = equippable;

        weaponWheelManager.SetWeaponWheelButton(equippable, index);
    }

    public void PlaceEquippable(Spell spell, int index)
    {
        Equippable equippable = new Equippable(spell);

        int checkDup = GetSpellIndexFromQuickEquippables(spell);

        // check if this equippable is already in the list
        if (checkDup != -1)
        {
            // item exists in quick equippables -- clear its old slot
            ClearQuickWeaponSlot(checkDup);
        }

        spell.QuickEquippedIndex = index;

        QuickEquippables[index] = equippable;

        weaponWheelManager.SetWeaponWheelButton(equippable, index);
    }

    public void ClearQuickWeaponSlot(int index)
    {
        if (index > -1)
        {
            Equippable emptyEquippable = new Equippable();

            // set as not quick equipped
            if (QuickEquippables[index].equippableType == Equippable.EquippableType.ITEM)
            {
                QuickEquippables[index].item.QuickEquippedIndex = -1;
            }
            else if (QuickEquippables[index].equippableType == Equippable.EquippableType.SPELL)
            {
                QuickEquippables[index].spell.QuickEquippedIndex = -1;
            }

            QuickEquippables[index] = emptyEquippable;

            weaponWheelManager.SetWeaponWheelButton(emptyEquippable, index);
        }
    }

    private int GetItemIndexFromQuickEquippables(Item item)
    {
        for (int i = 0; i < quickEquippables.Length; i++)
        {
            if (quickEquippables[i].equippableType == Equippable.EquippableType.ITEM)
            {
                if (quickEquippables[i].item == item)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private int GetSpellIndexFromQuickEquippables(Spell spell)
    {
        for (int i = 0; i < quickEquippables.Length; i++)
        {
            if (quickEquippables[i].equippableType == Equippable.EquippableType.SPELL)
            {
                if (quickEquippables[i].spell.SpellID == spell.SpellID)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public Equippable GetMainhandAsEquippable()
    {
        if (MainHand)
        {
            EquippedWeapon mainHandEquippedWeapon = MainHand.GetComponent<EquippedWeapon>();
            if (mainHandEquippedWeapon)
            {
                Equippable newWeapon = new Equippable(mainHandEquippedWeapon.itemPair.First);
                return newWeapon;
            }
            else
            {
                EquippedSpell mainHandEquippedSpell = MainHand.GetComponent<EquippedSpell>();
                Equippable newSpell = new Equippable(mainHandEquippedSpell.Spell);
                return newSpell;
            }
        }
        else
        {
            return null;
        }
    }

    public Equippable GetOffhandAsEquippable()
    {
        if (OffHand)
        {
            EquippedWeapon offHandEquippedWeapon = OffHand.GetComponent<EquippedWeapon>();
            if (offHandEquippedWeapon)
            {
                Equippable newWeapon = new Equippable(offHandEquippedWeapon.itemPair.First);
                return newWeapon;
            }
            else
            {
                EquippedSpell offHandEquippedSpell = OffHand.GetComponent<EquippedSpell>();
                Equippable newSpell = new Equippable(offHandEquippedSpell.Spell);
                return newSpell;
            }
        }
        else
        {
            return null;
        }
    }

    public int GetMainhandAmmoInClip()
    {
        if (MainHand)
        {
            WeaponReloader mainWeaponReloader = MainHand.GetComponent<WeaponReloader>();
            if (mainWeaponReloader)
            {
                return mainWeaponReloader.AmmoInClip;
            }
        }

        return -1;
    }

    public int GetOffhandAmmoInClip()
    {
        if (OffHand)
        {
            WeaponReloader offWeaponReloader = OffHand.GetComponent<WeaponReloader>();
            if (offWeaponReloader)
            {
                return offWeaponReloader.AmmoInClip;
            }
        }

        return -1;
    }

    // cancels events such as reloading, charging spells, burning constant cast spells, charging energy weapons, etc.
    public void CancelMainhandActions(bool destroyHeatSlider = true)
    {
        // cancel reloads
        WeaponReloader reloader = MainHand.GetComponent<WeaponReloader>();
        if (reloader)
        {
            if (reloader.IsReloading)
            {
                reloader.CancelReload();
            }
        }

        // cancel tech weapons
        TechWeapon techWeapon = MainHand.GetComponent<TechWeapon>();
        if (techWeapon)
        {
            if (techWeapon.equippedWeapon.IsTechCharging)
            {
                techWeapon.CancelCharge();
            }
        }


        // cancel spells
        SpellScript spellScript = MainHand.GetComponent<SpellScript>();
        if (spellScript)
        {
            if (spellScript.IsCharging)
            {
                spellScript.CancelCharge();
            }

            if (spellScript.IsConstantCasting)
            {
                spellScript.StopConstantCast();
                spellScript.StopMPBurn();
            }
        }

        // cancel laser weapons
        LaserWeapon laserWeapon = MainHand.GetComponent<LaserWeapon>();
        if (laserWeapon)
        {
            if (laserWeapon.equippedWeapon.IsTechCharging)
            {
                laserWeapon.CancelCharge();
            }
        }

        // cancel melee weapons
        MeleeSwingWeapon meleeWeapon = MainHand.GetComponent<MeleeSwingWeapon>();
        if (meleeWeapon)
        {
            if (meleeWeapon.equippedWeapon.IsCharging)
            {
                meleeWeapon.CancelCharge();
            }
        }

        // cancel throwing weapons
        ThrowingWeapon throwingWeapon = MainHand.GetComponent<ThrowingWeapon>();
        if (throwingWeapon)
        {
            if (throwingWeapon.equippedWeapon.IsCharging)
            {
                throwingWeapon.CancelCharge();
            }
        }

        // cancel thermal ranged weapon
        ThermalWeaponRanged thermalWeaponRanged = MainHand.GetComponent<ThermalWeaponRanged>();
        if (thermalWeaponRanged)
        {
            if (thermalWeaponRanged.equippedWeapon.IsCharging)
            {
                thermalWeaponRanged.CancelCharge();
            }

            // destroy heat slider
            if (destroyHeatSlider)
                thermalWeaponRanged.DestroyHeatSlider();
        }
    }

    public void CancelOffhandActions(bool destroyHeatSlider = true)
    {
        // cancel reloads
        WeaponReloader reloader = OffHand.GetComponent<WeaponReloader>();
        if (reloader)
        {
            if (reloader.IsReloading)
            {
                reloader.CancelReload();
            }
        }

        // cancel tech weapons
        TechWeapon techWeapon = OffHand.GetComponent<TechWeapon>();
        if (techWeapon)
        {
            if (techWeapon.equippedWeapon.IsTechCharging)
            {
                techWeapon.CancelCharge();
            }
        }


        // cancel spells
        SpellScript spellScript = OffHand.GetComponent<SpellScript>();
        if (spellScript)
        {
            if (spellScript.IsCharging)
            {
                spellScript.CancelCharge();
            }

            if (spellScript.IsConstantCasting)
            {
                spellScript.StopConstantCast();
                spellScript.StopMPBurn();
            }
        }

        // cancel laser weapons
        LaserWeapon laserWeapon = OffHand.GetComponent<LaserWeapon>();
        if (laserWeapon)
        {
            if (laserWeapon.equippedWeapon.IsTechCharging)
            {
                laserWeapon.CancelCharge();
            }
        }

        // cancel melee weapons
        MeleeSwingWeapon meleeWeapon = OffHand.GetComponent<MeleeSwingWeapon>();
        if (meleeWeapon)
        {
            if (meleeWeapon.equippedWeapon.IsCharging)
            {
                meleeWeapon.CancelCharge();
            }
        }

        // cancel throwing weapons
        ThrowingWeapon throwingWeapon = OffHand.GetComponent<ThrowingWeapon>();
        if (throwingWeapon)
        {
            if (throwingWeapon.equippedWeapon.IsCharging)
            {
                throwingWeapon.CancelCharge();
            }
        }

        // cancel thermal ranged weapon
        ThermalWeaponRanged thermalWeaponRanged = OffHand.GetComponent<ThermalWeaponRanged>();
        if (thermalWeaponRanged)
        {
            if (thermalWeaponRanged.equippedWeapon.IsCharging)
            {
                thermalWeaponRanged.CancelCharge();
            }

            // destroy heat slider
            if (destroyHeatSlider)
                thermalWeaponRanged.DestroyHeatSlider();
        }
    }

    /* --- STATUS EFFECTS --- */
    private void AddStatusEffectsOnEquip(Item item)
    {
        if (statusEffects)
        {
            foreach (StatusEffect template in item.OnEquipStatusEffects)
            {
                StatusEffect statusEffect = Instantiate(template);

                // Add status effect. Set the item as the initiator.
                statusEffects.AddStatusEffect(statusEffect, item);
            }
        }
    }

    private void RemoveStatusEffectsOnUnequip(Item item)
    {
        if (statusEffects)
        {
            // Remove all status effects that this item initiated
            statusEffects.RemoveStatusEffectsOfInitiator(item);
        }
    }

    /* --- QUICK EQUIP --- */
    public int GetActiveQuickAidIndex()
    {
        return FindObjectOfType<QuickAidManager>().ActiveKey;
    }

    /* --- INPUT --- */
    public void UseMainHand(InputAction.CallbackContext context)
    {
        if (mainhandWeaponScript)
        {
            mainhandWeaponScript.Use(context);
        }
    }

    public void UseOffHand(InputAction.CallbackContext context)
    {
        if (offhandWeaponScript)
        {
            offhandWeaponScript.Use(context);
        }
    }

    public void ReleaseMainHand(InputAction.CallbackContext context)
    {
        if (mainhandWeaponScript)
        {
            mainhandWeaponScript.Release(context);
        }
    }

    public void ReleaseOffHand(InputAction.CallbackContext context)
    {
        if (offhandWeaponScript)
        {
            offhandWeaponScript.Release(context);
        }
    }

    public void UtilityMainHand(InputAction.CallbackContext context)
    {
        if (mainhandWeaponScript)
        {
            mainhandWeaponScript.Utility(context);
        }
    }

    public void UtilityOffHand(InputAction.CallbackContext context)
    {
        if (offhandWeaponScript)
        {
            offhandWeaponScript.Utility(context);
        }
    }

    public void InterruptMainHand()
    {
        if (mainhandWeaponScript)
        {
            mainhandWeaponScript.Interrupt();
        }
    }

    public void InterruptOffHand()
    {
        if (offhandWeaponScript)
        {
            offhandWeaponScript.Interrupt();
        }
    }

    public void InterruptHands()
    {
        InterruptMainHand();
        InterruptOffHand();
    }

    public void InterruptMainHandMagic()
    {
        if (mainhandWeaponScript)
        {
            mainhandWeaponScript.InterruptMagic();
        }
    }

    public void InterruptOffHandMagic()
    {
        if (offhandWeaponScript)
        {
            offhandWeaponScript.InterruptMagic();
        }
    }

    public void InterruptHandsMagic()
    {
        InterruptMainHandMagic();
        InterruptOffHandMagic();
    }
}
