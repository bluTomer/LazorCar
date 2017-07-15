using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider DistanceBar;
    public Text Speedometer;
    public Slider FuelMeter;

    public int MinSpeed;
    public int MaxSpeed;

    public void UpdateDistance(float normalizedValue)
    {
        normalizedValue = Mathf.Clamp01(normalizedValue);
        DistanceBar.value = normalizedValue;
    }

    public void UpdateSpeed(float normalizedValue)
    {
        int speed = Mathf.FloorToInt(Mathf.Lerp(MinSpeed, MaxSpeed, normalizedValue));
        Speedometer.text = speed.ToString();
    }

    public void UpdateFuel(float normalizedValue)
    {
        normalizedValue = Mathf.Clamp01(normalizedValue);
        FuelMeter.value = normalizedValue;
    }
}
