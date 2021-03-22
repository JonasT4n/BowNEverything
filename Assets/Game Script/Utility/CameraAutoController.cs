using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(Camera))]
public class CameraAutoController : MonoBehaviour
{
    private const float SIZE_LIMIT = 50f;
    private const int MAX_DETECT = 4;

    [Header("Auto Controller Attributes")]
    [SerializeField] private float _camMoveSpeed = 1f;
    [SerializeField] private float _povChangeSpeed = 1f;
    [SerializeField] private float _maxRangeCameraEntity = 100f;
    [SerializeField] private float _sizeIncrementPerEntity = 1f;
    [InfoBox("This Main Focus transform will keep the camera follow this target. It will calculate all living entities from this transform to know" +
        " other entities by distance.")]
    [SerializeField] private Transform _centerHook = null;
    [SerializeField] private Vector2 _offsetPosition = Vector2.zero;
    [SerializeField] private Vector2 _maxMoveCoor = new Vector2(120f, 40f);
    [SerializeField] private Vector2 _minMoveCoor = new Vector2(-50f, -35);

    private Camera _cam;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float _fovSizeOrigin = 0;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector2 _centerFollowPoint = Vector2.zero;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private List<Transform> _targets = new List<Transform>();

    public Transform CenterHook
    {
        set => _centerHook = value;
        get => _centerHook;
    }

    #region Unity BuiltIn Methods
    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void Start()
    {
        _fovSizeOrigin = _cam.orthographicSize;
    }

    // Update is called once per frame
    private void Update()
    {
        CalculatePivot();
        MoveCameraHandler();

        if (_centerHook != null)
            DetectEntityNearby();
    }
    #endregion

    private void CalculatePivot()
    {
        if (_centerHook != null)
            _centerFollowPoint = _centerHook.position;
        else
            _centerFollowPoint = Vector2.zero;

        for (int i = 0; i < _targets.Count; i++)
        {
            Vector3 targetPos = _targets[i].position;
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
            Vector3 afterMove = transform.position + moveDir * _camMoveSpeed * distance * Time.deltaTime;
            float distanceAfterMove = Vector3.Distance(targetPosition, afterMove);

            if (distanceAfterMove > distance || distance < 0.005f)
                afterMove = targetPosition;

            if (afterMove.x < _minMoveCoor.x)
                afterMove.x = _minMoveCoor.x;
            if (afterMove.x > _maxMoveCoor.x)
                afterMove.x = _maxMoveCoor.x;
            if (afterMove.y < _minMoveCoor.y)
                afterMove.y = _minMoveCoor.y;
            if (afterMove.y > _maxMoveCoor.y)
                afterMove.y = _maxMoveCoor.y;

            transform.position = new Vector3(afterMove.x, afterMove.y, transform.position.z);
        }

        // Smooth adjust camera pov size
        float targetSize = _fovSizeOrigin + _sizeIncrementPerEntity * _targets.Count;
        float inbetweenSize = Mathf.Abs(targetSize - _cam.orthographicSize);
        if (_cam.orthographicSize < targetSize)
        {
            float afterResizing = _cam.orthographicSize + _povChangeSpeed * inbetweenSize * Time.deltaTime;
            if (afterResizing > targetSize)
                afterResizing = targetSize;

            _cam.orthographicSize = afterResizing;
        }
        else if (_cam.orthographicSize > targetSize)
        {
            float afterResizing = _cam.orthographicSize - _povChangeSpeed * inbetweenSize * Time.deltaTime;
            if (afterResizing < targetSize)
                afterResizing = targetSize;

            _cam.orthographicSize = afterResizing;
        }

        if (_cam.orthographicSize > SIZE_LIMIT)
            _cam.orthographicSize = SIZE_LIMIT;
    }

    private void DetectEntityNearby()
    {
        if (_targets.Count < MAX_DETECT)
        {
            LivingEntity[] entities = FindObjectsOfType<LivingEntity>();
            foreach (LivingEntity ent in entities)
            {
                if (ent.gameObject.Equals(_centerHook.gameObject))
                    continue;

                Vector3 entPos = ent.transform.position;
                if (Vector2.Distance(entPos, _centerHook.position) <= _maxRangeCameraEntity && !_targets.Contains(ent.transform))
                    _targets.Add(ent.transform);
                else if (Vector2.Distance(entPos, _centerHook.position) > _maxRangeCameraEntity && _targets.Contains(ent.transform))
                    _targets.Remove(ent.transform);
            }
        }

        // Remove all of death entities
        for (int i = 0; i < _targets.Count; i++)
        {
            Transform t = _targets[i];
            if (!t.gameObject.activeSelf)
            {
                _targets.Remove(t);
                i--;
            }
        }
    }
}
