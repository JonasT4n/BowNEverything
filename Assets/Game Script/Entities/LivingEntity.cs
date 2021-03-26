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
    protected const float DETECT_RANGE = 0.01f;

    [Header("Living Entity Attributes")]
    [SerializeField] private int _maxHealth = 10;
    [SerializeField] private float _speed = 0.425f;
    [SerializeField] private float _jumpForce = 0.75f;
    [SerializeField] protected LayerMask _groundMask = ~0;
    [SerializeField] protected LayerMask _wallMask = ~0;
    [SerializeField] private Rigidbody2D _playerRigidbody = null;
    [SerializeField] private EntityUIInformation _uiInfoPlaceholder = null;
    [SerializeField] private AudioSource _soundMaker = null;

    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private int _currentHealth = 0;
    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private bool _isPaused = false;

    public Rigidbody2D EntityR2D
    {
        protected set => _playerRigidbody = value;
        get => _playerRigidbody;
    }
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

    public float JumpForce
    {
        set => _jumpForce = value;
        get => _jumpForce;
    }

    public abstract void AddHealth(int h);
    public abstract void AddEffects(EntityEffects effect, float value, bool temporary = true);
    public abstract void ResetEntityValues();

    protected abstract IEnumerator TemporaryEffect(EntityEffects effect, float negativeValue, float seconds);
}
