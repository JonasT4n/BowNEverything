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
    public ArrowHitEventArgs()
    {
        
    }
}

public class PlayerShootEventArgs : IGameEventArgs
{
    private PlayerEntity _player;
    private Vector2 _shootDir;
    private ArrowTypes _type;

    public PlayerEntity Player => _player;
    public Vector2 ShootDirection => _shootDir;
    public ArrowTypes TypeOfArrow => _type;

    public PlayerShootEventArgs(PlayerEntity player, Vector2 shootDir, ArrowTypes type)
    {
        _player = player;
        _shootDir = shootDir;
        _type = type;
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
    private GameMode _mode;
    private bool _isPause;

    public GameMode Mode => _mode;
    public bool IsPause => _isPause;

    public PauseGamePressEventArgs(GameMode mode, bool pause)
    {
        _mode = mode;
        _isPause = pause;
    }
}