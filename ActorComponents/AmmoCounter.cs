using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCounter : MonoBehaviour
{
    [Header("Ammo")]
    public Pair<Item, int> nineMill;
    public Pair<Item, int> fivefivesix;
    public Pair<Item, int> twelveGauge;
    public Pair<Item, int> fiveMill;
    public Pair<Item, int> threeZeroEight;
    public Pair<Item, int> rocket;
    public Pair<Item, int> energyCell;
    public Pair<Item, int> fortyFourMagnum;
    public Pair<Item, int> fiftyCaliber;
    public Pair<Item, int> plasmaCell;
    public Pair<Item, int> flare;
    public Pair<Item, int> arrow;
    public Pair<Item, int> cryoCell;
    public Pair<Item, int> twentyTwoCaliber;
    public Pair<Item, int> fuelCanister;
    public Pair<Item, int> sawblade;

    [SerializeField] Inventory inventory;

    public void AssignAmmoType(Pair<Item, int> itemPair)
    {
        Ammo ammo = (Ammo)itemPair.First;

        switch (ammo.AmmunitionType)
        {
            case Item.AmmoType.NINEMILL:
                nineMill = itemPair;
                break;
            case Item.AmmoType.NATOFIVEFIVESIX:
                fivefivesix = itemPair;
                break;
            case Item.AmmoType.TWELVEGAUGE:
                twelveGauge = itemPair;
                break;
            case Item.AmmoType.FIVEMILL:
                fiveMill = itemPair;
                break;
            case Item.AmmoType.THREEZEROEIGHT:
                threeZeroEight = itemPair;
                break;
            case Item.AmmoType.ROCKET:
                rocket = itemPair;
                break;
            case Item.AmmoType.ENERGYCELL:
                energyCell = itemPair;
                break;
            case Item.AmmoType.FORTYFOURMAGNUM:
                fortyFourMagnum = itemPair;
                break;
            case Item.AmmoType.FIFTYCALIBER:
                fiftyCaliber = itemPair;
                break;
            case Item.AmmoType.PLASMA_CELL:
                plasmaCell = itemPair;
                break;
            case Item.AmmoType.FLARE:
                flare = itemPair;
                break;
            case Item.AmmoType.ARROW:
                arrow = itemPair;
                break;
            case Item.AmmoType.CRYO_CELL:
                cryoCell = itemPair;
                break;
            case Item.AmmoType.TWENTYTWOCALIBER:
                twentyTwoCaliber = itemPair;
                break;
            case Item.AmmoType.FUEL_CANISTER:
                fuelCanister = itemPair;
                break;
            case Item.AmmoType.BUZZSAW:
                sawblade = itemPair;
                break;
        }
    }

    public bool DecreaseAmmoCount(Item.AmmoType ammoType, int quantity)
    {
        return ammoType switch
        {
            Item.AmmoType.NINEMILL => inventory.RemoveAmmo(nineMill, quantity),
            Item.AmmoType.NATOFIVEFIVESIX => inventory.RemoveAmmo(fivefivesix, quantity),
            Item.AmmoType.TWELVEGAUGE => inventory.RemoveAmmo(twelveGauge, quantity),
            Item.AmmoType.FIVEMILL => inventory.RemoveAmmo(fiveMill, quantity),
            Item.AmmoType.THREEZEROEIGHT => inventory.RemoveAmmo(threeZeroEight, quantity),
            Item.AmmoType.ROCKET => inventory.RemoveAmmo(rocket, quantity),
            Item.AmmoType.ENERGYCELL => inventory.RemoveAmmo(energyCell, quantity),
            Item.AmmoType.FORTYFOURMAGNUM => inventory.RemoveAmmo(fortyFourMagnum, quantity),
            Item.AmmoType.FIFTYCALIBER => inventory.RemoveAmmo(fiftyCaliber, quantity),
            Item.AmmoType.PLASMA_CELL => inventory.RemoveAmmo(plasmaCell, quantity),
            Item.AmmoType.FLARE => inventory.RemoveAmmo(flare, quantity),
            Item.AmmoType.ARROW => inventory.RemoveAmmo(arrow, quantity),
            Item.AmmoType.CRYO_CELL => inventory.RemoveAmmo(cryoCell, quantity),
            Item.AmmoType.TWENTYTWOCALIBER => inventory.RemoveAmmo(twentyTwoCaliber, quantity),
            Item.AmmoType.FUEL_CANISTER => inventory.RemoveAmmo(fuelCanister, quantity),
            Item.AmmoType.BUZZSAW => inventory.RemoveAmmo(sawblade, quantity),
            _ => false,
        };
    }

    public int GetAmmoCountByType(Item.AmmoType ammoType)
    {
        return ammoType switch
        {
            Item.AmmoType.NINEMILL => (nineMill != null) ? nineMill.Second : 0,
            Item.AmmoType.NATOFIVEFIVESIX => (fivefivesix != null) ? fivefivesix.Second : 0,
            Item.AmmoType.TWELVEGAUGE => (twelveGauge != null) ? twelveGauge.Second : 0,
            Item.AmmoType.FIVEMILL => (fiveMill != null) ? fiveMill.Second : 0,
            Item.AmmoType.THREEZEROEIGHT => (threeZeroEight != null) ? threeZeroEight.Second : 0,
            Item.AmmoType.ROCKET => (rocket != null) ? rocket.Second : 0,
            Item.AmmoType.ENERGYCELL => (energyCell != null) ? energyCell.Second : 0,
            Item.AmmoType.FORTYFOURMAGNUM => (fortyFourMagnum != null) ? fortyFourMagnum.Second : 0,
            Item.AmmoType.FIFTYCALIBER => (fiftyCaliber != null) ? fiftyCaliber.Second : 0,
            Item.AmmoType.PLASMA_CELL => (plasmaCell != null) ? plasmaCell.Second : 0,
            Item.AmmoType.FLARE => (flare != null) ? flare.Second : 0,
            Item.AmmoType.ARROW => (arrow != null) ? arrow.Second : 0,
            Item.AmmoType.CRYO_CELL => (cryoCell != null) ? cryoCell.Second : 0,
            Item.AmmoType.TWENTYTWOCALIBER => (twentyTwoCaliber != null) ? twentyTwoCaliber.Second : 0,
            Item.AmmoType.FUEL_CANISTER => (fuelCanister != null) ? fuelCanister.Second : 0,
            Item.AmmoType.BUZZSAW => (sawblade != null) ? sawblade.Second : 0,
            _ => 0,
        };
    }

    // pull quantity from reserves without going below zero
    public int PullAmmoFromReserves(Item.AmmoType ammoType, int quantity)
    {
        // return the quantity requested if there's room
        if (GetAmmoCountByType(ammoType) - quantity >= 0)
        {
            DecreaseAmmoCount(ammoType, quantity);
            return quantity;
        }
        else
        {
            // otherwise return everything in the reserves
            int returnVal = GetAmmoCountByType(ammoType);
            DecreaseAmmoCount(ammoType, GetAmmoCountByType(ammoType));
            return returnVal;
        }
    }

    public void SetAmmoTypeNull(Item.AmmoType ammoType)
    {
        switch (ammoType)
        {
            case Item.AmmoType.NINEMILL:
                nineMill = null;
                break;
            case Item.AmmoType.NATOFIVEFIVESIX:
                fivefivesix = null;
                break;
            case Item.AmmoType.TWELVEGAUGE:
                twelveGauge = null;
                break;
            case Item.AmmoType.FIVEMILL:
                fiveMill = null;
                break;
            case Item.AmmoType.THREEZEROEIGHT:
                threeZeroEight = null;
                break;
            case Item.AmmoType.ROCKET:
                rocket = null;
                break;
            case Item.AmmoType.ENERGYCELL:
                energyCell = null;
                break;
            case Item.AmmoType.FORTYFOURMAGNUM:
                fortyFourMagnum = null;
                break;
            case Item.AmmoType.FIFTYCALIBER:
                fiftyCaliber = null;
                break;
            case Item.AmmoType.PLASMA_CELL:
                plasmaCell = null;
                break;
            case Item.AmmoType.FLARE:
                flare = null;
                break;
            case Item.AmmoType.ARROW:
                arrow = null;
                break;
            case Item.AmmoType.CRYO_CELL:
                cryoCell = null;
                break;
            case Item.AmmoType.TWENTYTWOCALIBER:
                twentyTwoCaliber = null;
                break;
            case Item.AmmoType.FUEL_CANISTER:
                fuelCanister = null;
                break;
            case Item.AmmoType.BUZZSAW:
                sawblade = null;
                break;
        }
    }
}
