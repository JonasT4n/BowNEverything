using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowfiedAnvil : NormalArrow
{
    [Header("Additional Info Anvil")]
    [SerializeField] private float _dropMultiplier = 8f;
    [SerializeField] private float _timeBeforeDrop = 1.5f;

    private float _defaultGravityScale;

    protected override void OnShootEffect(Vector2 origin, Vector2 shootDir)
    {
        base.OnShootEffect(origin, shootDir);

        _defaultGravityScale = ArrowRigid.gravityScale;
        StartCoroutine(DropTimer());
    }

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

            // Check entity hit
            LivingEntity hit = arg.VictimHit.gameObject.GetComponent<LivingEntity>();
            if (hit != null)
            {
                hit.AddHealth(Damage);

                // Knockback
                if (hit.CurrentHealth > 0)
                    hit.AddForce(new Vector2(Random.Range(0, 2) == 0 ? -100 : 100, 0), ForceMode2D.Impulse);

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

        ArrowRigid.gravityScale = _defaultGravityScale;
        if (gameObject.activeSelf)
            StartCoroutine(DestroyTimer());
    }

    private IEnumerator DropTimer()
    {
        float timeHolder = _timeBeforeDrop;
        while (timeHolder > 0)
        {
            timeHolder -= Time.deltaTime;
            yield return null;
        }

        if (gameObject.activeSelf)
        {
            ArrowRigid.gravityScale = _dropMultiplier;
            ArrowRigid.velocity = new Vector2(0, -Mathf.Abs(_dropMultiplier));
        }
    }
}
