using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class ObjectFollower : MonoBehaviour
{
    [Header("Follower Attributes")]
    [SerializeField] private Transform _target = null;
    [SerializeField] [Slider(-1, 1)] protected float _followByX = 1;
    [SerializeField] [Slider(-1, 1)] protected float _followByY = 1;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector3 _targetLastPos = Vector3.zero;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector3 _origin = Vector3.zero;

    protected Vector3 PositionOrigin => _origin;

    public Transform TargetFollow
    {
        set
        {
            if (value != null)
            {
                _target = value;
                _targetLastPos = _target.transform.position;
            }
        }
        get => _target;
    }

    #region Unity BuiltIn Methods
    // Start is called before the first frame update
    protected virtual void Start()
    {
        _origin = transform.position;

        if (_target != null)
            _targetLastPos = _target.transform.position;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        Vector3 _currentTargetPos = _target.transform.position;
        if (_currentTargetPos.x != _targetLastPos.x || _currentTargetPos.y != _targetLastPos.y)
        {
            float moveX = (_currentTargetPos.x - _targetLastPos.x) * _followByX;
            float moveY = (_currentTargetPos.y - _targetLastPos.y) * _followByY;

            _origin += new Vector3(moveX, moveY);
            transform.position = _origin;
            _targetLastPos = _currentTargetPos;
        }
    }
    #endregion
}
