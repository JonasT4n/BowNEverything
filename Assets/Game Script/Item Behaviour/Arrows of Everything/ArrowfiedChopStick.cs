using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowfiedChopStick : NormalArrow
{
    private const float BACK_TO_PARENT_TIME = 2f;

    protected override void OnHitEffect(Vector3 pos, LivingEntity hit = null)
    {
        if (hit != null)
        {
            transform.parent = hit.transform;
            _poolRef.Enqueue(this);
            StartCoroutine(GoBackToObjManager());
            return;
        }

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
