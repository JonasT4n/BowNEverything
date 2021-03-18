using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public abstract class ArrowBehaviour : MonoBehaviour
{
    [SerializeField] private float _maxShootForce = 5f;
    [SerializeField] private ArrowTypes _type = ArrowTypes.Normal;
    [SerializeField] private SpriteRenderer _spriteRenderer = null;
    [SerializeField] private LayerMask _ignoreHitLayer = ~0;

    protected Queue<ArrowBehaviour> _poolRef;

    [BoxGroup("ABSTRACT DEBUG"), SerializeField, ReadOnly] private PlayerEntity _who = null;

    public Queue<ArrowBehaviour> PoolReference { set => _poolRef = value; }
    public PlayerEntity Who
    {
        set => _who = value;
        get => _who;
    }

    public float MaxShootForce => _maxShootForce;
    public ArrowTypes TypeOfArrow => _type;
    public SpriteRenderer Renderer => _spriteRenderer;

    protected LayerMask IgnoreHitLayer => ~_ignoreHitLayer;

    public abstract void Shoot(Vector2 origin, Vector2 shootDir);
}
