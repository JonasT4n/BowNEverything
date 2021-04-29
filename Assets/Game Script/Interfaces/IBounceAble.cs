using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNEGame
{
    public interface IBounceAble
    {
        int MaxBounceCounter { set; get; }

        void Bounce(Vector3 currentMoveDir, Collider2D hit);
    }

}