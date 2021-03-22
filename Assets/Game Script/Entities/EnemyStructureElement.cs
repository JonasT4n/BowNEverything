using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyStructureElement : IElementInfo
{
    [SerializeField] private string _name = "";
    [SerializeField] private EnemyType _type = EnemyType.Dummy;
    [SerializeField] private EnemyEntity _prefab = null;

    private bool _isDisposed = false;

    public string Name => _name;
    public EnemyType Type => _type;
    public EnemyEntity EnemyPrefab => _prefab;


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
        if (obj is EnemyStructureElement)
        {
            EnemyStructureElement a = (EnemyStructureElement)obj;
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
