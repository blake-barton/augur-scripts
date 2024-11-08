using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public List<int> quantities = new List<int>();
    public List<Pair<Item, int>> itemPairs = new List<Pair<Item, int>>();

    private void Awake()
    {
        PopulatePairList();
    }

    private void PopulatePairList()
    {
        for (int i = 0; i < items.Count; i++)
        {
            itemPairs.Add(new Pair<Item, int>(Instantiate(items[i]), quantities[i]));
        }
    }
}
