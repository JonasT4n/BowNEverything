using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArrowQuiverElement : IElementInfo
{
    [SerializeField] private string _name;
    [SerializeField] private ArrowTypes _type;
    [SerializeField] private GameRarity _rarity = GameRarity.Common;
    [SerializeField] private ArrowBehaviour _prefab;
    [SerializeField] private Sprite _itemSprite;
    [SerializeField] private int _amountItemOnCollected;
    [SerializeField] private float _drawingTime;
    [SerializeField] private float _degreeAnticipation;

    private bool _isDisposed = false;

    #region Properties
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
