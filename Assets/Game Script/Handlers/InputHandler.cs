using System.Collections;
using Mirror;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public struct PCKeyMapControl
{
    [SerializeField] private KeyCode _left;
    [SerializeField] private KeyCode _right;
    [SerializeField] private KeyCode _jump;
    [SerializeField] private KeyCode _shoot;
    [SerializeField] private KeyCode _rollAmmo;
    [SerializeField] private KeyCode _pause;

    public KeyCode MoveLeft
    {
        set => _left = value;
        get => _left;
    }

    public KeyCode MoveRight
    {
        set => _right = value;
        get => _right;
    }

    public KeyCode Jump
    {
        set => _jump = value;
        get => _jump;
    }

    public KeyCode Shoot
    {
        set => _shoot = value;
        get => _shoot;
    }

    public KeyCode RollAmmo
    {
        set => _rollAmmo = value;
        get => _rollAmmo;
    }

    public KeyCode Pause
    {
        set => _pause = value;
        get => _pause;
    }
}

[System.Serializable]
public class InputData
{
    [SerializeField] private bool _leftPressed;
    [SerializeField] private bool _leftHold;
    [SerializeField] private bool _leftRelease;

    [SerializeField] private bool _rightPressed;
    [SerializeField] private bool _rightHold;
    [SerializeField] private bool _rightReleased;

    [SerializeField] private bool _jumpPressed;
    [SerializeField] private bool _jumpHold;
    [SerializeField] private bool _jumpReleased;

    [SerializeField] private bool _shootPressed;
    [SerializeField] private bool _shootHold;
    [SerializeField] private bool _shootReleased;

    [SerializeField] private bool _rollAmmoPressed;

    [SerializeField] private bool _pausePressed;

    [SerializeField] private bool _enterPressed;

    [SerializeField] private Vector2 _aimPos;

    public bool MoveLeftPressed
    {
        set => _leftPressed = value;
        get => _leftPressed;
    }

    public bool MoveLeftHold
    {
        set => _leftHold = value;
        get => _leftHold;
    }

    public bool MoveLeftReleased
    {
        set => _leftRelease = value;
        get => _leftRelease;
    }

    public bool MoveRightPressed
    {
        set => _rightPressed = value;
        get => _rightPressed;
    }

    public bool MoveRightHold
    {
        set => _rightHold = value;
        get => _rightHold;
    }

    public bool MoveRightReleased
    {
        set => _rightReleased = value;
        get => _rightReleased;
    }

    public bool JumpPressed
    {
        set => _jumpPressed = value;
        get => _jumpPressed;
    }

    public bool JumpHold
    {
        set => _jumpHold = value;
        get => _jumpHold;
    }

    public bool JumpReleased
    {
        set => _jumpReleased = value;
        get => _jumpReleased;
    }

    public bool ShootPressed
    {
        set => _shootPressed = value;
        get => _shootPressed;
    }

    public bool ShootHold
    {
        set => _shootHold = value;
        get => _shootHold;
    }

    public bool ShootReleased
    {
        set => _shootReleased = value;
        get => _shootReleased;
    }

    public bool RollAmmoPressed
    {
        set => _rollAmmoPressed = value;
        get => _rollAmmoPressed;
    }

    public bool PausePressed
    {
        set => _pausePressed = value;
        get => _pausePressed;
    }

    public bool ReturnEnterPressed
    {
        set => _enterPressed = value;
        get => _enterPressed;
    }

    public Vector2 AimPosition
    {
        set => _aimPos = value;
        get => _aimPos;
    }

    public void ResetInput()
    {
        _leftPressed = false;
        _leftHold = false;
        _leftRelease = false;

        _rightPressed = false;
        _rightHold = false;
        _rightReleased = false;

        _jumpPressed = false;
        _jumpHold = false;
        _jumpReleased = false;

        _shootPressed = false;
        _shootHold = false;
        _shootReleased = false;

        _aimPos = Vector2.zero;
    }
}

[RequireComponent(typeof(PlayerController2D))]
public class InputHandler : MonoBehaviour, IInputHandler
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

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _mobileControl = false;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private InputData _inputData = new InputData();

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
    public bool IsLocalPlayer => true;
    public InputData LocalInputData => _inputData;

    #region Unity BuiltIn Methods
    private void Awake()
    {
        _inputData.ResetInput();
    }

    private void Start()
    {
        FindObjectOfType<CameraAutoController>().CenterHook = transform;
    }

    private void Update()
    {
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
