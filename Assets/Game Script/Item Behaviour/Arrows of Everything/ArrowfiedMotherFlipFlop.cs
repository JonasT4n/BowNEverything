using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNEGame
{
    public class ArrowfiedMotherFlipFlop : NormalArrow, IBounceAble
    {
        [Header("Additional Info for Mother Flip-Flop")]
        [SerializeField] private float _rotateSpeedPerFrame = 4f;
        [SerializeField] private int _bounceAliveAmount = 3;

        public int MaxBounceCounter
        {
            set => _bounceAliveAmount = value;
            get => _bounceAliveAmount;
        }

        protected override void OnHitEffect(Collider2D collision)
        {
            ArrowHitEventArgs arg = new ArrowHitEventArgs(WhoShot, collision, this);
            EventHandler.CallEvent(arg);

            Vector3 vel = ArrowRigid.velocity;
            if (!arg.IsCancelled)
            {
                LivingEntity hit = collision.GetComponent<LivingEntity>();
                if (hit != null)
                {
                    hit.AddHealth(Damage);

                    // Knockback
                    if (hit.CurrentHealth > 0)
                    {
                        Vector3 hitDir = vel.normalized;
                        hit.EntityR2D.AddForce(new Vector2(hitDir.x < 0 ? -50 : 50, Mathf.Abs(hitDir.y) * 2), ForceMode2D.Impulse);
                    }

                    // Check entity killed
                    else if (hit.CurrentHealth <= 0)
                    {
                        if (WhoShot is PlayerEntity)
                            ((PlayerEntity)WhoShot).EnemyKillCount++;

                        EntityDeathEventArgs deathArg = new EntityDeathEventArgs(hit, WhoShot);
                        EventHandler.CallEvent(deathArg);

                        if (!deathArg.IsCancelled)
                            gameObject.SetActive(false);
                    }
                }
            }

            // Automatically bounce no matter event is cancelled
            Bounce(vel, collision);

            if (_bounceAliveAmount <= 0)
            {
                IsAlreadyHit = true;
                ArrowRigid.isKinematic = true;
                ArrowRigid.velocity = Vector2.zero;
                ArrowAnimator.SetBool("Is Landed", true);

                if (gameObject.activeSelf)
                    StartCoroutine(DestroyTimer());
            }
        }

        protected override void HandleOnAirRotation(Vector2 velocity)
        {
            transform.eulerAngles += new Vector3(0, 0, _bounceAliveAmount % 2 == 0 ? _rotateSpeedPerFrame : -_rotateSpeedPerFrame);
        }

        public void Bounce(Vector3 currentMoveDir, Collider2D hit)
        {
            _bounceAliveAmount--;
            ArrowRigid.AddForce(-currentMoveDir * 1.75f, ForceMode2D.Impulse);
        }

        #region Static Utility Methods
        private static Vector2 RadianToVector2(float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }
        #endregion
    }
}