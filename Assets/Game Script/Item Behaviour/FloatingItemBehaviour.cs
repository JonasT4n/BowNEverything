using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class FloatingItemBehaviour : MonoBehaviour
{
    #region Deprecated
    [System.Obsolete("Handle animation programatically is deprecated.")]
    private float _tension = 0.5f;

    [System.Obsolete("Handle animation programatically is deprecated.")]
    private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [System.Obsolete("Handle animation programatically is deprecated.")]
    private float _currentTimeHandler;

    [System.Obsolete("Handle animation programatically is deprecated.")]
    private void HandleAnimation()
    {
        float maxAnimTime = _curve.keys[_curve.keys.Length - 1].time;
        if (_currentTimeHandler < maxAnimTime)
        {
            _currentTimeHandler += Time.deltaTime;
            if (_currentTimeHandler > maxAnimTime)
                _currentTimeHandler = maxAnimTime;

            float v = _curve.Evaluate(_currentTimeHandler) * _tension;
            transform.position = new Vector3(_origin.x, _origin.y + v, _origin.z);
        }
        else
        {
            _currentTimeHandler = 0f;
        }
    }
    #endregion

    [Header("Floating Attributes")]
    [SerializeField] private SpriteRenderer _renderer = null;
    [SerializeField] private BoxCollider2D _collider = null;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private LayerMask _collectAbleBy = ~0;

    private IElementInfo _itemInfo = null;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Sprite _sprite = null;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector3 _origin;

    public Sprite RenderSprite
    {
        set => _sprite = value;
        get => _sprite;
    }

    public IElementInfo ItemInfo
    {
        set => _itemInfo = value;
        get => _itemInfo;
    }

    #region Unity BuiltIn Methods
    private void OnEnable()
    {
        //_currentTimeHandler = 0f;
        _origin = transform.position;

        if (_sprite != null)
            _renderer.sprite = _sprite;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // Handler method called
        HandleCollision();
    }

    private void OnDisable()
    {
        if (_itemInfo != null)
        {
            _itemInfo.Dispose();
            _itemInfo = null;
        }
    }
    #endregion

    private void HandleCollision()
    {
        Vector2 origin = _collider.bounds.center;
        Vector2 size = _collider.size;
        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0, Vector2.zero, 0, _collectAbleBy);

        if (hit.collider != null)
        {
            GameObject obj = hit.collider.gameObject;
            if (obj.GetComponent<PlayerEntity>())
            {
                PlayerCollectItemEventArgs arg = new PlayerCollectItemEventArgs(obj.GetComponent<PlayerEntity>(), this);
                EventHandler.CallEvent(arg);
            }
        }
    }
}
