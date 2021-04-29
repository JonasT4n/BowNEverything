using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace BNEGame
{
    using Network;

    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Controller Attributes")]
        [SerializeField] private PlayerEntity _playerControlled = null;

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private InputData _inputData = null;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float _drawingTimeHandler = 0f;

        #region Unity BuiltIn Methods
        private void Awake()
        {
            _inputData = null;
            if (_inputData == null)
                _inputData = GetComponent<InputHandler>() == null ? null : GetComponent<InputHandler>().LocalInputData;
            if (_inputData == null)
                _inputData = GetComponent<OnlineInputHandler>() == null ? null : GetComponent<OnlineInputHandler>().LocalInputData;

            // Subscribe events
            EventHandler.OnGamePauseEvent += PauseGameEvent;
        }

        // Update is called once per frame
        private void Update()
        {
            if (_inputData != null)
            {
                if (!_playerControlled.IsPaused)
                {
                    AimControlHandler();

                    // Handle ammo rolling
                    if (_inputData.RollAmmoPressed)
                        _playerControlled.NextArrowUse();
                }

                if (!_playerControlled.IsPaused)
                    MovementControlHandler();
            }
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
            Vector2 curMoveVel = _playerControlled.EntityR2D.velocity;

            // Handle left right movement
            if (!_playerControlled.IsPaused)
            {
                if ((_inputData.MoveLeftHold && _inputData.MoveRightHold) || (!_inputData.MoveLeftHold && !_inputData.MoveRightHold))
                {
                    if (_playerControlled.IsGrounded && curMoveVel.y < 0)
                        curMoveVel = Vector2.zero;
                }
                else
                {
                    if (!_playerControlled.IsHitLeftWall && _inputData.MoveLeftHold)
                        curMoveVel.x = -_playerControlled.MoveSpeed;
                    if (!_playerControlled.IsHitRightWall && _inputData.MoveRightHold)
                        curMoveVel.x = _playerControlled.MoveSpeed;
                }
            }

            // Handle jump
            if (_playerControlled.IsGrounded)
            {
                if (!_playerControlled.IsPaused)
                {
                    if (_inputData.JumpPressed)
                        curMoveVel.y = _playerControlled.JumpForce;
                }
            }

            // Send back info to origin
            _playerControlled.EntityR2D.velocity = curMoveVel;
        }

        private void AimControlHandler()
        {
            // Handle aiming by cursor or target something
            Vector2 mousePos = _inputData.AimPosition;
            Vector2 aimDir = (mousePos - new Vector2(transform.position.x, transform.position.y)).normalized;

            _playerControlled.PlayerAnim.SetFloat("XDirection", aimDir.x);
            _playerControlled.AimDirection = aimDir;

            // Handle shoot
            if (_playerControlled.CurrentArrowUse != ArrowTypes.None)
            {
                if (_drawingTimeHandler <= 0 && _inputData.ShootReleased)
                {
                    Vector3 playerPos = _playerControlled.transform.position;
                    Vector2 shootDir = (mousePos - new Vector2(playerPos.x, playerPos.y)).normalized;
                    _playerControlled.BowShoot(shootDir, _playerControlled.BowShootOrigin);
                }

                if (_inputData.ShootHold)
                    _drawingTimeHandler -= Time.deltaTime;

                if (_inputData.ShootPressed)
                    _drawingTimeHandler = ObjectManager._instance.GetArrowElement(_playerControlled.CurrentArrowUse).DrawingTime;

                // Update drawing time information on ui
                EntityUIInformation info = _playerControlled.gameObject.GetComponent<EntityUIInformation>();
                if (info != null)
                {
                    if (_drawingTimeHandler == Mathf.Infinity)
                        info.DrawingTimeValue = 0;
                    else
                        if (ObjectManager._instance.GetArrowElement(_playerControlled.CurrentArrowUse) != null)
                        info.DrawingTimeValue = 1 - (_drawingTimeHandler / ObjectManager._instance.GetArrowElement(_playerControlled.CurrentArrowUse).DrawingTime);
                }
            }
        }
        #endregion

        #region Event Methods
        private void PauseGameEvent(PauseGamePressEventArgs args)
        {
            _playerControlled.IsPaused = args.IsPause;
            ArrowQuiverElement e = ObjectManager._instance.GetArrowElement(_playerControlled.CurrentArrowUse);
            _drawingTimeHandler = e == null ? Mathf.Infinity : e.DrawingTime;
        }
        #endregion

    }

}