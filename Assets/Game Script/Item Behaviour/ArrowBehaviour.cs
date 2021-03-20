using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public abstract class ArrowBehaviour : MonoBehaviour
{
    [Header("General Info")]
    [SerializeField] private float _maxShootForce = 5f;
    [SerializeField] private int _damage = 1;
    [SerializeField] private ArrowTypes _type = ArrowTypes.Normal;
    [SerializeField] private SpriteRenderer _spriteRenderer = null;
    [SerializeField] private LayerMask _ignoreHitLayer = ~0;

    protected Queue<ArrowBehaviour> _poolRef;

    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private PlayerEntity _who = null;
    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private bool _isLanded = false;

    public Queue<ArrowBehaviour> PoolReference { set => _poolRef = value; }
    
    /// <summary>
    /// Who shoot this arrow.
    /// </summary>
    public PlayerEntity Who
    {
        set => _who = value;
        get => _who;
    }

    public bool IsLanded
    {
        set => _isLanded = value;
        get => _isLanded;
    }

    public float MaxShootForce => _maxShootForce;
    public int Damage => -_damage;
    public ArrowTypes TypeOfArrow => _type;
    public SpriteRenderer Renderer => _spriteRenderer;

    protected LayerMask IgnoreHitLayer => ~_ignoreHitLayer;

    public abstract void Shoot(Vector2 origin, Vector2 shootDir);
}
