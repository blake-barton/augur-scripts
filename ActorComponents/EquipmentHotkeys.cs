using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentHotkeys : MonoBehaviour
{
    [SerializeField] Equipment equipment;
    [SerializeField] Player player;
    [SerializeField] PlayerMovement playerMovement;

    Equippable slotEquippable;

    // cached references
    TabMenuManager tabMenuManager;

    private void Awake()
    {
        tabMenuManager = FindObjectOfType<TabMenuManager>();
    }

    private void Update()
    {
        if (!tabMenuManager.InMenu && !player.InDialogue && !playerMovement.Dodging && !player.Character.Incapacitated)
        {
            // holding middle mouse button enables equipping offhand
            if (Input.GetMouseButton(2))
            {
                InputOffHand();
            }
            else
            {
                InputMainHand();
            }
        }
    }

    private void InputMainHand()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            slotEquippable = equipment.QuickEquippables[0];
            EquipMain(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            slotEquippable = equipment.QuickEquippables[1];
            EquipMain(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            slotEquippable = equipment.QuickEquippables[2];
            EquipMain(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            slotEquippable = equipment.QuickEquippables[3];
            EquipMain(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            slotEquippable = equipment.QuickEquippables[4];
            EquipMain(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            slotEquippable = equipment.QuickEquippables[5];
            EquipMain(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
        {
            slotEquippable = equipment.QuickEquippables[6];
            EquipMain(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
        {
            slotEquippable = equipment.QuickEquippables[7];
            EquipMain(slotEquippable);
        }
    }

    private void InputOffHand()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            slotEquippable = equipment.QuickEquippables[0];
            EquipOff(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            slotEquippable = equipment.QuickEquippables[1];
            EquipOff(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            slotEquippable = equipment.QuickEquippables[2];
            EquipOff(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            slotEquippable = equipment.QuickEquippables[3];
            EquipOff(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            slotEquippable = equipment.QuickEquippables[4];
            EquipOff(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            slotEquippable = equipment.QuickEquippables[5];
            EquipOff(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
        {
            slotEquippable = equipment.QuickEquippables[6];
            EquipOff(slotEquippable);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8))
        {
            slotEquippable = equipment.QuickEquippables[7];
            EquipOff(slotEquippable);
        }
    }

    private void EquipMain(Equippable equippable)
    {
        // weapons
        if (equippable.equippableType == Equippable.EquippableType.ITEM)
        {
            if (equippable.item.Type == Item.ItemType.RANGEDWEAPON || equippable.item.Type == Item.ItemType.MELEEWEAPON)
            {
                equipment.EquipMainHandWeapon(equippable);
            }
        }
        // spells
        else if (equippable.equippableType == Equippable.EquippableType.SPELL)
        {
            equipment.EquipMainHandSpell(equippable.spell);
        }
    }

    private void EquipOff(Equippable equippable)
    {
        // weapons
        if (equippable.equippableType == Equippable.EquippableType.ITEM)
        {
            if (equippable.item.Type == Item.ItemType.RANGEDWEAPON || equippable.item.Type == Item.ItemType.MELEEWEAPON)
            {
                equipment.EquipOffHandWeapon(equippable);
            }
        }
        // spells
        else if (equippable.equippableType == Equippable.EquippableType.SPELL)
        {
            equipment.EquipOffHandSpell(equippable.spell);
        }
    }
}
