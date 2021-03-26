using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(RectTransform))]
public class UIObjectFollower : ObjectFollower
{
    [InfoBox("Attach this script to UI element (Object which has RectTransform not Transform)")]
    [SerializeField] private Camera _camTarget = null;
    [SerializeField] private Vector2 _globatOffset = Vector2.zero;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private RectTransform _rectTransform = null;

    #region Unity BuiltIn Methods
    protected override void Start()
    {
        base.Start();

        if (_camTarget == null)
            _camTarget = Camera.main;
        _rectTransform = GetComponent<RectTransform>();
    }

    protected override void FixedUpdate()
    {
        if (TargetFollow != null)
        {
            Vector3 screenPos = TargetFollow.position;
            Vector3 p = _rectTransform.position;

            _rectTransform.position = TargetFollow.position + new Vector3(_globatOffset.x, _globatOffset.y, 0);
        }
    }
    #endregion
}
