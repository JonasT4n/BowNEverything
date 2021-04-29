using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNEGame
{
    [System.Serializable]
    public class ArrowQuiverElement : IElementItemInfo
    {
        [SerializeField] private string _name = "";
        [SerializeField] private ArrowTypes _type = ArrowTypes.None;
        [SerializeField] private GameRarity _rarity = GameRarity.Common;
        [SerializeField] private ArrowBehaviour _prefab = null;
        [SerializeField] private Sprite _itemSprite = null;
        [SerializeField] private int _amountItemOnCollected = 0;
        [SerializeField] private float _drawingTime = 1f;
        [SerializeField] private float _degreeAnticipation = 0;

        private bool _isDisposed = false;

        #region Properties
        public string Name => _name;
        public ArrowBehaviour ItemPrefab => _prefab;
        public Sprite ItemSprite => _itemSprite;
        public float OffsetDegreeAnticipation => _degreeAnticipation;

        public ArrowTypes Type
        {
            set => _type = value;
            get => _type;
        }

        public int CollectAmount
        {
            set => _amountItemOnCollected = value;
            get => _amountItemOnCollected;
        }

        public float DrawingTime
        {
            set => _drawingTime = value;
            get => _drawingTime;
        }
        #endregion

        #region Methods
        public void Dispose() => Dispose(true);

        public void Dispose(bool dispose)
        {
            if (!_isDisposed)
                return;

            System.GC.SuppressFinalize(this);
            _isDisposed = dispose;
        }

        // Make equalizer less restricted
        public override bool Equals(object obj)
        {
            if (obj is ArrowQuiverElement)
            {
                ArrowQuiverElement a = (ArrowQuiverElement)obj;
                if (a.Type == _type)
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

}
