using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveReloadBackground : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] float backgroundWidth;
    [SerializeField] float backgroundHeight = 5f;
    [SerializeField] float sweetSpotPos;
    [SerializeField] float sweetSpotWidth;
    [SerializeField] float normalSpotPos;       // normal spot is always attached to sweet spot so this should be calculated after getting the width of the normal spot
    [SerializeField] float normalSpotWidth;

    [Header("Positioning")]
    public bool isMainHand = false;
    [SerializeField] Vector2 mainHandOffset = new Vector2();
    [SerializeField] Vector2 offHandOffset = new Vector2();

    [Header("Bars")]
    [SerializeField] RectTransform handleSlideArea;
    [SerializeField] RectTransform background;
    [SerializeField] RectTransform sweetSpot;
    [SerializeField] RectTransform normalSpot;

    // cached references
    [SerializeField] Player player;
    [SerializeField] RectTransform sliderTransform;
    [SerializeField] Slider slider;
    public WeaponReloader weaponReloader;

    // state
    [SerializeField] bool reloadComplete = false;

    // references
    AttributeScores playerAttributes;

    // calculated
    float sweetSpotStartTime;
    float sweetSpotEndTime;
    float normalSpotEndTime;

    private void Awake()
    {
        sliderTransform = GetComponent<RectTransform>();
        player = FindObjectOfType<Player>();
        playerAttributes = player.GetComponent<AttributeScores>();
        slider = GetComponent<Slider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        FormatBar();
    }

    // Update is called once per frame
    void Update()
    {
        LockToPlayer();

        if (!reloadComplete)
        {
            // immediately start moving the handle
            slider.value = Time.time;
        }
    }

    // handles the size of the background and slider
    public void FormatBar()
    {
        // size background
        background.sizeDelta = new Vector2(backgroundWidth, backgroundHeight);

        // size slider
        sliderTransform.sizeDelta = new Vector2(backgroundWidth, backgroundHeight * 2);
    }

    void FormatReloadSpots()
    {
        // size sweet spot
        sweetSpot.sizeDelta = new Vector2(sweetSpotWidth, backgroundHeight);

        // size normal spot
        normalSpot.sizeDelta = new Vector2(normalSpotWidth, backgroundHeight);

        // clamp sweetSpotPos to the background width
        sweetSpotPos = Mathf.Clamp(sweetSpotPos, (-1f * backgroundWidth / 2) + sweetSpotWidth / 2, backgroundWidth / 2 - sweetSpotWidth / 2);

        // set sweet spot pos
        sweetSpot.anchoredPosition = new Vector3(sweetSpotPos, 0, 0);

        // attach normal spot to sweet spot
        normalSpotPos = sweetSpotPos + sweetSpotWidth / 2 + normalSpotWidth / 2;

        // position normal spot
        normalSpot.anchoredPosition = new Vector3(normalSpotPos, 0, 0);
    }

    private void LockToPlayer()
    {
        // lock position to player if player exists
        if (isMainHand)
        {
            if (player)
            {
                transform.position = Camera.main.WorldToScreenPoint((Vector2)player.transform.position + mainHandOffset);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (player)
            {
                transform.position = Camera.main.WorldToScreenPoint((Vector2)player.transform.position + offHandOffset);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void SetParameters(bool isMainHand, float reloadTime, float sweetSpotStartPercentage, float sweetSpotEndPercentage, float normalSpotEndPercentage, WeaponReloader owner)
    {
        // add bonus percentage from playerAttributes
        sweetSpotEndPercentage += playerAttributes.sweetSpotBonusPercentage;

        // set owner
        weaponReloader = owner;

        // determines placement of slider
        this.isMainHand = isMainHand;

        // set speed of handle based on reload speed
        slider.minValue = Time.time;
        slider.maxValue = Time.time + reloadTime;

        // set the reload bar's spot positions and size
        SetSpotParameters(sweetSpotStartPercentage, sweetSpotEndPercentage, normalSpotEndPercentage);
        FormatReloadSpots();
    }

    private void SetSpotParameters(float sweetSpotStartPercentage, float sweetSpotEndPercentage, float normalSpotEndPercentage)
    {
        // basically here we're using the sweet spot start and end percentages to get the start and end values for the sweet spot in terms of actual seconds
        sweetSpotStartTime = Mathf.Lerp(slider.minValue, slider.maxValue, sweetSpotStartPercentage);
        sweetSpotEndTime = Mathf.Lerp(slider.minValue, slider.maxValue, sweetSpotEndPercentage);
        normalSpotEndTime = Mathf.Lerp(slider.minValue, slider.maxValue, normalSpotEndPercentage);

        // Now we're using the percentages to get the position of the sweet spot on the bar
        float sweetMidpointTimeAsPercentage = (sweetSpotStartPercentage + sweetSpotEndPercentage) / 2f;
        sweetSpotPos = sweetMidpointTimeAsPercentage * backgroundWidth - backgroundWidth / 2;

        // this gives the width of the sweet spot in relation to the background, adding in the player's sleight of hand bonus
        float sweetSpotPercentOfWidth = sweetSpotEndPercentage - sweetSpotStartPercentage;
        sweetSpotWidth = sweetSpotPercentOfWidth * backgroundWidth;

        // now calculate the width of the normal spot using the sweet spot end percentage and the normal spot end percentage (since they're attached)
        float normalSpotPercentOfWidth = normalSpotEndPercentage - sweetSpotEndPercentage;
        normalSpotWidth = normalSpotPercentOfWidth * backgroundWidth;
    }

    public WeaponReloader.ActiveReloadState FinishReload()
    {
        // freeze slider
        reloadComplete = true;
        slider.value = slider.value;    // this might not be necessary?

        // check if slider is in zones
        if (slider.value >= sweetSpotStartTime && slider.value < sweetSpotEndTime)
        {
            return WeaponReloader.ActiveReloadState.SWEETSPOT;
        }
        else if (slider.value >= sweetSpotEndTime && slider.value <= normalSpotEndTime)
        {
            // send normal
            return WeaponReloader.ActiveReloadState.NORMALSPOT;
        }
        else
        {
            // send jam
            return WeaponReloader.ActiveReloadState.JAMMED;
        }
    }
}
