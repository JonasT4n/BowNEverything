using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(EntityUIInformation))]
public abstract class LivingEntity : MonoBehaviour
{
    [Header("Living Entity Attributes")]
    [SerializeField] private int _maxHealth = 10;
    [SerializeField] private EntityUIInformation _uiInfoPlaceholder = null;

    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private int _currentHealth;
    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private bool _isPaused;

    protected EntityUIInformation InformationUI => _uiInfoPlaceholder;

    public int MaxHealth => _maxHealth;
    public int CurrentHealth
    {
        set => _currentHealth = value;
        get => _currentHealth;
    }
    public bool IsPaused
    {
        set => _isPaused = value;
        get => _isPaused;
    }

    public abstract void AddHealth(int h);
    public abstract void ResetEntityValues();
}
