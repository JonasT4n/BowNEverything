using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class AutoArrowShooter : MonoBehaviour
{
    [Header("Auto Arrow Shooter Set Up Attributes")]
    [SerializeField] private ArrowTypes _typeShoot = ArrowTypes.None;
    [SerializeField] private float _interval = 1f;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float _timeIntervalHandler = 0f;

    #region Unity BuiltIn Methods
    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        
    }
    #endregion
}
