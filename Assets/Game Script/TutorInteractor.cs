using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNEGame
{
    public class TutorInteractor : Interactable2D
    {
        [SerializeField] private TutorialScript _script = null;

        public override void Interact()
        {
            _script.InvokeInteraction(InteractCodeID);
        }
    }

}