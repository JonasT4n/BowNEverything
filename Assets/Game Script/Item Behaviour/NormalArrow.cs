using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class NormalArrow : ArrowBehaviour
{
    [Header("Normal Arrow")]
    [InfoBox("This normal arrow behaviour will shoot once and hit once when landed.")]
    [SerializeField] private float _timerDestroy = 5f;
    [SerializeField] private BoxCollider2D _collider = null;
    [SerializeField] private Rigidbody2D _rigid2D = null;

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
    }

    private void Update()
    {
        // Arrow still in the air
        if (!_hitSmh)
        {
            if (!_freeze)
            {
                _velHolder = _rigid2D.velocity;

                // Adjust realistic rotation
                float rotDegree = Mathf.Atan(_velHolder.y / _velHolder.x) * Mathf.Rad2Deg;
                Renderer.transform.eulerAngles = new Vector3(0, 0, rotDegree);

                // Determine sprite flip
                if (_velHolder.x < 0)
                    Renderer.flipX = true;
                else
                    Renderer.flipX = false;
            }
        }
    }

    private void OnDisable()
    {
        // Unsubscribe event
        EventHandler.OnGamePauseEvent -= ArrowFreezeOnSinglePlayer;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Who.gameObject.Equals(collision.gameObject))
        {
            _hitSmh = true;
            _rigid2D.isKinematic = true;
            _rigid2D.velocity = Vector2.zero;
            StartCoroutine(DestroyTimer());
        }
    }
    #endregion

    #region Overriden Methods
    public override void Shoot(Vector2 origin, Vector2 shootDir)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        _hitSmh = false;
        transform.position = new Vector3(origin.x, origin.y, 0);
        _rigid2D.AddForce(shootDir * MaxShootForce, ForceMode2D.Impulse);
    }
    #endregion

    #region Event Methods
    private void ArrowFreezeOnSinglePlayer(PauseGamePressEventArgs args)
    {
        if (args.IsPause && GameManager.CurrentGameMode != GameMode.MultiPlayer)
        {
            _freeze = true;
            _rigid2D.isKinematic = true;
            _rigid2D.velocity = Vector2.zero;
        }
        else if (!args.IsPause && GameManager.CurrentGameMode != GameMode.MultiPlayer)
        {
            _freeze = false;
            _rigid2D.isKinematic = false;
            _rigid2D.velocity = _velHolder;
        }
    }
    #endregion

    private IEnumerator DestroyTimer()
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
