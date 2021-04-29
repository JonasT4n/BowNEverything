using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace BNEGame.Network
{
    public class ItemSpawnerInfoParser : NetworkBehaviour
    {
        struct CurrentOnItemSpawn
        {
            public int type;
            public bool wasActive;
        }

        [SerializeField] private ItemSpawnerManager spawner = null;
        [SerializeField] private ServerNonAuthParser serverParser = null;

        private SyncDictionary<int, CurrentOnItemSpawn> onFloatCurrent = new SyncDictionary<int, CurrentOnItemSpawn>();

        public ItemSpawnerManager ItemSpawner => spawner;

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Subscribe Event
            EventHandler.OnItemSpawnedEvent += HandleSpawnOrigin;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Subscribe network event
            onFloatCurrent.Callback += OnValueInitCallback;

            // Initialize sync with server
            InitSpawnedItems();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            // Unsubscribe network event
            onFloatCurrent.Callback -= OnValueInitCallback;

            // Clearing floating items
            spawner.ClearSpawner();
        }

        private void OnDestroy()
        {
            // Unsubscribe Event
            EventHandler.OnItemSpawnedEvent -= HandleSpawnOrigin;
        }
        #endregion

        #region Event Methods
        private void OnValueInitCallback(SyncIDictionary<int, CurrentOnItemSpawn>.Operation op, int key, CurrentOnItemSpawn item)
        {

        }

        private void HandleSpawnOrigin(ItemSpawnedEventArgs args)
        {
            if (!isServer)
            {
                spawner.SetRunning(false);
                args.CancelAction(true);
                return;
            }

            if (args.SpawnedItemInfo is ArrowQuiverElement)
            {
                ArrowQuiverElement arrowElement = (ArrowQuiverElement)args.SpawnedItemInfo;

                CurrentOnItemSpawn newInfoSpawn = new CurrentOnItemSpawn()
                {
                    type = (int)arrowElement.Type,
                    wasActive = true
                };

                // Update synchronously for new client when join
                if (!onFloatCurrent.ContainsKey(args.IndexSpawned))
                    onFloatCurrent.Add(args.IndexSpawned, newInfoSpawn);
                else
                    onFloatCurrent[args.IndexSpawned] = newInfoSpawn;

                RpcSpawnArrowSync(args.IndexSpawned, (int)arrowElement.Type);
            }
        }
        #endregion

        public void InitSpawnedItems()
        {
            foreach (int keyIndex in onFloatCurrent.Keys)
            {
                FloatingItemBehaviour fItem = spawner.GetSpawnerPoint(keyIndex);
                if (fItem == null)
                    continue;

                CurrentOnItemSpawn curSp = onFloatCurrent[keyIndex];

                //Debug.Log($"Sync on index: {keyIndex}");

                // TODO: not always arrow spawned

                fItem.ItemInfo = ObjectManager._instance.GetArrowElement((ArrowTypes)curSp.type);
                fItem.gameObject.SetActive(curSp.wasActive);
            }
        }

        public void SyncItemOnSpawnerCollected(int floatingItemIndex)
        {
            CurrentOnItemSpawn fItem;
            if (onFloatCurrent.TryGetValue(floatingItemIndex, out fItem))
                fItem.wasActive = false;

            spawner.GetSpawnerPoint(floatingItemIndex)?.gameObject.SetActive(false);
        }

        [Server]
        public void SrvItemCollectedOrigin(PlayerCollectItemEventArgs args)
        {
            CurrentOnItemSpawn i = onFloatCurrent[args.IndexCollected];
            onFloatCurrent[args.IndexCollected] = new CurrentOnItemSpawn() { type = i.type, wasActive = false };
        }

        [ClientRpc]
        private void RpcSpawnArrowSync(int indexSpawned, int arrowTypeEnumIndex)
        {
            ArrowTypes type = (ArrowTypes)arrowTypeEnumIndex;
            FloatingItemBehaviour floatingItem = spawner.GetSpawnerPoint(indexSpawned);

            if (floatingItem == null)
                return;

            floatingItem.ItemInfo = ObjectManager._instance.GetArrowElement(type);
            floatingItem.gameObject.SetActive(true);
        }
    }
}