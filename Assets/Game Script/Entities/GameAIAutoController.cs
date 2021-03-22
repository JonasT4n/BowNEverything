using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GameAIAutoController : MonoBehaviour
{
    private const float GROUNDED_SENSITIVITY = 0.01f;

    [Header("Controller Attributes")]
    [SerializeField] private LayerMask _groundMask = ~0;
    [SerializeField] private LayerMask _wallMask = ~0;
    [SerializeField] private float _jumpForce = 0.75f;
    [SerializeField] private float _gravityWeight = 2.85f;
    [SerializeField] private BoxCollider2D _collider = null;

    [Space, Header("Auto Controller Attributes")]
    [SerializeField] private LayerMask _enemyDetectMask = ~0;
    [SerializeField] private float _attackInterval = 2f;
    [SerializeField] private Vector2 _detectRangeXY = new Vector2(100f, 5f);
    [SerializeField] private float _targetDistanceAttack = 1f;

    private EnemyEntity _entityControlled;
    private Vector2 _onPauseMoveDirHolder;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _isGrounded = false;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float _attackTimeHolder;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Transform _targetDetected = null;

    #region Unity BuiltIn Methods
    private void Awake()
    {
        _entityControlled = GetComponent<EnemyEntity>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        _attackTimeHolder = _attackInterval;
    }

    // Update is called once per frame
    private void Update()
    {
        DetectEnemy();
        PhysicalBodyControl();
    }
    #endregion

    private void PhysicalBodyControl()
    {
        // Get info origin
        Vector2 curMoveVel = _entityControlled.CurrentVelocity;

        // Check X Control
        if (_targetDetected != null)
        {
            float targetDistance = Vector2.Distance(_targetDetected.position, transform.position);
            if (targetDistance < Vector2.Distance(_detectRangeXY, Vector2.zero))
            {
                curMoveVel.x = (_targetDetected.position.x - transform.position.x < 0 ? -1f :
                    _targetDetected.position.x - transform.position.x > 0 ? 1f : 0) * _entityControlled.Speed;

                // Check attack control
                if (targetDistance < _targetDistanceAttack)
                {
                    curMoveVel.x = 0;
                    RunAttackInterval();
                }
            }
        }
        else
        {
            float prevXDir = curMoveVel.x;
            curMoveVel.x += curMoveVel.x < 0 ? _gravityWeight * Time.deltaTime : (curMoveVel.x > 0 ? -_gravityWeight * Time.deltaTime : 0);
            if ((curMoveVel.x < 0 && prevXDir > 0) || (curMoveVel.x > 0 && prevXDir < 0))
                curMoveVel.x = 0;
        }

        // Check Y control
        Collider2D groundCol = CheckAIOnGround();
        _isGrounded = groundCol == null ? false : true;

        if (curMoveVel.y > 0)
        {
            _isGrounded = false;

            Collider2D beforeHitCeiling = CheckCeiling();
            if (beforeHitCeiling != null)
            {
                if (beforeHitCeiling.tag != "Platform")
                    curMoveVel.y = 0;
            }
        }

        if (_isGrounded)
        {
            curMoveVel.y = 0;
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
            Collider2D beforeGroundCol = CheckAIOnGround(true);
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
        _entityControlled.CurrentVelocity = curMoveVel;
    }

    private void IdlePatrolControl()
    {

    }

    private Collider2D CheckAIOnGround(bool afterDropChecker = false)
    {
        // Get collider information
        Vector3 centerCol = _collider.bounds.center, extentCol = _collider.size / 2;

        Collider2D hit = null;
        if (afterDropChecker && _entityControlled.CurrentVelocity.y < 0)
        {
            // Condition
            Vector2 boxSize = new Vector2(_collider.size.x, Mathf.Abs(_entityControlled.CurrentVelocity.y));
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
        Vector2 boxSize = new Vector2(_collider.size.x, Mathf.Abs(_entityControlled.CurrentVelocity.y));
        Vector3 origin = centerCol + new Vector3(0, extentCol.y + boxSize.y, 0);

        return Physics2D.BoxCast(origin, boxSize, 0, Vector2.down, boxSize.y, _groundMask).collider;
    }

    private void Teleport(Vector3 position)
    {

    }

    private void DetectEnemy()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(_collider.bounds.center, _detectRangeXY, 0, Vector2.zero, 0, _enemyDetectMask);
        float nearestEnemyDistance = Mathf.Infinity;
        Collider2D targetLocked = null;
        foreach(RaycastHit2D h in hits)
        {
            if (h.collider.gameObject.Equals(gameObject) || h.collider.gameObject.GetComponent<EnemyEntity>())
                continue;

            Vector3 targetPos = h.collider.transform.position;
            if (Vector2.Distance(targetPos, transform.position) < nearestEnemyDistance)
                targetLocked = h.collider;
        }

        _targetDetected = targetLocked == null ? null : targetLocked.transform;
    }

    private void RunAttackInterval()
    {
        if (_attackTimeHolder > 0)
        {
            _attackTimeHolder -= Time.deltaTime;
        }
        else
        {
            _attackTimeHolder = _attackInterval;
            _entityControlled.Attack(_targetDetected);
        }
    }
}
