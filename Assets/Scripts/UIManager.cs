using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider HealthBar;

    public void UpdatePlayerHealth(float normalizedValue)
    {
        normalizedValue = Mathf.Clamp01(normalizedValue);
        HealthBar.value = normalizedValue;
    }
}
