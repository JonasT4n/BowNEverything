using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public abstract class LivingEntity : MonoBehaviour
{
    [Header("Living Entity Attributes")]
    [SerializeField] private int _maxHealth = 10;

    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private int _currentHealth;
    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private bool _isPaused;

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
}
