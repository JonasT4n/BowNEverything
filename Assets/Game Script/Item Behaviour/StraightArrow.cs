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
    [SerializeField] private Animator _arrowAnim = null;

    private Vector2 _velHolder;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector2 _moveDir = Vector2.zero;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _hitSmh = false;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _freeze = false;

    #region Unity BuiltIn Methods
    private void OnEnable()
    {
        // Subscribe event
        EventHandler.OnGamePauseEvent += ArrowFreezeOnSinglePlayer;
        EventHandler.OnArrowHitEvent += OnHitEvent;
        EventHandler.OnPlayerShootEvent += ArrowBeingShoot;
    }

    // Update is called once per frame
    private void Update()
    {
        // Arrow still in the air
        if (!_hitSmh && !_freeze)
        {
            HandleStraightMovement();

            if (_simulateRotation)
                HandleOnAirRotation(_velHolder);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe event
        EventHandler.OnGamePauseEvent -= ArrowFreezeOnSinglePlayer;
        EventHandler.OnArrowHitEvent -= OnHitEvent;
        EventHandler.OnPlayerShootEvent -= ArrowBeingShoot;
    }
    #endregion

    #region Event Methods
    private void ArrowFreezeOnSinglePlayer(PauseGamePressEventArgs args)
    {
        if (!_hitSmh && GameManager._instance.CurrentGameMode != GameModeState.MultiPlayer)
        {
            _freeze = args.IsPause;
            _moveDir = args.IsPause ? Vector2.zero : _velHolder;
        }
    }

    private void OnHitEvent(ArrowHitEventArgs args)
    {
        if (args.ArrowHit.Equals(this))
        {
            //_arrowAnim.SetBool("Is Landed", true);
            OnHitEffect(_collider.bounds.center, args.VictimHit.gameObject.GetComponent<LivingEntity>());
        }
    }

    private void ArrowBeingShoot(PlayerShootEventArgs args)
    {
        //if (args.ArrowObject.gameObject.Equals(gameObject))
        //    _arrowAnim.SetBool("Is Landed", false);
    }
    #endregion

    #region Overriden Methods
    public override void Shoot(Vector2 origin, Vector2 shootDir)
    {
        OnShootEffect(origin, shootDir);
    }
    #endregion

    protected virtual void HandleStraightMovement()
    {
        _velHolder = _moveDir;
        transform.Translate(_moveDir.normalized * MaxShootForce * Time.deltaTime);

        Collider2D hit = DetectHit();
        if (hit != null)
        {
            if (hit.gameObject.tag == "MainCamera" || _hitSmh)
                return;

            if (hit.gameObject.layer == LayerMask.NameToLayer("VOID"))
            {
                gameObject.SetActive(false);
                _poolRef.Enqueue(this);
            }

            if (!WhoShot.gameObject.Equals(hit.gameObject))
            {
                if (hit.GetComponent<EnemyEntity>())
                    return;

                ArrowHitEventArgs arg = new ArrowHitEventArgs(WhoShot, hit, this);
                EventHandler.CallEvent(arg);

                if (!arg.IsCancelled)
                {
                    _hitSmh = true;
                    _moveDir = Vector2.zero;
                }
            }
        }
    }

    protected virtual void HandleOnAirRotation(Vector2 velocity)
    {
        // Adjust realistic rotation
        float rotDegree = Mathf.Atan(_velHolder.y / _velHolder.x) * Mathf.Rad2Deg;
        ArrowQuiverElement e = ObjectManager._instance.GetArrowElement(TypeOfArrow);

        // Determine sprite flip
        if (_velHolder.x < 0)
        {
            Renderer.flipX = true;
            rotDegree = rotDegree + (e == null ? 0 : -e.OffsetDegreeAnticipation);
        }
        else
        {
            Renderer.flipX = false;
            rotDegree = rotDegree + (e == null ? 0 : e.OffsetDegreeAnticipation);
        }

        // Apply Rotation
        Renderer.transform.eulerAngles = new Vector3(0, 0, rotDegree);
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

    protected virtual void OnShootEffect(Vector2 origin, Vector2 shootDir)
    {
        _hitSmh = false;
        _moveDir = shootDir;
        transform.position = new Vector3(origin.x, origin.y, 0);
    }

    private Collider2D DetectHit()
    {
        RaycastHit2D hit = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.zero, 0, IgnoreHitLayer);
        return hit.collider;
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
