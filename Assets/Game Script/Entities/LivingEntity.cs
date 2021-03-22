using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum EntityEffects
{
    None = 0,
    Slowdown = 1
}

[RequireComponent(typeof(EntityUIInformation))]
public abstract class LivingEntity : MonoBehaviour
{
    [Header("Living Entity Attributes")]
    [SerializeField] private int _maxHealth = 10;
    [SerializeField] private float _speed = 0.425f;
    [SerializeField] private EntityUIInformation _uiInfoPlaceholder = null;
    [SerializeField] private AudioSource _soundMaker = null;

    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private int _currentHealth = 0;
    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private bool _isPaused = false;
    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private Vector2 _moveDir = Vector2.zero;

    public EntityUIInformation InformationUI => _uiInfoPlaceholder;
    public AudioSource SoundMaker => _soundMaker;

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
    public float Speed
    {
        protected set => _speed = value;
        get => _speed;
    }

    public Vector2 CurrentVelocity 
    { 
        set => _moveDir = value; 
        get => _moveDir; 
    }

    public abstract void AddHealth(int h);
    public abstract void AddForce(Vector2 forceDir, ForceMode2D force);
    public abstract void AddEffects(EntityEffects effect, float value, bool temporary = true);
    public abstract void ResetEntityValues();

    protected abstract IEnumerator TemporaryEffect(EntityEffects effect, float negativeValue, float seconds);
}
