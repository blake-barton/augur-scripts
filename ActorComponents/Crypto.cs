using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crypto : MonoBehaviour
{
    [SerializeField] int currency = 0;

    [Header("Events")]
    public UnityEvent currencyChangedEvent;

    public int CurrentCurrency { get => currency; }

    public void AddCurrency(int count)
    {
        currency += count;

        currencyChangedEvent.Invoke();
    }

    public void SubtractCurrency(int count)
    {
        currency -= count;

        if (currency < 0)
            currency = 0;

        currencyChangedEvent.Invoke();
    }

    public bool IsCurrencyAboveZero()
    {
        if (currency > 0)
            return true;

        return false;
    }
}
