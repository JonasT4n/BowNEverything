using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(Camera))]
public class CameraAutoController : MonoBehaviour
{
    [SerializeField] private float _camMoveSpeed = 5f;
    [SerializeField] private Vector2 _offsetPosition = Vector2.zero;

    private Camera _cam;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float _zoomSize = 8;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector2 _centerFollowPoint = Vector2.zero;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private List<Transform> _targets = new List<Transform>();

    #region Unity BuiltIn Methods
    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void Start()
    {
        //PlayerEntity[] ents = FindObjectsOfType<PlayerEntity>();
        //foreach (PlayerEntity e in ents)
        //    _targets.Add(e.transform);
    }

    // Update is called once per frame
    private void Update()
    {
        CalculatePivot();
        MoveCameraHandler();
    }
    #endregion

    private void CalculatePivot()
    {
        for (int i = 0; i < _targets.Count; i++)
        {
            Vector3 targetPos = _targets[i].position;
            if (i == 0)
                _centerFollowPoint = targetPos;
            else
                _centerFollowPoint = (_centerFollowPoint + new Vector2(targetPos.x, targetPos.y)) / 2;
        }
    }

    private void MoveCameraHandler()
    {
        Vector2 targetPosition = _centerFollowPoint + _offsetPosition;

        // Smooth Camera Follow Motion
        Vector3 pos = transform.position;
        Vector3 moveDir = (targetPosition - new Vector2(pos.x, pos.y)).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > 0f)
        {
            Vector3 afterMove = transform.position + moveDir * _camMoveSpeed * distance;
            float distanceAfterMove = Vector3.Distance(targetPosition, afterMove);

            if (distanceAfterMove > distance || distance < 0.01f)
                afterMove = targetPosition;

            transform.position = new Vector3(afterMove.x, afterMove.y, transform.position.z);
        }
    }
}
