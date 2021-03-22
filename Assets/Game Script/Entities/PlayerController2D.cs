using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController2D : MonoBehaviour
{
    private const float DETECT_RANGE = 0.01f;

    [Header("Controller Attributes")]
    [SerializeField] private LayerMask _groundMask = ~0;
    [SerializeField] private LayerMask _wallMask = ~0;
    [SerializeField] private float _jumpForce = 0.75f;
    [SerializeField] private float _gravityWeight = 2.85f;
    [SerializeField] private Transform _aimPivot = null;
    [SerializeField] private PlayerEntity _playerControlled = null;

    private BoxCollider2D _collider;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _isGrounded = false;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float _drawingTimeHandler = 0f;
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
        // Get info origin
        Vector2 curMoveVel = _playerControlled.CurrentVelocity;

        // Handle left right movement
        InputData dat = InputHandler.LocalInputData;
        if ((dat.MoveLeftHold && dat.MoveRightHold) || (!dat.MoveLeftHold && !dat.MoveRightHold))
        {
            float prevXDir = curMoveVel.x;
            curMoveVel.x += curMoveVel.x < 0 ? _gravityWeight * Time.deltaTime : (curMoveVel.x > 0 ? -_gravityWeight * Time.deltaTime : 0);
            if ((curMoveVel.x < 0 && prevXDir > 0) || (curMoveVel.x > 0 && prevXDir < 0))
                curMoveVel.x = 0;
        }
        else
        {
            if (!_playerControlled.IsPaused)
            {
                if (dat.MoveLeftHold)
                    curMoveVel.x = -_playerControlled.Speed;
                if (dat.MoveRightHold)
                    curMoveVel.x = _playerControlled.Speed;
            }
        }

        // Handle jump
        Collider2D groundCol = HitTheGround();
        _isGrounded = groundCol == null ? false : true;
        if (curMoveVel.y > 0)
        {
            _isGrounded = false;

            Collider2D beforeHitCeiling = HitCeilingCollider();
            if (beforeHitCeiling != null)
            {
                if (beforeHitCeiling.tag != "Platform")
                    curMoveVel.y = 0;
            }
        }

        if (_isGrounded)
        {
            curMoveVel.y = 0;
            if (!_playerControlled.IsPaused)
            {
                if (dat.JumpPressed)
                    curMoveVel.y = _jumpForce;
            }
        }
        else
        {
            // Make sure the jump is consistent
            if (curMoveVel.y > _jumpForce)
                curMoveVel.y = _jumpForce;

            curMoveVel.y -= _gravityWeight * Time.deltaTime;
            if (curMoveVel.y > _gravityWeight)
                curMoveVel.y = _gravityWeight;

            // Check before drop collision
            Collider2D beforeGroundCol = HitTheGround(true);
            if (beforeGroundCol != null)
            {
                float bottomMostY = _collider.bounds.center.y - _collider.bounds.extents.y + curMoveVel.y;
                float topMostColY = beforeGroundCol.bounds.center.y + beforeGroundCol.bounds.extents.y;

                if (bottomMostY < topMostColY)
                    curMoveVel.y += topMostColY - bottomMostY;
            }
        }

        // Return information to origin
        transform.Translate(curMoveVel);

        // Send back info to origin
        _playerControlled.CurrentVelocity = curMoveVel;
    }

    private Collider2D HitTheGround(bool afterDropChecker = false)
    {
        // Get collider information
        Vector3 centerCol = _collider.bounds.center, extentCol = _collider.size / 2;

        Collider2D hit = null;
        if (afterDropChecker && _playerControlled.CurrentVelocity.y < 0)
        {
            // Condition
            Vector2 boxSize = new Vector2(_collider.size.x, Mathf.Abs(_playerControlled.CurrentVelocity.y));
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
            Vector2 boxSize = new Vector2(_collider.size.x, DETECT_RANGE);
            Vector3 origin = centerCol - new Vector3(0, extentCol.y + boxSize.y, 0);
            hit = Physics2D.BoxCast(origin, boxSize, 0, Vector2.down, boxSize.y, _groundMask).collider;

            #if UNITY_EDITOR
            // Debugger
            Vector2 leftMostBound = new Vector2(centerCol.x - extentCol.x, centerCol.y - extentCol.y);
            Vector2 rightMostBound = new Vector2(centerCol.x + extentCol.x, centerCol.y - extentCol.y);
            float distance = Vector2.Distance(leftMostBound, rightMostBound);

            Debug.DrawRay(leftMostBound, -new Vector2(0, DETECT_RANGE), Color.cyan);
            Debug.DrawRay(rightMostBound, -new Vector2(0, DETECT_RANGE), Color.cyan);
            Debug.DrawRay(leftMostBound, new Vector2(distance, 0), Color.cyan);
            Debug.DrawRay(leftMostBound - new Vector2(0, DETECT_RANGE), new Vector2(distance, 0), Color.cyan);
            #endif
        }

        return hit;
    }

    private Collider2D HitCeilingCollider()
    {
        // Get collider information
        Vector3 centerCol = _collider.bounds.center, extentCol = _collider.size / 2;

        // Condition
        Vector2 boxSize = new Vector2(_collider.size.x, Mathf.Abs(_playerControlled.CurrentVelocity.y));
        Vector3 origin = centerCol + new Vector3(0, extentCol.y + boxSize.y, 0);

        return Physics2D.BoxCast(origin, boxSize, 0, Vector2.down, boxSize.y, _groundMask).collider;
    }

    private bool IsHitLeftWall()
    {
        // Get collider information
        Vector3 centerCol = _collider.bounds.center, extentCol = _collider.size / 2;

        // Condition
        Vector2 boxSize = new Vector2(DETECT_RANGE, _collider.size.y);
        Vector3 originLeft = centerCol - new Vector3(extentCol.x + boxSize.x, 0);
        
        RaycastHit2D leftHit = Physics2D.BoxCast(originLeft, boxSize, 0, Vector2.left, boxSize.x, _wallMask);
        Debug.Log(leftHit.collider);

        return leftHit.collider != null ? true : false;
    }

    private bool IsHitRightWall()
    {
        // Get collider information
        Vector3 centerCol = _collider.bounds.center, extentCol = _collider.size / 2;

        // Condition
        Vector2 boxSize = new Vector2(DETECT_RANGE, _collider.size.y);
        Vector3 originRight = centerCol + new Vector3(extentCol.x + boxSize.x, 0);

        RaycastHit2D rightHit = Physics2D.BoxCast(originRight, boxSize, 0, Vector2.right, boxSize.x, _wallMask);

        return rightHit.collider != null ? true : false;
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
            {
                _drawingTimeHandler = ObjectManager._instance.GetArrowElement(_playerControlled.CurrentUse).DrawingTime;
                _playerControlled.CallPullArrowEvent();
            }

            // Update drawing time information on ui
            EntityUIInformation info = _playerControlled.gameObject.GetComponent<EntityUIInformation>();
            if (info != null)
            {
                if (_drawingTimeHandler == Mathf.Infinity)
                    info.DrawingTimeValue = 0;
                else
                    if (ObjectManager._instance.GetArrowElement(_playerControlled.CurrentUse) != null)
                        info.DrawingTimeValue = 1 - (_drawingTimeHandler / ObjectManager._instance.GetArrowElement(_playerControlled.CurrentUse).DrawingTime);
            }
        }
    }
    #endregion

    #region Event Methods
    private void PauseGameEvent(PauseGamePressEventArgs args)
    {
        _playerControlled.IsPaused = args.IsPause;
        ArrowQuiverElement e = ObjectManager._instance.GetArrowElement(_playerControlled.CurrentUse);
        _drawingTimeHandler = e == null ? Mathf.Infinity : e.DrawingTime;
    }
    #endregion

}
