using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider Slider;

    public void SetMaxhealth(int maxHealth) 
    {
        Slider.maxValue = maxHealth;
        Slider.value = maxHealth;
    }

    public void SetCurrentHealth(int currentHealth) 
    {
        Slider.value = currentHealth;
    }
}
