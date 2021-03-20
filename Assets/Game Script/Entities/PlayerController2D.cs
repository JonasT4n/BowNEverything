using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    private const float GROUNDED_SENSITIVITY = 0.01f;

    [Header("Controller Attributes")]
    [SerializeField] private LayerMask _groundMask = ~0;
    [SerializeField] private float _speed = 0.425f;
    [SerializeField] private float _jumpForce = 0.75f;
    [SerializeField] private float _gravityWeight = 2.85f;
    [SerializeField] private Transform _aimPivot = null;
    [SerializeField] private PlayerEntity _playerControlled = null;

    private BoxCollider2D _collider;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _isGrounded = false;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float _drawingTimeHandler = 0f;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector2 _moveDir = Vector2.zero;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector2 _aimDir = Vector2.zero;

    public bool IsGrounded
    {
        get => _isGrounded;
    }

    #region Unity BuiltIn Methods
    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();

        // Subscribe events
        EventHandler.OnGamePauseEvent += PauseGameEvent;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_playerControlled.IsPaused)
        {
            AimControlHandler();

            // Handle ammo rolling
            if (InputHandler.LocalInputData.RollAmmoPressed)
                _playerControlled.NextArrowUse();
        }

        if (!_playerControlled.IsPaused || GameManager._instance.CurrentGameMode == GameModeState.MultiPlayer)
            MovementControlHandler();
    }

    private void OnDestroy()
    {
        // Subscribe events
        EventHandler.OnGamePauseEvent -= PauseGameEvent;
    }
    #endregion

    #region Handler Methods
    private void MovementControlHandler()
    {
        // Handle left right movement
        InputData dat = InputHandler.LocalInputData;
        if ((dat.MoveLeftHold && dat.MoveRightHold) || (!dat.MoveLeftHold && !dat.MoveRightHold))
        {
            _moveDir.x = 0;
        }
        else
        {
            if (!_playerControlled.IsPaused)
            {
                if (dat.MoveLeftHold)
                    _moveDir.x = -_speed;
                if (dat.MoveRightHold)
                    _moveDir.x = _speed;
            }
        }

        // Handle jump
        Collider2D groundCol = CheckPlayerOnGround();
        _isGrounded = groundCol == null ? false : true;
        if (_moveDir.y > 0)
        {
            _isGrounded = false;

            Collider2D beforeHitCeiling = CheckCeiling();
            if (beforeHitCeiling != null)
            {
                if (beforeHitCeiling.tag != "Platform")
                    _moveDir.y = 0;
            }
        }

        if (_isGrounded)
        {
            _moveDir.y = 0;
            if (!_playerControlled.IsPaused)
            {
                if (dat.JumpPressed)
                    _moveDir.y = _jumpForce;
            }
        }
        else
        {
            // Make sure the jump is consistent
            if (_moveDir.y > _jumpForce)
                _moveDir.y = _jumpForce;

            _moveDir.y -= _gravityWeight * Time.deltaTime;
            if (_moveDir.y > _gravityWeight)
                _moveDir.y = _gravityWeight;

            // Check before drop collision
            Collider2D beforeGroundCol = CheckPlayerOnGround(true);
            if (beforeGroundCol != null)
            {
                float bottomMostY = _collider.bounds.center.y - _collider.bounds.extents.y + _moveDir.y;
                float topMostColY = beforeGroundCol.bounds.center.y + beforeGroundCol.bounds.extents.y;

                if (bottomMostY < topMostColY)
                    _moveDir.y += topMostColY - bottomMostY;
            }
        }

        //Debug.Log($"Direction Move: {_moveDir}; Player Pos Y: {transform.position.y}");

        // Return information to origin
        transform.Translate(_moveDir);
    }

    private Collider2D CheckPlayerOnGround(bool afterDropChecker = false)
    {
        // Get collider information
        Vector3 centerCol = _collider.bounds.center, extentCol = _collider.size / 2;

        Collider2D hit = null;
        if (afterDropChecker && _moveDir.y < 0)
        {
            // Condition
            Vector2 boxSize = new Vector2(_collider.size.x, Mathf.Abs(_moveDir.y));
            Vector3 origin = centerCol - new Vector3(0, extentCol.y + boxSize.y, 0);
            hit = Physics2D.BoxCast(origin, boxSize, 0, Vector2.down, boxSize.y, _groundMask).collider;

            #if UNITY_EDITOR
            // Debugger
            Vector2 leftMostBound = new Vector2(centerCol.x - extentCol.x, centerCol.y - extentCol.y);
            Vector2 rightMostBound = new Vector2(centerCol.x + extentCol.x, centerCol.y - extentCol.y);
            float distance = Vector2.Distance(leftMostBound, rightMostBound);

            Debug.DrawRay(leftMostBound, -new Vector2(0, boxSize.y / 2), Color.red);
            Debug.DrawRay(rightMostBound, -new Vector2(0, boxSize.y / 2), Color.red);
            Debug.DrawRay(leftMostBound, new Vector2(distance, 0), Color.red);
            Debug.DrawRay(leftMostBound - new Vector2(0, boxSize.y / 2), new Vector2(distance, 0), Color.red);
            #endif
        }
        else
        {
            // Condition
            Vector2 boxSize = new Vector2(_collider.size.x, GROUNDED_SENSITIVITY);
            Vector3 origin = centerCol - new Vector3(0, extentCol.y + boxSize.y, 0);
            hit = Physics2D.BoxCast(origin, boxSize, 0, Vector2.down, boxSize.y, _groundMask).collider;

            #if UNITY_EDITOR
            // Debugger
            Vector2 leftMostBound = new Vector2(centerCol.x - extentCol.x, centerCol.y - extentCol.y);
            Vector2 rightMostBound = new Vector2(centerCol.x + extentCol.x, centerCol.y - extentCol.y);
            float distance = Vector2.Distance(leftMostBound, rightMostBound);

            Debug.DrawRay(leftMostBound, -new Vector2(0, GROUNDED_SENSITIVITY), Color.cyan);
            Debug.DrawRay(rightMostBound, -new Vector2(0, GROUNDED_SENSITIVITY), Color.cyan);
            Debug.DrawRay(leftMostBound, new Vector2(distance, 0), Color.cyan);
            Debug.DrawRay(leftMostBound - new Vector2(0, GROUNDED_SENSITIVITY), new Vector2(distance, 0), Color.cyan);
            #endif
        }

        return hit;
    }

    private Collider2D CheckCeiling()
    {
        // Get collider information
        Vector3 centerCol = _collider.bounds.center, extentCol = _collider.size / 2;

        // Condition
        Vector2 boxSize = new Vector2(_collider.size.x, Mathf.Abs(_moveDir.y));
        Vector3 origin = centerCol + new Vector3(0, extentCol.y + boxSize.y, 0);

        return Physics2D.BoxCast(origin, boxSize, 0, Vector2.down, boxSize.y, _groundMask).collider;
    }

    private void AimControlHandler()
    {
        // Handle aiming by cursor or target something
        InputData localData = InputHandler.LocalInputData;
        Vector2 mousePos = localData.AimPosition;
        _aimDir = (mousePos - new Vector2(transform.position.x, transform.position.y)).normalized;

        float degreeAngle = Mathf.Atan(_aimDir.y / _aimDir.x) * Mathf.Rad2Deg;

        if (mousePos.x < transform.position.x)
            degreeAngle += 180f;

        _playerControlled.PlayerAnim.SetFloat("XDirection", _aimDir.x);
        _aimPivot.eulerAngles = new Vector3(0, 0, degreeAngle);

        // Handle shoot
        if (_playerControlled.CurrentUse != ArrowTypes.None)
        {
            if (_drawingTimeHandler <= 0 && localData.ShootReleased)
            {
                Vector3 playerPos = _playerControlled.transform.position;
                Vector2 shootDir = (mousePos - new Vector2(playerPos.x, playerPos.y)).normalized;
                _playerControlled.BowShoot(shootDir);
            }

            if (localData.ShootHold)
                _drawingTimeHandler -= Time.deltaTime;

            if (localData.ShootPressed)
                _drawingTimeHandler = ObjectManager.GetArrowElement(_playerControlled.CurrentUse).DrawingTime;

            // Update drawing time information on ui
            EntityUIInformation info = _playerControlled.gameObject.GetComponent<EntityUIInformation>();
            if (info != null)
            {
                if (_drawingTimeHandler == Mathf.Infinity)
                    info.DrawingTimeValue = 0;
                else
                    if (ObjectManager.GetArrowElement(_playerControlled.CurrentUse) != null)
                        info.DrawingTimeValue = 1 - (_drawingTimeHandler / ObjectManager.GetArrowElement(_playerControlled.CurrentUse).DrawingTime);
            }
        }
    }
    #endregion

    #region Event Methods
    private void PauseGameEvent(PauseGamePressEventArgs args)
    {
        _playerControlled.IsPaused = args.IsPause;
        ArrowQuiverElement e = ObjectManager.GetArrowElement(_playerControlled.CurrentUse);
        _drawingTimeHandler = e == null ? Mathf.Infinity : e.DrawingTime;
    }
    #endregion

}
