using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameAIAutoController))]
public class AIEntity : LivingEntity
{
    [Header("Enemy Information Attributes")]
    [SerializeField] private EnemyType _type = EnemyType.Dummy;

    public EnemyType TypeOfEnemy => _type;

    #region Unity BuiltIn Methods
    // Start is called before the first frame update
    private void Start()
    {
        InformationUI.MaxHealthValue = MaxHealth;
        ResetEntityValues();
    }
    #endregion

    #region Overriden Methods
    public override void AddHealth(int h)
    {
        CurrentHealth += h;
        CurrentHealth = MaxHealth < CurrentHealth ? MaxHealth : (CurrentHealth < 0) ? 0 : CurrentHealth;
        InformationUI.HealthValue = CurrentHealth;
    }

    public override void ResetEntityValues()
    {
        CurrentHealth = MaxHealth;

        InformationUI.HealthValue = CurrentHealth;
        InformationUI.DrawingTimeValue = 0;
    }
    #endregion

}
