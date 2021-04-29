using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNEGame
{
    public class ArrowfiedChopStick : NormalArrow
    {
        private const float BACK_TO_PARENT_TIME = 2f;

        protected override void OnHitEffect(Collider2D collision)
        {
            ArrowHitEventArgs arg = new ArrowHitEventArgs(WhoShot, collision, this);
            EventHandler.CallEvent(arg);

            if (!arg.IsCancelled)
            {
                IsAlreadyHit = true;
                ArrowRigid.isKinematic = true;
                ArrowRigid.velocity = Vector2.zero;
                ArrowAnimator.SetBool("Is Landed", true);

                LivingEntity hit = collision.GetComponent<LivingEntity>();
                if (hit != null)
                {
                    hit.AddHealth(Damage);
                    transform.parent = hit.transform;
                    _poolRef.Enqueue(this);

                    // Sticky chopstick stick on victim
                    if (gameObject.activeSelf && hit.gameObject.activeSelf)
                    {
                        StartCoroutine(GoBackToObjManager());
                    }
                    else
                    {
                        transform.parent = ObjectManager._instance.gameObject.transform;
                        gameObject.SetActive(false);
                    }

                    // Check entity killed
                    if (hit.CurrentHealth <= 0)
                    {
                        if (WhoShot is PlayerEntity)
                            ((PlayerEntity)WhoShot).EnemyKillCount++;

                        EntityDeathEventArgs deathArg = new EntityDeathEventArgs(hit, WhoShot);
                        EventHandler.CallEvent(deathArg);

                        if (!deathArg.IsCancelled)
                            gameObject.SetActive(false);
                    }

                    return;
                }
            }

            if (gameObject.activeSelf)
                StartCoroutine(DestroyTimer());
        }

        private IEnumerator GoBackToObjManager()
        {
            float timeHandler = BACK_TO_PARENT_TIME;
            while (timeHandler > 0)
            {
                timeHandler -= Time.deltaTime;
                yield return null;
            }

            transform.parent = ObjectManager._instance.gameObject.transform;
            gameObject.SetActive(false);
        }
    }
}