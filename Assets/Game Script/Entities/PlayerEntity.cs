using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class PlayerEntity : LivingEntity
{
    [Header("Player Informations")]
    [SerializeField] private string _username = "Player";
    [SerializeField] private Transform _bowNeedle = null;
    [SerializeField] private Animator _animator = null;
    [SerializeField] private SpriteRenderer _anticipatorRenderer = null;

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

    #region Unity BuiltIn Methods
    private void OnEnable()
    {
        // Subscribe event
        EventHandler.OnPlayerCollectedItemEvent += FloatingItemCollected;

        if (tag == "MainPlayer")
            FindObjectOfType<CameraAutoController>().CenterHook = transform;
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

        CameraAutoController autoCam = FindObjectOfType<CameraAutoController>();
        if (tag == "MainPlayer" && autoCam != null)
            autoCam.CenterHook = null;
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
            Debug.Log($"Collected: {_quiver[arrowElement.Type]}");
        }

        // Disable floating item, means that the item has been collected by player
        args.CollectedItem.gameObject.SetActive(false);
    }
    #endregion

    #region Overriden Methods
    public override void AddHealth(int h)
    {
        CurrentHealth += h;
        CurrentHealth = MaxHealth < CurrentHealth ? MaxHealth : (CurrentHealth < 0) ? 0 : CurrentHealth;
        InformationUI.HealthValue = CurrentHealth;
    }

    public override void ResetEntityValues()
    {
        CurrentHealth = MaxHealth;

        InformationUI.HealthValue = CurrentHealth;
        InformationUI.DrawingTimeValue = 0;
    }
    #endregion

    public void BowShoot(Vector2 shootDir)
    {
        ArrowBehaviour arrow = ObjectManager.ArrowMaker.GetObjectRequired(CurrentUse);
        if (!arrow.gameObject.activeSelf)
            arrow.gameObject.SetActive(true);

        // Call event first then shoot
        PlayerShootEventArgs arg = new PlayerShootEventArgs(this, shootDir, arrow);
        EventHandler.CallEvent(arg);

        arrow.Shoot(_bowNeedle == null ? transform.position : _bowNeedle.position, arg.ShootDirection);

        int leftOver;
        if (_quiver.TryGetValue(CurrentUse, out leftOver))
            CollectArrow(CurrentUse, -1);
    }

    /// <summary>
    /// Arrow roller to choose which arrow will be use.
    /// </summary>
    public void NextArrowUse()
    {
        if (_currentUse < 0)
            return;

        _currentUse = _currentUse + 1 >= _haveArrows.Count ? 0 : _currentUse + 1;
        ChangeAnticipatorSpriteRenderer(ObjectManager.GetArrowElement(_haveArrows[_currentUse]));
    }

    public void CollectArrow(ArrowTypes type, int amount)
    {
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
            ChangeAnticipatorSpriteRenderer(null);
        }
        else
        {
            if (_currentUse < 0)
                _currentUse = 0;
            else if (_currentUse >= _haveArrows.Count)
                _currentUse = _haveArrows.Count - 1;
            ChangeAnticipatorSpriteRenderer(ObjectManager.GetArrowElement(_haveArrows[_currentUse]));
        }
    }

    private void ChangeAnticipatorSpriteRenderer(ArrowQuiverElement e)
    {
        _anticipatorRenderer.sprite = e == null ? null : e.ItemSprite;
        _anticipatorRenderer.transform.localEulerAngles = e == null ? Vector3.zero : new Vector3(0, 0, e.OffsetDegreeAnticipation);
    }
}
