using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    // basis for the inventory
    [Header("Item List")]
    [SerializeField] List<Pair<Item, int>> items = new List<Pair<Item, int>>();

    // cached reference
    [SerializeField] Player player;
    [SerializeField] Equipment equipment;
    [SerializeField] AmmoCounter ammoCounter;
    [SerializeField] AttributeScores attributeScores;
    [SerializeField] QuickAidManager quickAidManager;

    public UnityEvent addedItemEvent;

    public List<Pair<Item, int>> Items { get => items; set => items = value; }

    public Pair<Item, int> AddItem(Item item, int quantity = 1, bool enableWeaponEquip = true)
    {
        Pair<Item, int> returnPair = null;

        // add the item if it isn't in the inventory
        bool itemExists = false;

        // search list to see if item already exists
        foreach (Pair<Item, int> pair in Items)
        {
            if (item.ItemID == pair.First.ItemID)
            {
                // If this addition does not exceed max or copies aren't allowed, add to existing
                if (pair.Second + quantity <= pair.First.HeldMax || !pair.First.CopiesAllowed)
                {
                    itemExists = true;

                    // increase existing item quantity
                    pair.Second += quantity;

                    // track return pair
                    returnPair = pair;
                }
            }
        }

        // make a new item, quantity pair
        if (!itemExists)
        {
            Pair<Item, int> newPair = new Pair<Item, int>(item, quantity);

            items.Add(newPair);

            // If item is ammo, assign ammo type in ammo counter
            if (item is Ammo)
            {
                ammoCounter.AssignAmmoType(newPair);
            }

            // If item is aid, automatically assign it to a quick aid slot
            if (item.Type == Item.ItemType.AID)
            {
                quickAidManager.AddItemToFirstOpenSlot(newPair);
            }

            returnPair = newPair;
        }

        // if main hand is empty, equip the item
        if (item is Weapon && equipment.MainHand == null && !player.InDialogue && enableWeaponEquip && !itemExists)
        {
            Equippable newWeapon = new Equippable(item);
            equipment.EquipMainHandWeapon(newWeapon);
        }
        else if (item is Weapon && equipment.MainHand && equipment.OffHand == null && !player.InDialogue && enableWeaponEquip && !itemExists)
        {
            // mainhand equipped but offhand empty -- equip in offhand
            Equippable newWeapon = new Equippable(item);
            equipment.EquipOffHandWeapon(newWeapon);
        }

        SortListByNameAscending();

        addedItemEvent.Invoke();

        return returnPair;
    }

    public bool DropItem(int index, int quantity)
    {
        bool stillInInventory = true;

        // store item to be spawned in the scene
        Item droppedItem = items[index].First;

        // unequip if equipped
        if (droppedItem is Weapon weapon)
        {
            if (weapon.EquippedMain)
            {
                equipment.UnequipMainHandWeapon(index);
            }
            else if (weapon.EquippedOff)
            {
                equipment.UnequipOffHandWeapon(index);
            }
        }

        if (droppedItem is Outfit)
        {
            if (droppedItem.Equipped)
            {
                equipment.UnequipOutfit();
            }
        }

        // decrement ammo if ammo
        if (droppedItem is Ammo ammo)
        {
            stillInInventory = ammoCounter.DecreaseAmmoCount(ammo.AmmunitionType, quantity);
        }
        else
        {
            // decrease the quantity of the item
            items[index].Second -= quantity;

            // update quick item hud
            quickAidManager.UpdateHUDToActiveKey();

            // remove the item from the list if its quantity drops below 0
            if (items[index].Second <= 0)
            {
                // If quick aid equipped, remove
                quickAidManager.ClearItem(items[index].First.QuickAidIndex);

                // If quick equipped, remove
                equipment.ClearQuickWeaponSlot(items[index].First.QuickEquippedIndex);

                items.RemoveAt(index);
                stillInInventory = false;
            }
        }

        // drop this item's scene item
        GameObject droppedSceneItem = Instantiate(droppedItem.sceneItem, transform.position, transform.rotation);

        // update the item's data
        SceneItem newSceneItem = droppedSceneItem.GetComponent<SceneItem>();
        newSceneItem.GetComponent<SceneItem>().ItemData = droppedItem;
        newSceneItem.quantity = quantity;
        newSceneItem.UseVortexPickup = false;

        return stillInInventory;
    }

    // does what DropItem does except doesn't spawn a scene item
    public bool RemoveItem(int index)
    {
        bool stillInInventory = true;

        Item droppedItem = items[index].First;

        // unequip if equipped
        if (droppedItem is Weapon weapon)
        {
            if (weapon.EquippedMain)
            {
                equipment.UnequipMainHandWeapon(index);
            }
            else if (weapon.EquippedOff)
            {
                equipment.UnequipOffHandWeapon(index);
            }
        }

        // decrement ammo if ammo
        if (droppedItem is Ammo ammo)
        {
            ammoCounter.DecreaseAmmoCount(ammo.AmmunitionType, 1);
        }

        // decrease the quantity of the item
        items[index].Second--;

        // update quick item hud
        quickAidManager.UpdateHUDToActiveKey();

        // remove the item from the list if its quantity drops below 0
        if (items[index].Second <= 0)
        {
            // If quick aid equipped, remove
            quickAidManager.ClearItem(items[index].First.QuickAidIndex);

            // If quick equipped, remove
            equipment.ClearQuickWeaponSlot(items[index].First.QuickEquippedIndex);

            items.RemoveAt(index);
            stillInInventory = false;
        }

        return stillInInventory;
    }

    public bool RemoveItem(Pair<Item, int> itemPair, int quantity = 1, bool clearQuickAid = true)
    {
        bool stillInInventory = true;

        // decrement ammo if ammo
        if (itemPair.First is Ammo ammo)
        {
            ammoCounter.DecreaseAmmoCount(ammo.AmmunitionType, quantity);
        }
        else
        {
            // decrease the quantity of the item
            itemPair.Second -= quantity;

            // update quick item hud
            quickAidManager.UpdateHUDToActiveKey();

            // remove the item from the list if its quantity drops below 0
            if (itemPair.Second <= 0)
            {
                // unequip if equipped
                if (itemPair.First is Weapon weapon)
                {
                    if (weapon.EquippedMain)
                    {
                        Equippable newWeapon = new Equippable(itemPair.First);
                        equipment.UnequipMainHandWeapon(newWeapon);
                    }
                    else if (weapon.EquippedOff)
                    {
                        Equippable newWeapon = new Equippable(itemPair.First);
                        equipment.UnequipOffHandWeapon(newWeapon);
                    }
                }

                // If quick aid equipped, remove
                if (clearQuickAid)
                {
                    quickAidManager.ClearItem(itemPair.First.QuickAidIndex);
                }

                // If quick equipped, remove
                equipment.ClearQuickWeaponSlot(itemPair.First.QuickEquippedIndex);

                items.Remove(itemPair);
                stillInInventory = false;
            }
        }

        return stillInInventory;
    }

    public bool RemoveAmmo(Pair<Item, int> pair, int quantity = 1)
    {
        bool stillInInventory = true;

        // decrease the quantity of the item
        pair.Second -= quantity;

        // remove the item from the list if its quantity drops below 0
        if (pair.Second <= 0)
        {
            items.Remove(pair);
            stillInInventory = false;

            // set null in ammo counter
            Ammo ammo = (Ammo)pair.First;
            ammoCounter.SetAmmoTypeNull(ammo.AmmunitionType);
        }

        return stillInInventory;
    }

    private void SortListByNameAscending()
    {
        items.Sort((x, y) => x.First.ItemName.CompareTo(y.First.ItemName));
    }

    public int GetIndexOfMainHandWeapon()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].First is Weapon weapon)
            {
                if (weapon.EquippedMain)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public int GetIndexOfOffHandWeapon()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].First is Weapon weapon)
            {
                if (weapon.EquippedOff)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public int GetIndexOfEquippedOutfit()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].First is Outfit outfit)
            {
                if (outfit.Equipped)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public int GetIndexOfEquippedFocusImplant()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].First is Misc implant)
            {
                if (implant.Type == Item.ItemType.FOCUSIMPLANT && implant.Equipped)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public int GetItemCount(int itemID)
    {
        foreach (Pair<Item, int> itemPair in items)
        {
            if (itemPair.First.ItemID == itemID)
            {
                return itemPair.Second;
            }
        }

        // item does not exist
        return 0;
    }

    public int GetItemCountAtIndex(int index)
    {
        return items[index].Second;
    }

    public Item GetItemEquippedMain()
    {
        foreach (Pair<Item, int> pair in items)
        {
            if (pair.First.EquippedMain)
            {
                return pair.First;
            }
        }

        // return null if no item in main hand
        return null;
    }

    public Item GetItemEquippedOff()
    {
        foreach (Pair<Item, int> pair in items)
        {
            if (pair.First.EquippedOff)
            {
                return pair.First;
            }
        }

        // return null if no item in off hand
        return null;
    }

    public Pair<Item, int> GetPairOfItemEquippedMain(Item item)
    {
        foreach (Pair<Item, int> pair in items)
        {
            if (pair.First == item && pair.First.EquippedMain)
            {
                return pair;
            }
        }

        // return null if no pair
        return null;
    }

    public Pair<Item, int> GetPairOfItemEquippedOff(Item item)
    {
        foreach (Pair<Item, int> pair in items)
        {
            if (pair.First == item && pair.First.EquippedOff)
            {
                return pair;
            }
        }

        // return null if no pair
        return null;
    }

    // excludes quantity from list
    public List<Item> GetInventoryAsListOfItems()
    {
        List<Item> itemList = new List<Item>();

        foreach (Pair<Item, int> pair in items)
        {
            if (!itemList.Contains(pair.First))
            {
                itemList.Add(pair.First);
            }
        }

        return itemList;
    }

    public int GetNumberOfStacks(Item item)
    {
        int stacks = 0;

        foreach (Pair<Item, int> pair in items)
        {
            if (pair.First.ItemID == item.ItemID)
            {
                stacks++;
            }
        }

        return stacks;
    }
}
