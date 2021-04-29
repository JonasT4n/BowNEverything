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

    private IElementItemInfo _itemInfo = null;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private int _onCollectionIndex;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Sprite _sprite = null;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector3 _origin;

    public IElementItemInfo ItemInfo
    {
        set
        {
            _itemInfo = value;
            _sprite = _itemInfo.ItemSprite;
        }
        get => _itemInfo;
    }

    public int OnCollectionIndex
    {
        set => _onCollectionIndex = value;
        get => _onCollectionIndex;
    }

    #region Unity BuiltIn Methods
    private void OnEnable()
    {
        if (_sprite != null)
            _renderer.sprite = _sprite;
    }

    private void Start()
    {
        _origin = transform.position;
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
}
