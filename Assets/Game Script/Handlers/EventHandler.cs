using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNEGame
{
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
        public delegate void ItemSpawned(ItemSpawnedEventArgs args);
        public delegate void MsgSent(MsgSentEventArgs args);


        public static event ArrowHitVictim OnArrowHitEvent;
        public static event GameStarted OnGameStartedEvent;
        public static event GameEnded OnGameEndedEvent;
        public static event PlayerShoot OnPlayerShootEvent;
        public static event PlayerCollectItem OnPlayerCollectedItemEvent;
        public static event PauseGamePress OnGamePauseEvent;
        public static event PlayerChangeArrow OnPlayerChangeArrowEvent;
        public static event EntityDeath OnEntityDeathEvent;
        public static event PlayerRespawn OnPlayerRespawnEvent;
        public static event ItemSpawned OnItemSpawnedEvent;
        public static event MsgSent OnMsgSentEvent;

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
            else if (ev is ItemSpawnedEventArgs)
                OnItemSpawnedEvent?.Invoke((ItemSpawnedEventArgs)ev);
            else if (ev is MsgSentEventArgs)
                OnMsgSentEvent?.Invoke((MsgSentEventArgs)ev);
        }
    }

    public class ArrowHitEventArgs : IGameEventArgs, ICancelAbleAction
    {
        private Collider2D victim;
        private LivingEntity shooter;
        private ArrowBehaviour arrow;
        private bool isCancelled;

        public Collider2D VictimHit => victim;
        public LivingEntity EntityShooter => shooter;
        public ArrowBehaviour ArrowHit => arrow;
        public bool IsCancelled => isCancelled;

        public ArrowHitEventArgs(LivingEntity shooter, Collider2D victim, ArrowBehaviour arrow)
        {
            this.victim = victim;
            this.shooter = shooter;
            this.arrow = arrow;
        }

        public void CancelAction(bool cancel)
        {
            isCancelled = cancel;
        }
    }

    public class GameStartedEventArgs : IGameEventArgs
    {
        private BNEGameState oldState;
        private BNEGameState nextState;

        public BNEGameState PrevState => oldState;
        public BNEGameState NextState => nextState;

        public GameStartedEventArgs(BNEGameState oldState, BNEGameState nextState)
        {
            this.oldState = oldState;
            this.nextState = nextState;
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
        private PlayerEntity player;
        private Vector2 shootDir;
        private ArrowTypes typeArrow;
        private ArrowBehaviour arrow;
        private bool isCancelled;

        public PlayerEntity Player => player;
        public Vector2 ShootDirection => shootDir;
        public ArrowTypes TypeArrow => typeArrow;
        public bool IsCancelled => isCancelled;

        public ArrowBehaviour ArrowObject => arrow;

        public PlayerShootEventArgs(PlayerEntity player, Vector2 shootDir, ArrowBehaviour arrow)
        {
            this.player = player;
            this.shootDir = shootDir;
            this.arrow = arrow;
            this.arrow.WhoShot = this.player;
            typeArrow = this.arrow.TypeOfArrow;
            isCancelled = false;
        }

        public void CancelAction(bool cancel)
        {
            isCancelled = cancel;
        }
    }

    public class PlayerCollectItemEventArgs : IGameEventArgs, ICancelAbleAction
    {
        private int indexCollected;
        private PlayerEntity player;
        private FloatingItemBehaviour collectedItem;
        private bool _isCancelled = false;

        public PlayerEntity Player => player;
        public int IndexCollected => indexCollected;
        public FloatingItemBehaviour CollectedItem => collectedItem;
        public IElementItemInfo InfoElement => collectedItem.ItemInfo;

        public bool IsCancelled => _isCancelled;

        public PlayerCollectItemEventArgs(int indexCollected, PlayerEntity player, FloatingItemBehaviour collectedItem)
        {
            this.indexCollected = indexCollected;
            this.player = player;
            this.collectedItem = collectedItem;
        }

        public void CancelAction(bool cancel)
        {
            _isCancelled = cancel;
        }
    }

    public class PauseGamePressEventArgs : IGameEventArgs
    {
        private bool isPause;

        public bool IsPause => isPause;

        public PauseGamePressEventArgs(bool isPause)
        {
            this.isPause = isPause;
        }
    }

    public class PlayerChangeArrowEventArgs : IGameEventArgs
    {
        public PlayerEntity player;
        public ArrowTypes changeArrowTo;

        public PlayerEntity Player => player;
        public ArrowTypes ChangeTo => changeArrowTo;

        public PlayerChangeArrowEventArgs(PlayerEntity player, ArrowTypes arrowTo)
        {
            this.player = player;
            changeArrowTo = arrowTo;
        }
    }

    public class EntityDeathEventArgs : IGameEventArgs, ICancelAbleAction
    {
        private LivingEntity victim;
        private LivingEntity whoKill;
        private bool _isCancelled = false;

        public LivingEntity EntityVictim => victim;

        /// <summary>
        /// Who killed the entity, it returns null if not exists.
        /// </summary>
        public LivingEntity WhoKill => whoKill;
        public bool IsCancelled => _isCancelled;

        public EntityDeathEventArgs(LivingEntity victim, LivingEntity whoKill = null)
        {
            this.victim = victim;
            this.whoKill = whoKill;
        }

        public void CancelAction(bool cancel)
        {
            _isCancelled = cancel;
        }
    }

    public class PlayerRespawnEventArgs : IGameEventArgs, ICancelAbleAction
    {
        private PlayerEntity player;
        private bool isCancelled = false;

        public PlayerEntity Player => player;
        public bool IsCancelled => isCancelled;

        public PlayerRespawnEventArgs(PlayerEntity player)
        {
            this.player = player;
        }

        public void CancelAction(bool cancel)
        {
            isCancelled = cancel;
        }
    }

    public class ItemSpawnedEventArgs : IGameEventArgs, ICancelAbleAction
    {
        private int indexSpawn;
        private IElementItemInfo spawnedItemInfo;
        private bool _isCancelled = false;

        public IElementItemInfo SpawnedItemInfo => spawnedItemInfo;
        public int IndexSpawned => indexSpawn;
        public bool IsCancelled => _isCancelled;

        public ItemSpawnedEventArgs(int indexSpawn, IElementItemInfo spawnedItemInfo)
        {
            this.indexSpawn = indexSpawn;
            this.spawnedItemInfo = spawnedItemInfo;
        }

        public void CancelAction(bool cancel)
        {
            _isCancelled = cancel;
        }
    }

    public class MsgSentEventArgs : IGameEventArgs, ICancelAbleAction
    {
        private string msg;
        private Color color;
        private bool isCancelled = false;

        public string Message => msg;
        public Color MsgColor => color;
        public bool IsCancelled => isCancelled;

        public MsgSentEventArgs(string msg, Color color)
        {
            this.msg = msg;
            this.color = color;
        }

        public void SetMessage(string msg)
        {
            this.msg = msg;
        }

        public void SetColor(Color color)
        {
            this.color = color;
        }

        public void CancelAction(bool cancel)
        {
            isCancelled = cancel;
        }
    }
}
