using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class PlayerEntity : LivingEntity
{
    [Header("Player Informations")]
    [SerializeField] private string _username = "Player";
    [SerializeField] private Transform _bowNeedle = null;
    [SerializeField] private Animator _animator = null;

    private Dictionary<ArrowTypes, int> _quiver;

    public Animator PlayerAnim => _animator;

    #region Unity BuiltIn Methods
    private void OnEnable()
    {
        // Subscribe event
        EventHandler.OnPlayerCollectedItemEvent += FloatingItemCollected;
        EventHandler.OnPlayerShootEvent += BowShoot;
    }

    // Start is called before the first frame update
    private void Start()
    {
        _quiver = new Dictionary<ArrowTypes, int>();
    }

    private void OnDisable()
    {
        // Unsubscribe event
        EventHandler.OnPlayerCollectedItemEvent += FloatingItemCollected;
        EventHandler.OnPlayerShootEvent -= BowShoot;
    }
    #endregion

    public void CollectArrow(ArrowTypes type, int amount)
    {
        // Find and add arrow by type with amount
        if (!_quiver.ContainsKey(type))
            _quiver.Add(type, amount);
        else
            _quiver[type] += amount;

        // Check if have no more arrow left of this type of arrow
        if (_quiver[type] <= 0)
            _quiver.Remove(type);
    }

    private void BowShoot(PlayerShootEventArgs args)
    {
        if (Equals(args.Player))
        {
            int leftOver;
            if (_quiver.TryGetValue(args.TypeOfArrow, out leftOver))
            {
                CollectArrow(args.TypeOfArrow, -1);
                ArrowBehaviour arrow = ObjectManager.ArrowMaker.GetObjectRequired(args.TypeOfArrow);
                arrow.Who = this;
                arrow.Shoot(_bowNeedle.position, args.ShootDirection);
            }
        }
    }

    private void FloatingItemCollected(PlayerCollectItemEventArgs args)
    {
        // Check player collected the item
        if (!args.Player.Equals(this))
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
}
