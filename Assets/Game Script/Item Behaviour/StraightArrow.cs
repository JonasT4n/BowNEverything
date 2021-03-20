using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class StraightArrow : ArrowBehaviour
{
    [Header("Straight Arrow Attributes")]
    [InfoBox("This straight arrow behaviour will shoot once and hit once when landed, not affected by physics, it will just go straight.")]
    [SerializeField] private float _timerDestroy = 5f;
    [SerializeField] private BoxCollider2D _collider = null;

    #region Unity BuiltIn Methods
    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
    #endregion

    public override void Shoot(Vector2 origin, Vector2 shootDir)
    {
        throw new System.NotImplementedException();
    }
}
