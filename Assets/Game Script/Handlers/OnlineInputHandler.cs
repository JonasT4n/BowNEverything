using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class OnlineInputHandler : NetworkBehaviour, IInputHandler
{
    private bool _inputLocked = false;

    [Header("Input Attributes")]
    [SerializeField] private PCKeyMapControl _keyMap = new PCKeyMapControl() {
        MoveLeft = KeyCode.A,
        MoveRight = KeyCode.D,
        Jump = KeyCode.Space,
        Shoot = KeyCode.Mouse0,
        RollAmmo = KeyCode.Q,
        Pause = KeyCode.Escape
    };

    private bool _mobileControl = false;
    private InputData _inputData = new InputData();

    public bool UseMobileControl 
    { 
        set => _mobileControl = value; 
        get => _mobileControl; 
    }
    public bool InputLocked
    {
        set
        {
            _inputLocked = value;
            LocalInputData.ResetInput();
        }
    }
    public InputData LocalInputData => _inputData;
    public bool IsLocalPlayer => isLocalPlayer;

    #region Unity BuiltIn Methods
    private void Awake()
    {
        _inputData.ResetInput();
    }

    private void Start()
    {
        if (isLocalPlayer)
            FindObjectOfType<CameraAutoController>().CenterHook = transform;
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        if (_inputLocked)
            return;

        if (_mobileControl)
            MobileControlHandler();
        else
            PCControlHandler();
    }
    #endregion

    #region Control Handler Methods
    private void MobileControlHandler()
    {
        // TODO: Mobile controller
    }

    private void PCControlHandler()
    {
        _inputData.MoveLeftPressed = Input.GetKeyDown(_keyMap.MoveLeft);
        _inputData.MoveLeftReleased = Input.GetKeyUp(_keyMap.MoveLeft);

        if (_inputData.MoveLeftPressed)
            _inputData.MoveLeftHold = true;
        if (_inputData.MoveLeftReleased)
            _inputData.MoveLeftHold = false;

        _inputData.MoveRightPressed = Input.GetKeyDown(_keyMap.MoveRight);
        _inputData.MoveRightReleased = Input.GetKeyUp(_keyMap.MoveRight);

        if (_inputData.MoveRightPressed)
            _inputData.MoveRightHold = true;
        if (_inputData.MoveRightReleased)
            _inputData.MoveRightHold = false;

        _inputData.JumpPressed = Input.GetKeyDown(_keyMap.Jump);
        _inputData.JumpReleased = Input.GetKeyUp(_keyMap.Jump);

        if (_inputData.JumpPressed)
            _inputData.JumpHold = true;
        if (_inputData.JumpReleased)
            _inputData.JumpHold = false;

        _inputData.ShootPressed = Input.GetKeyDown(_keyMap.Shoot);
        _inputData.ShootReleased = Input.GetKeyUp(_keyMap.Shoot);

        if (_inputData.ShootPressed)
            _inputData.ShootHold = true;
        if (_inputData.ShootReleased)
            _inputData.ShootHold = false;

        _inputData.RollAmmoPressed = Input.GetKeyDown(_keyMap.RollAmmo);
        _inputData.PausePressed = Input.GetKeyDown(_keyMap.Pause);
        _inputData.ReturnEnterPressed = Input.GetKeyDown(KeyCode.Return);
        _inputData.AimPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    #endregion
}
