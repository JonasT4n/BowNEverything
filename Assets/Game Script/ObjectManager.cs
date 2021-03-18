using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


public class ObjectManager : MonoBehaviour
{
    public static ObjectManager _instance;

    private static ArrowFactory _arrowFactory;
    private static EnemyFactory _enemyFactory;

    private static Dictionary<ArrowTypes, ArrowQuiverElement> _kindsOfArrow = new Dictionary<ArrowTypes, ArrowQuiverElement>();

    [Header("Object Elements")]
    [SerializeField] private ArrowQuiverElement[] _arrows = null;

    #region Properties
    public static ArrowFactory ArrowMaker => _arrowFactory;
    public static EnemyFactory EnemtMaker => _enemyFactory;
    #endregion

    #region Unity BuiltIn Methods
    private void Awake()
    {
        // Make class singleton
        if (_instance)
        {
            Debug.Log($"Deleted multiple object of singleton behaviour: {name}");
            Destroy(this);
        }
        else
        {
            _instance = this;
        }

        // Init objects to be pulled, in this case every kind of arrows
        Dictionary<ArrowTypes, ArrowBehaviour> a = new Dictionary<ArrowTypes, ArrowBehaviour>();
        foreach (ArrowQuiverElement e in _arrows)
        {
            if (a.ContainsKey(e.Type))
                continue;

            if (e.ArrowPrefab.gameObject.activeSelf)
                e.ArrowPrefab.gameObject.SetActive(false);

            a.Add(e.Type, e.ArrowPrefab);
            _kindsOfArrow.Add(e.Type, e);
        }

        _arrowFactory = new ArrowFactory(a, transform);
    }
    #endregion

    public static ArrowQuiverElement GetArrowElement(ArrowTypes type)
    {
        return _kindsOfArrow[type];
    }
}
