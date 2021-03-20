using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class NormalArrow : ArrowBehaviour
{
    [Header("Normal Arrow Attributes")]
    [InfoBox("This normal arrow behaviour will shoot once and hit once when landed, affected by physics.")]
    [SerializeField] private float _timerDestroy = 5f;
    [SerializeField] private BoxCollider2D _collider = null;
    [SerializeField] private Rigidbody2D _rigid2D = null;
    [SerializeField] private Animator _arrowAnim = null;

    private Vector2 _velHolder;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _hitSmh = false;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _freeze = false;

    #region Unity BuiltIn Methods
    private void OnEnable()
    {
        // Make it dynamic not kinematic
        _rigid2D.isKinematic = false;

        // Subscribe event
        EventHandler.OnGamePauseEvent += ArrowFreezeOnSinglePlayer;
        EventHandler.OnArrowHitEvent += OnHitEvent;
        EventHandler.OnPlayerShootEvent += ArrowBeingShoot;
    }

    private void Update()
    {
        // Arrow still in the air
        if (!_hitSmh && !_freeze)
        {
            _velHolder = _rigid2D.velocity;

            // Adjust realistic rotation
            float rotDegree = Mathf.Atan(_velHolder.y / _velHolder.x) * Mathf.Rad2Deg;
            ArrowQuiverElement e = ObjectManager.GetArrowElement(TypeOfArrow);
            rotDegree = rotDegree + (e == null ? 0 : e.OffsetDegreeAnticipation);
            Renderer.transform.eulerAngles = new Vector3(0, 0, rotDegree);

            // Determine sprite flip
            if (_velHolder.x < 0)
                Renderer.flipX = true;
            else
                Renderer.flipX = false;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe event
        EventHandler.OnGamePauseEvent -= ArrowFreezeOnSinglePlayer;
        EventHandler.OnArrowHitEvent -= OnHitEvent;
        EventHandler.OnPlayerShootEvent -= ArrowBeingShoot;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "MainCamera" || _hitSmh)
            return;

        if (!Who.gameObject.Equals(collision.gameObject))
        {
            _hitSmh = true;
            _rigid2D.isKinematic = true;
            _rigid2D.velocity = Vector2.zero;
            EventHandler.CallEvent(new ArrowHitEventArgs(Who, collision, this));
        }
    }
    #endregion

    #region Overriden Methods
    public override void Shoot(Vector2 origin, Vector2 shootDir)
    {
        OnShootEffect(origin, shootDir);
    }
    #endregion

    #region Event Methods
    private void ArrowFreezeOnSinglePlayer(PauseGamePressEventArgs args)
    {
        if (!_hitSmh)
        {
            if (args.IsPause && GameManager._instance.CurrentGameMode != GameModeState.MultiPlayer)
            {
                _freeze = true;
                _rigid2D.isKinematic = true;
                _rigid2D.velocity = Vector2.zero;
            }
            else if (!args.IsPause && GameManager._instance.CurrentGameMode != GameModeState.MultiPlayer)
            {
                _freeze = false;
                _rigid2D.isKinematic = false;
                _rigid2D.velocity = _velHolder;
            }
        }
    }

    private void OnHitEvent(ArrowHitEventArgs args)
    {
        if (args.ArrowHit.Equals(this))
        {
            _arrowAnim.SetBool("Is Landed", true);
            OnHitEffect(_collider.bounds.center, args.VictimHit.gameObject.GetComponent<LivingEntity>());
        }
    }

    private void ArrowBeingShoot(PlayerShootEventArgs args)
    {
        if (args.ArrowObject.gameObject.Equals(gameObject))
            _arrowAnim.SetBool("Is Landed", false);
    }
    #endregion

    protected virtual void OnShootEffect(Vector2 origin, Vector2 shootDir)
    {
        _hitSmh = false;
        transform.position = new Vector3(origin.x, origin.y, 0);
        _rigid2D.AddForce(shootDir * MaxShootForce, ForceMode2D.Impulse);
    }

    protected virtual void OnHitEffect(Vector3 pos, LivingEntity hit = null)
    {
        if (hit != null)
        {
            hit.AddHealth(Damage);
            gameObject.SetActive(false);
            _poolRef.Enqueue(this);
            return;
        }

        StartCoroutine(DestroyTimer());
    }

    protected IEnumerator DestroyTimer()
    {
        float timerHandler = _timerDestroy;

        while (timerHandler > 0)
        {
            yield return null;
            timerHandler -= Time.deltaTime;
        }

        gameObject.SetActive(false);
        _poolRef.Enqueue(this);
    }
}
