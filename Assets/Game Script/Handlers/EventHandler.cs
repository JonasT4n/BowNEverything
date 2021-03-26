using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameEventArgs
{

}

public static class EventHandler
{
    // Technical Events
    public delegate void WindowResize(Vector2Int prevSize, Vector2Int newSize);

    public static event WindowResize OnWindowResizeEvent;

    // In Game Events
    public delegate void ArrowHitVictim(ArrowHitEventArgs args);
    public delegate void GameStarted(GameStartedEventArgs args);
    public delegate void GameEnded(GameEndedEventArgs args);
    public delegate void PlayerShoot(PlayerShootEventArgs args);
    public delegate void PlayerCollectItem(PlayerCollectItemEventArgs args);
    public delegate void PauseGamePress(PauseGamePressEventArgs args);
    public delegate void PlayerChangeArrow(PlayerChangeArrowEventArgs args);
    public delegate void EntityDeath(EntityDeathEventArgs args);
    public delegate void PlayerRespawn(PlayerRespawnEventArgs args);
    

    public static event ArrowHitVictim OnArrowHitEvent;
    public static event GameStarted OnGameStartedEvent;
    public static event GameEnded OnGameEndedEvent;
    public static event PlayerShoot OnPlayerShootEvent;
    public static event PlayerCollectItem OnPlayerCollectedItemEvent;
    public static event PauseGamePress OnGamePauseEvent;
    public static event PlayerChangeArrow OnPlayerChangeArrowEvent;
    public static event EntityDeath OnEntityDeathEvent;
    public static event PlayerRespawn OnPlayerRespawnEvent;

    public static void CallWindowResizeEvent(Vector2Int prevSize, Vector2Int newSize)
    {
        OnWindowResizeEvent?.Invoke(prevSize, newSize);
    }

    public static void CallEvent(IGameEventArgs ev)
    {
        if (ev is ArrowHitEventArgs)
            OnArrowHitEvent?.Invoke((ArrowHitEventArgs)ev);
        else if (ev is GameStartedEventArgs)
            OnGameStartedEvent?.Invoke((GameStartedEventArgs)ev);
        else if (ev is GameEndedEventArgs)
            OnGameEndedEvent?.Invoke((GameEndedEventArgs)ev);
        else if (ev is PlayerShootEventArgs)
            OnPlayerShootEvent?.Invoke((PlayerShootEventArgs)ev);
        else if (ev is PlayerCollectItemEventArgs)
            OnPlayerCollectedItemEvent?.Invoke((PlayerCollectItemEventArgs)ev);
        else if (ev is PauseGamePressEventArgs)
            OnGamePauseEvent?.Invoke((PauseGamePressEventArgs)ev);
        else if (ev is PlayerChangeArrowEventArgs)
            OnPlayerChangeArrowEvent?.Invoke((PlayerChangeArrowEventArgs)ev);
        else if (ev is EntityDeathEventArgs)
            OnEntityDeathEvent?.Invoke((EntityDeathEventArgs)ev);
        else if (ev is PlayerRespawnEventArgs)
            OnPlayerRespawnEvent?.Invoke((PlayerRespawnEventArgs)ev);
    }
}

public class ArrowHitEventArgs : IGameEventArgs, ICancelAbleAction
{
    private Collider2D _victim;
    private LivingEntity _shooter;
    private ArrowBehaviour _arrow;
    private bool _isCancelled;

    public Collider2D VictimHit => _victim;
    public LivingEntity EntityShooter => _shooter;
    public ArrowBehaviour ArrowHit => _arrow;
    public bool IsCancelled => _isCancelled;

    public ArrowHitEventArgs(LivingEntity shooter, Collider2D victim, ArrowBehaviour arrow)
    {
        _victim = victim;
        _shooter = shooter;
        _arrow = arrow;
    }

    public void CancelAction(bool cancel)
    {
        _isCancelled = cancel;
    }
}

public class GameStartedEventArgs : IGameEventArgs
{
    public GameStartedEventArgs()
    {

    }
}

public class GameEndedEventArgs : IGameEventArgs
{
    public GameEndedEventArgs()
    {

    }
}

public class PlayerShootEventArgs : IGameEventArgs, ICancelAbleAction
{
    private PlayerEntity _player;
    private Vector2 _shootDir;
    private ArrowTypes _type;
    private ArrowBehaviour _arrow;
    private bool _isCancelled;
    private bool _keepQuantity;

    public PlayerEntity Player => _player;
    public Vector2 ShootDirection => _shootDir;
    public ArrowTypes TypeOfArrow => _type;
    public bool IsCancelled => _isCancelled;

    public bool KeepQuantity
    {
        set => _keepQuantity = value;
        get => _keepQuantity;
    }

    public ArrowBehaviour ArrowObject => _arrow;

    public PlayerShootEventArgs(PlayerEntity player, Vector2 shootDir, ArrowBehaviour arrow)
    {
        _player = player;
        _shootDir = shootDir;
        _arrow = arrow;
        _arrow.WhoShot = _player;
        _type = _arrow.TypeOfArrow;
        _isCancelled = false;
        _keepQuantity = false;
    }

    public void CancelAction(bool cancel)
    {
        _isCancelled = cancel;
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
    private SingleGameMode _mode;
    private bool _isPause;

    public SingleGameMode Mode => _mode;
    public bool IsPause => _isPause;

    public PauseGamePressEventArgs(SingleGameMode mode, bool pause)
    {
        _mode = mode;
        _isPause = pause;
    }
}

public class PlayerChangeArrowEventArgs : IGameEventArgs
{
    public PlayerEntity _player;
    public ArrowTypes _changeArrowTo;

    public PlayerEntity Player => _player;
    public ArrowTypes ChangeTo => _changeArrowTo;

    public PlayerChangeArrowEventArgs(PlayerEntity player, ArrowTypes arrowTo)
    {
        _player = player;
        _changeArrowTo = arrowTo;
    }
}

public class EntityDeathEventArgs : IGameEventArgs, ICancelAbleAction
{
    private LivingEntity _entityDied;
    private LivingEntity _whoKill;
    private bool _isCancelled = false;

    public LivingEntity EntityVictim => _entityDied;

    /// <summary>
    /// Who killed the entity, it returns null if not exists.
    /// </summary>
    public LivingEntity WhoKill => _whoKill;
    public bool IsCancelled => _isCancelled;

    public EntityDeathEventArgs(LivingEntity entityDied, LivingEntity whoKill = null)
    {
        _entityDied = entityDied;
        _whoKill = whoKill;
    }

    public void CancelAction(bool cancel)
    {
        _isCancelled = cancel;
    }
}

public class PlayerRespawnEventArgs : IGameEventArgs
{
    public PlayerRespawnEventArgs()
    {

    }
}