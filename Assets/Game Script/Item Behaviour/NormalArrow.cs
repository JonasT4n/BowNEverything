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

    protected Rigidbody2D ArrowRigid => _rigid2D;
    protected Animator ArrowAnimator => _arrowAnim;
    protected bool IsAlreadyHit
    {
        set => _hitSmh = value;
        get => _hitSmh;
    }

    #region Unity BuiltIn Methods
    private void OnEnable()
    {
        // Make it dynamic not kinematic
        _rigid2D.isKinematic = false;

        // Subscribe event
        EventHandler.OnGamePauseEvent += ArrowFreezeOnSinglePlayer;
        EventHandler.OnPlayerShootEvent += ArrowBeingShoot;
    }

    private void FixedUpdate()
    {
        // Arrow still in the air
        if (!_hitSmh && !_freeze)
        {
            _velHolder = _rigid2D.velocity;
            if (_simulateRotation)
                HandleOnAirRotation(_velHolder);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe event
        EventHandler.OnGamePauseEvent -= ArrowFreezeOnSinglePlayer;
        EventHandler.OnPlayerShootEvent -= ArrowBeingShoot;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "MainCamera" || _hitSmh)
            return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("VOID"))
        {
            gameObject.SetActive(false);
            _poolRef.Enqueue(this);
        }

        if (!WhoShot.gameObject.Equals(collision.gameObject))
            OnHitEffect(collision);
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
            _freeze = args.IsPause;
            _rigid2D.isKinematic = args.IsPause;
            _rigid2D.velocity = args.IsPause ? Vector2.zero : _velHolder;
        }
    }

    private void ArrowBeingShoot(PlayerShootEventArgs args)
    {
        if (args.ArrowObject.gameObject.Equals(gameObject))
            _arrowAnim.SetBool("Is Landed", false);
    }
    #endregion

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

    protected virtual void OnShootEffect(Vector2 origin, Vector2 shootDir)
    {
        _hitSmh = false;
        transform.position = new Vector3(origin.x, origin.y, 0);
        _rigid2D.AddForce(shootDir * MaxShootForce, ForceMode2D.Impulse);
    }

    protected virtual void OnHitEffect(Collider2D collision)
    {
        ArrowHitEventArgs arg = new ArrowHitEventArgs(WhoShot, collision, this);
        EventHandler.CallEvent(arg);

        if (!arg.IsCancelled)
        {
            _hitSmh = true;
            _rigid2D.isKinematic = true;
            _rigid2D.velocity = Vector2.zero;
            _arrowAnim.SetBool("Is Landed", true);

            // Check entity hit
            LivingEntity hit = arg.VictimHit.gameObject.GetComponent<LivingEntity>();
            if (hit != null)
            {
                hit.AddHealth(Damage);

                // Knockback
                if (hit.CurrentHealth > 0)
                {
                    Vector3 hitDir = (hit.transform.position - _collider.bounds.center).normalized;
                    hit.EntityR2D.AddForce(new Vector2(hitDir.x < 0 ? -20 : 20, Mathf.Abs(hitDir.y) * 2), ForceMode2D.Impulse);
                }

                // Check entity killed
                else if (hit.CurrentHealth <= 0)
                {
                    if (WhoShot is PlayerEntity)
                        ((PlayerEntity)WhoShot).EnemyKillCount++;

                    EntityDeathEventArgs deathArg = new EntityDeathEventArgs(hit, WhoShot);
                    EventHandler.CallEvent(deathArg);

                    if (!deathArg.IsCancelled)
                        gameObject.SetActive(false);
                }

                gameObject.SetActive(false);
                _poolRef.Enqueue(this);
                return;
            }
        }

        if (gameObject.activeSelf)
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
