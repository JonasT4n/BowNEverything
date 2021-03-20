using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameEventArgs
{

}

public static class EventHandler
{
    public delegate void ArrowHit(ArrowHitEventArgs args);
    public delegate void PlayerShoot(PlayerShootEventArgs args);
    public delegate void PlayerCollectItem(PlayerCollectItemEventArgs args);
    public delegate void PauseGamePress(PauseGamePressEventArgs args);

    public static event ArrowHit OnArrowHitEvent;
    public static event PlayerShoot OnPlayerShootEvent;
    public static event PlayerCollectItem OnPlayerCollectedItemEvent;
    public static event PauseGamePress OnGamePauseEvent;

    public static void CallEvent(IGameEventArgs ev)
    {
        if (ev is ArrowHitEventArgs)
            OnArrowHitEvent?.Invoke((ArrowHitEventArgs)ev);
        else if (ev is PlayerShootEventArgs)
            OnPlayerShootEvent?.Invoke((PlayerShootEventArgs)ev);
        else if (ev is PlayerCollectItemEventArgs)
            OnPlayerCollectedItemEvent?.Invoke((PlayerCollectItemEventArgs)ev);
        else if (ev is PauseGamePressEventArgs)
            OnGamePauseEvent?.Invoke((PauseGamePressEventArgs)ev);
    }
}

public class ArrowHitEventArgs : IGameEventArgs
{
    private Collider2D _victim;
    private LivingEntity _shooter;
    private ArrowBehaviour _arrow;

    public Collider2D VictimHit => _victim;
    public LivingEntity EntityShooter => _shooter;
    public ArrowBehaviour ArrowHit => _arrow;

    public ArrowHitEventArgs(LivingEntity shooter, Collider2D victim, ArrowBehaviour arrow)
    {
        _victim = victim;
        _shooter = shooter;
        _arrow = arrow;
    }
}

public class PlayerShootEventArgs : IGameEventArgs
{
    private PlayerEntity _player;
    private Vector2 _shootDir;
    private ArrowTypes _type;
    private ArrowBehaviour _arrow;

    public PlayerEntity Player => _player;
    public Vector2 ShootDirection => _shootDir;
    public ArrowTypes TypeOfArrow => _type;

    public ArrowBehaviour ArrowObject => _arrow;

    public PlayerShootEventArgs(PlayerEntity player, Vector2 shootDir, ArrowBehaviour arrow)
    {
        _player = player;
        _shootDir = shootDir;
        _arrow = arrow;
        _arrow.Who = _player;
        _type = _arrow.TypeOfArrow;
    }
}

public class PlayerCollectItemEventArgs : IGameEventArgs
{
    private PlayerEntity _player;
    private FloatingItemBehaviour _collectedItem;

    public PlayerEntity Player => _player;
    public FloatingItemBehaviour CollectedItem => _collectedItem;
    public IElementInfo InfoElement => _collectedItem.ItemInfo;

    public PlayerCollectItemEventArgs(PlayerEntity player, FloatingItemBehaviour floatingItem)
    {
        _player = player;
        _collectedItem = floatingItem;
    }
}

public class PauseGamePressEventArgs : IGameEventArgs
{
    private GameModeState _mode;
    private bool _isPause;

    public GameModeState Mode => _mode;
    public bool IsPause => _isPause;

    public PauseGamePressEventArgs(GameModeState mode, bool pause)
    {
        _mode = mode;
        _isPause = pause;
    }
}