using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;
using BNEGame;
using BNEGame.Network;

public class CameraAutoController : MonoBehaviour
{
    private const float SIZE_LIMIT = 50f;
    private const int MAX_DETECT = 4;

    [Header("Auto Controller Attributes")]
    [SerializeField] private float cameraMoveSpeed = 1f;
    [SerializeField] private CinemachineVirtualCamera virtualCamera = null;
    //[SerializeField] private float povChangeSpeed = 1f;
    //[SerializeField] private float maxRangeCameraEntity = 100f;
    //[SerializeField] private float sizeIncrementPerEntity = 1f;
    [InfoBox("This Main Focus transform will keep the camera follow this target. It will calculate all living entities from this transform to know" +
        " other entities by distance.")]
    [SerializeField] private Transform targetHook = null;
    //[SerializeField] private Vector2 maxMoveCoor = new Vector2(120f, 40f);
    //[SerializeField] private Vector2 minMoveCoor = new Vector2(-50f, -35);

    //private Camera _cam;

    //[BoxGroup("DEBUG"), SerializeField, ReadOnly] private float fovSizeOrigin = 0;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector2 centerFollowPoint = Vector2.zero;
    //[BoxGroup("DEBUG"), SerializeField, ReadOnly] private List<Transform> targets = new List<Transform>();

    public Transform CenterHook
    {
        set => targetHook = value;
        get => targetHook;
    }

    #region Unity BuiltIn Methods
    private void Awake()
    {
        // Subscribe events
        EventHandler.OnEntityDeathEvent += HandlePlayerDeathFocus;
        EventHandler.OnPlayerRespawnEvent += HandlePlayerCameraAttach;

        //_cam = GetComponent<Camera>();
    }

    //private void Start()
    //{
    //    fovSizeOrigin = _cam.orthographicSize;
    //}

    // Update is called once per frame
    private void FixedUpdate()
    {
        //CalculatePivot();
        //MoveCameraHandler();

        // Set follow target
        if (targetHook != null)
        {
            if (virtualCamera.Follow == null)
                virtualCamera.Follow = targetHook;

            //DetectEntityNearby();
        }
        else
        {
            if (virtualCamera.Follow != null)
                virtualCamera.Follow = null;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe events
        EventHandler.OnEntityDeathEvent -= HandlePlayerDeathFocus;
        EventHandler.OnPlayerRespawnEvent -= HandlePlayerCameraAttach;
    }
    #endregion

    #region Event Methods
    private void HandlePlayerDeathFocus(EntityDeathEventArgs args)
    {
        // Check player died is whom the camera focus following on
        if (args.EntityVictim.transform.Equals(targetHook))
            targetHook = null;
    }

    private void HandlePlayerCameraAttach(PlayerRespawnEventArgs args)
    {
        // Check if in multiplayer scene game
        if (Mirror.NetworkManager.singleton != null)
        {
            NetworkInGamePlayer player = args.Player.GetComponent<NetworkInGamePlayer>();
            targetHook = player == null ? null : player.isLocalPlayer ? player.transform : null;
            return;
        }

        // Else then single player games
        targetHook = args.Player.transform;
    }
    #endregion

    //private void CalculatePivot()
    //{
    //    if (targetHook != null)
    //    {
    //        centerFollowPoint = targetHook.position;

    //        for (int i = 0; i < targets.Count; i++)
    //        {
    //            if (targets[i] == null)
    //            {
    //                targets.RemoveAt(i);
    //                i--;
    //                continue;
    //            }

    //            Vector3 targetPos = targets[i].position;
    //            centerFollowPoint = (centerFollowPoint + new Vector2(targetPos.x, targetPos.y)) / 2;
    //        }
    //    }
    //    else
    //    {
    //        centerFollowPoint = Vector2.zero;
    //    }
    //}

    //private void MoveCameraHandler()
    //{
    //    Vector2 targetPosition = _centerFollowPoint + offsetPosition;

    //    // Smooth Camera Follow Motion
    //    Vector3 pos = transform.position;
    //    Vector3 moveDir = (targetPosition - new Vector2(pos.x, pos.y)).normalized;
    //    float distance = Vector3.Distance(transform.position, targetPosition);

    //    if (distance > 0f)
    //    {
    //        Vector3 afterMove = transform.position + moveDir * _camMoveSpeed * distance * Time.deltaTime;
    //        float distanceAfterMove = Vector3.Distance(targetPosition, afterMove);

    //        if (distanceAfterMove > distance || distance < 0.005f)
    //            afterMove = targetPosition;

    //        if (afterMove.x < _minMoveCoor.x)
    //            afterMove.x = _minMoveCoor.x;
    //        if (afterMove.x > _maxMoveCoor.x)
    //            afterMove.x = _maxMoveCoor.x;
    //        if (afterMove.y < _minMoveCoor.y)
    //            afterMove.y = _minMoveCoor.y;
    //        if (afterMove.y > _maxMoveCoor.y)
    //            afterMove.y = _maxMoveCoor.y;

    //        transform.position = new Vector3(afterMove.x, afterMove.y, transform.position.z);
    //    }

    //    // Smooth adjust camera pov size
    //    float targetSize = _fovSizeOrigin + _sizeIncrementPerEntity * _targets.Count;
    //    float inbetweenSize = Mathf.Abs(targetSize - _cam.orthographicSize);
    //    if (_cam.orthographicSize < targetSize)
    //    {
    //        float afterResizing = _cam.orthographicSize + _povChangeSpeed * inbetweenSize * Time.deltaTime;
    //        if (afterResizing > targetSize)
    //            afterResizing = targetSize;

    //        _cam.orthographicSize = afterResizing;
    //    }
    //    else if (_cam.orthographicSize > targetSize)
    //    {
    //        float afterResizing = _cam.orthographicSize - _povChangeSpeed * inbetweenSize * Time.deltaTime;
    //        if (afterResizing < targetSize)
    //            afterResizing = targetSize;

    //        _cam.orthographicSize = afterResizing;
    //    }

    //    if (_cam.orthographicSize > SIZE_LIMIT)
    //        _cam.orthographicSize = SIZE_LIMIT;
    //}

    //private void DetectEntityNearby()
    //{
    //    if (_targets.Count < MAX_DETECT)
    //    {
    //        LivingEntity[] entities = FindObjectsOfType<LivingEntity>();
    //        foreach (LivingEntity ent in entities)
    //        {
    //            if (ent.gameObject.Equals(targetHook.gameObject))
    //                continue;

    //            Vector3 entPos = ent.transform.position;
    //            if (Vector2.Distance(entPos, targetHook.position) <= _maxRangeCameraEntity && !_targets.Contains(ent.transform))
    //                _targets.Add(ent.transform);
    //            else if (Vector2.Distance(entPos, targetHook.position) > _maxRangeCameraEntity && _targets.Contains(ent.transform))
    //                _targets.Remove(ent.transform);
    //        }
    //    }

    //    // Remove all of death entities
    //    for (int i = 0; i < _targets.Count; i++)
    //    {
    //        Transform t = _targets[i];
    //        if (!t.gameObject.activeSelf)
    //        {
    //            _targets.Remove(t);
    //            i--;
    //        }
    //    }
    //}
}
