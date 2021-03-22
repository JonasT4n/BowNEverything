using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class PlayerEntity : LivingEntity
{
    [Header("Player Informations")]
    [SerializeField] private string _username = "Player";
    [SerializeField] private Transform _bowNeedle = null;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private SpriteRenderer _anticipatorRenderer = null;

    [Space, Header("Additional Events")]
    [SerializeField] private UnityEvent _pullArrowEvent = null;
    [SerializeField] private UnityEvent _releaseArrowEvent = null;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private int _enemyKilled = 0;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private List<ArrowTypes> _haveArrows = new List<ArrowTypes>();

    private Dictionary<ArrowTypes, int> _quiver;
    private int _currentUse = -1;

    public Animator PlayerAnim => _animator;
    public ArrowTypes CurrentUse
    {
        get
        {
            if (_currentUse < 0)
                return ArrowTypes.None;

            return _haveArrows[_currentUse];
        }
    }
    public int EnemyKillCount
    {
        set => _enemyKilled = value;
        get => _enemyKilled;
    }

    #region Unity BuiltIn Methods
    private void OnEnable()
    {
        // Subscribe event
        EventHandler.OnPlayerCollectedItemEvent += FloatingItemCollected;
        EventHandler.OnPlayerChangeArrowEvent += PlayerChangeArrow;

        if (tag == "MainPlayer")
            FindObjectOfType<CameraAutoController>().CenterHook = transform;

        InformationUI.ShowUIFollower(true);
    }

    // Start is called before the first frame update
    private void Start()
    {
        _quiver = new Dictionary<ArrowTypes, int>();
        InformationUI.MaxHealthValue = MaxHealth;
        ResetEntityValues();
    }

    private void OnDisable()
    {
        // Unsubscribe event
        EventHandler.OnPlayerCollectedItemEvent -= FloatingItemCollected;
        EventHandler.OnPlayerChangeArrowEvent -= PlayerChangeArrow;

        CameraAutoController autoCam = FindObjectOfType<CameraAutoController>();
        if (tag == "MainPlayer" && autoCam != null)
            autoCam.CenterHook = null;

        ResetEntityValues();
        InformationUI.ShowUIFollower(false);
    }
    #endregion

    #region Event Methods
    private void FloatingItemCollected(PlayerCollectItemEventArgs args)
    {
        // Check player collected the item
        if (!args.Player.gameObject.Equals(gameObject))
            return;

        // Check arrow collected
        if (args.InfoElement is ArrowQuiverElement)
        {
            ArrowQuiverElement arrowElement = (ArrowQuiverElement)args.InfoElement;
            CollectArrow(arrowElement.Type, arrowElement.CollectAmount);
        }

        // Disable floating item, means that the item has been collected by player
        args.CollectedItem.gameObject.SetActive(false);
    }

    private void PlayerChangeArrow(PlayerChangeArrowEventArgs args)
    {
        if (args.Player.gameObject.Equals(gameObject))
        {
            ArrowQuiverElement e = ObjectManager._instance.GetArrowElement(args.ChangeTo);
            _anticipatorRenderer.sprite = e == null ? null : e.ItemSprite;
            _anticipatorRenderer.transform.localEulerAngles = e == null ? Vector3.zero : new Vector3(0, 0, e.OffsetDegreeAnticipation);
        }
    }
    #endregion

    #region Overriden Methods
    public override void AddHealth(int h)
    {
        CurrentHealth += h;
        CurrentHealth = MaxHealth < CurrentHealth ? MaxHealth : (CurrentHealth < 0) ? 0 : CurrentHealth;
        InformationUI.HealthValue = CurrentHealth;
    }

    public override void AddForce(Vector2 forceDir, ForceMode2D force)
    {
        switch (force)
        {
            case ForceMode2D.Force:
                CurrentVelocity += forceDir * Time.deltaTime;
                break;

            case ForceMode2D.Impulse:
                CurrentVelocity = forceDir;
                break;
        }
    }

    public override void AddEffects(EntityEffects effect, float value, bool temporary = true)
    {
        if (temporary)
            StartCoroutine(TemporaryEffect(effect, -value, 3f));
    }

    public override void ResetEntityValues()
    {
        CurrentHealth = MaxHealth;

        InformationUI.HealthValue = CurrentHealth;
        InformationUI.DrawingTimeValue = 0;
    }

    protected override IEnumerator TemporaryEffect(EntityEffects effect, float negativeValue, float seconds)
    {
        float tempTime = seconds;
        while (tempTime > 0)
        {
            tempTime -= Time.deltaTime;
            yield return null;
        }

        AddEffects(effect, negativeValue);
    }
    #endregion

    public void BowShoot(Vector2 shootDir)
    {
        bool isShoot = false;
        switch (CurrentUse)
        {
            case ArrowTypes.ChopStick:
                float offsetDegree = 15f;
                float degreeChopstick = Mathf.Atan(shootDir.y / shootDir.x) * Mathf.Rad2Deg;
                isShoot = Shoot(CurrentUse, RadianToVector2((degreeChopstick + offsetDegree + (shootDir.x < 0 ? 180 : 0)) * Mathf.Deg2Rad));
                bool isSecondShoot = Shoot(CurrentUse, RadianToVector2((degreeChopstick - offsetDegree + (shootDir.x < 0 ? 180 : 0)) * Mathf.Deg2Rad));
                isShoot = isShoot || isSecondShoot;
                break;

            default:
                isShoot = Shoot(ArrowTypes.Normal, shootDir);
                break;
        }

        if (isShoot)
            _releaseArrowEvent?.Invoke();
    }

    /// <summary>
    /// Check whether the things is being shoot
    /// </summary>
    /// <param name="type">Type arrow shoot</param>
    /// <param name="shootDir">Direction shoot</param>
    /// <returns>true if successfully shoot</returns>
    private bool Shoot(ArrowTypes type, Vector2 shootDir)
    {
        ArrowBehaviour arrow = ObjectManager._instance.ArrowMaker.GetObjectRequired(CurrentUse);
        if (!arrow.gameObject.activeSelf)
            arrow.gameObject.SetActive(true);

        // Call event first then shoot
        PlayerShootEventArgs arg = new PlayerShootEventArgs(this, shootDir, arrow);
        EventHandler.CallEvent(arg);

        if (!arg.IsCancelled)
            arrow.Shoot(_bowNeedle == null ? transform.position : _bowNeedle.position, arg.ShootDirection);

        int leftOver;
        if (_quiver.TryGetValue(CurrentUse, out leftOver))
            if (!arg.KeepQuantity)
                CollectArrow(CurrentUse, -1);

        return !arg.IsCancelled;
    }

    public void CallPullArrowEvent()
    {
        _pullArrowEvent?.Invoke();
    }

    /// <summary>
    /// Arrow roller to choose which arrow will be use.
    /// </summary>
    public void NextArrowUse()
    {
        ArrowTypes temp = CurrentUse;
        if (_currentUse < 0)
            return;

        _currentUse = _currentUse + 1 >= _haveArrows.Count ? 0 : _currentUse + 1;
        EventHandler.CallEvent(new PlayerChangeArrowEventArgs(this, CurrentUse));
    }

    public void CollectArrow(ArrowTypes type, int amount)
    {
        ArrowTypes temp = CurrentUse;

        // Find and add arrow by type with amount
        if (!_quiver.ContainsKey(type))
        {
            _haveArrows.Add(type);
            _quiver.Add(type, amount);
        }
        else
        {
            _quiver[type] += amount;
        }

        // Check if have no more arrow left of this type of arrow
        if (_quiver[type] <= 0)
        {
            _haveArrows.Remove(type);
            _quiver.Remove(type);
        }

        // Check current usable arrow
        if (_haveArrows.Count <= 0)
        {
            _currentUse = -1;
        }
        else
        {
            if (_currentUse < 0)
                _currentUse = 0;
            else if (_currentUse >= _haveArrows.Count)
                _currentUse = _haveArrows.Count - 1;
        }

        if (CurrentUse != temp)
            EventHandler.CallEvent(new PlayerChangeArrowEventArgs(this, CurrentUse));
    }

    #region Static Utility Methods
    private static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    #endregion
}
