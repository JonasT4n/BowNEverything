using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNEGame
{
    [RequireComponent(typeof(GameAIAutoController))]
    public class EnemyEntity : LivingEntity
    {
        [Header("Enemy Information Attributes")]
        [SerializeField] private EnemyType _type = EnemyType.Dummy;

        private Queue<EnemyEntity> _poolReference;

        public Queue<EnemyEntity> PoolReference { set => _poolReference = value; }
        public EnemyType TypeOfEnemy => _type;

        #region Unity BuiltIn Methods
        private void OnEnable()
        {
            InformationUI.ShowUIFollower(true);
            ResetEntityValues();
        }

        private void OnDisable()
        {
            InformationUI.ShowUIFollower(false);
            if (_poolReference != null)
                _poolReference.Enqueue(this);
        }
        #endregion

        #region Overriden Methods
        public override void AddHealth(int h)
        {
            CurrentHealth += h;
            CurrentHealth = MaxHealth < CurrentHealth ? MaxHealth : (CurrentHealth < 0) ? 0 : CurrentHealth;
            InformationUI.HealthValue = CurrentHealth;
        }

        public override void AddEffects(EntityEffects effect, float value, bool temporary = true)
        {
            if (temporary)
                StartCoroutine(TemporaryEffectRoutine(effect, -value, 3f));
        }

        public override void ResetEntityValues()
        {
            CurrentHealth = MaxHealth;

            InformationUI.HealthValue = CurrentHealth;
            InformationUI.DrawingTimeValue = 0;
        }

        protected override IEnumerator TemporaryEffectRoutine(EntityEffects effect, float negativeValue, float seconds)
        {
            float tempTime = seconds;
            while (tempTime > 0)
            {
                tempTime -= Time.deltaTime;
                yield return null;
            }

            AddEffects(effect, negativeValue);
        }
        #endregion

        public void Attack(Transform entityTarget)
        {
            Vector3 attackDir = (entityTarget.position - transform.position).normalized;
            switch (_type)
            {
                case EnemyType.Caster:
                    ArrowBehaviour arr = ObjectManager._instance.ArrowMaker.GetObjectRequired(ArrowTypes.DarkCasterAmmo);
                    if (arr != null)
                    {
                        if (!arr.gameObject.activeSelf)
                            arr.gameObject.SetActive(true);
                        arr.WhoShot = this;
                        arr.Shoot(transform.position, attackDir);
                    }
                    break;

                default:
                    LivingEntity target = entityTarget.GetComponent<LivingEntity>();
                    if (target != null)
                        target.AddHealth(-1);

                    if (target.CurrentHealth <= 0)
                    {
                        if (target is PlayerEntity)
                            ((PlayerEntity)target).EnemyKillCount++;

                        EntityDeathEventArgs deathArg = new EntityDeathEventArgs(target, this);
                        EventHandler.CallEvent(deathArg);

                        if (!deathArg.IsCancelled)
                            gameObject.SetActive(false);
                    }
                    break;
            }
        }
    }
}