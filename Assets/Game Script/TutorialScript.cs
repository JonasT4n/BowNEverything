using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NaughtyAttributes;

namespace BNEGame
{
    public class TutorialScript : MonoBehaviour, IReceiveInteration
    {
        [Header("Dialog Attributes")]
        [SerializeField] private RectTransform tutorDialogPanel = null;
        [SerializeField] private ItemSpawnerManager itemSpawner = null;
        [SerializeField] private UIGameSceneManager sceneManager = null;
        [SerializeField] private Text tutorTextDisplay = null;

        private IEnumerator dialogRoutine;
        private InputHandler inputHandler;

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private List<string> texts = new List<string>();
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isDialogFinish = false;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isDoingTutorial = true;

        #region Unity BuiltIn Methods
        private void Start()
        {
            inputHandler = FindObjectOfType<InputHandler>();
            itemSpawner.SetRunning(true);
            isDoingTutorial = true;
            ResetTutorial();
        }
        #endregion

        public void InvokeInteraction(string codeId)
        {
            if (dialogRoutine != null)
                return;

            switch (codeId)
            {
                case "Movement101":
                    texts.AddRange(new string[] {
                    "Press A or D to move left or right."
                });
                    break;

                case "FinishLine101":
                    texts.AddRange(new string[] {
                    "Congratulation, you have reached the Finish Line!"
                });
                    isDoingTutorial = false;
                    break;

                case "JumpTutor101":
                    texts.AddRange(new string[] {
                    "Press Space to jump over the platform and cliffs.",
                    "Try jump over that cliff."
                });
                    break;

                case "CollectArrows101":
                    texts.AddRange(new string[] {
                    "Arrow-like items will be spawned in this tree of life.",
                    "Collect them all, fill up your quiver, you can only collect 2 for each arrow-like things",
                    "The arrow-like thing that you are currently use is on under right corner of your screen."
                });
                    break;

                case "ShootArrow101":
                    texts.AddRange(new string[] {
                    "To shoot an arrow, hold left click mouse button.",
                    "Wait until the drawing gauge is filled under your HP bar",
                    "Release left mouse button after it is filled",
                    "There's other note, change current use to other arrow-like you can press Q."
                });
                    break;

                case "Enemies101":
                    texts.AddRange(new string[] {
                    "Now shoot those enemies in front of you!",
                    "They always spawn on the dark altar, so be careful."
                });
                    break;

                default:
                    return;
            }

            dialogRoutine = GameDialogBegin();
            StartCoroutine(dialogRoutine);
        }

        public void OpenDialog(bool active)
        {
            tutorDialogPanel.gameObject.SetActive(active);
        }

        public void ResetTutorial()
        {
            texts.Clear();
            InvokeInteraction("Movement101");
        }

        private IEnumerator GameDialogBegin()
        {
            OpenDialog(true);
            inputHandler.InputLocked = true;
            while (texts.Count > 0)
            {
                string txtDialog = texts[0];
                isDialogFinish = false;
                texts.RemoveAt(0);

                StartCoroutine(DialogAnimation(txtDialog));

                bool hitNextLine = false;
                while (!isDialogFinish && !hitNextLine)
                {
                    yield return null;
                    hitNextLine = Input.touchCount > 0 || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);

                    if (hitNextLine && !isDialogFinish)
                        hitNextLine = false;
                }
            }

            inputHandler.InputLocked = false;
            OpenDialog(false);
            dialogRoutine = null;

            // Check tutorial finished
            if (!isDoingTutorial)
                sceneManager.LoadScene(0);
        }

        private IEnumerator DialogAnimation(string txt)
        {
            int txtLength = txt.Length;
            tutorTextDisplay.text = "";
            bool isTapNextContinue = false;
            while (tutorTextDisplay.text.Length < txtLength || !isTapNextContinue)
            {
                if (txtLength > tutorTextDisplay.text.Length)
                    tutorTextDisplay.text += txt[tutorTextDisplay.text.Length];

                yield return null;

                if ((Input.touchCount > 0 || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && tutorTextDisplay.text.Length < txtLength)
                    tutorTextDisplay.text = txt;
                else if (Input.touchCount > 0 || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                    isTapNextContinue = true;
            }

            isDialogFinish = true;
        }

    }

}