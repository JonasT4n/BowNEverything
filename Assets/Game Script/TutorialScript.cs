using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NaughtyAttributes;

public class TutorialScript : MonoBehaviour
{
    [Header("Dialog Attributes")]
    [SerializeField] private RectTransform _tutorialDialogPanel = null;
    [SerializeField] private Text _textShow = null;
    [SerializeField] private InputHandler _inputHandler = null;
    [SerializeField] private string[] _dialogText = null;

    [Space, Header("Hint Attributes")]
    [SerializeField] private Text _hint = null;
    [SerializeField] private BoxCollider2D _finishLine = null;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private List<string> _story = new List<string>();
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _isDialogLineFinish = false;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _isInTutorial = true;

    #region Unity BuiltIn Methods
    private void Awake()
    {
        ResetTutorial();
    }

    private void FixedUpdate()
    {
        if (IsPlayerReachFinishLine() && _isInTutorial)
        {
            _isInTutorial = false;
            _story.Add("Congratulation, you have passed the tutorial.");
            StartCoroutine(GamePrologue());
        }
    }
    #endregion

    private bool IsPlayerReachFinishLine()
    {
        RaycastHit2D hit = Physics2D.BoxCast(_finishLine.bounds.center, _finishLine.size, 0, Vector2.down, 0);
        if (hit.collider != null)
        {
            if (hit.collider.GetComponent<PlayerEntity>())
            {
                hit.collider.GetComponent<PlayerEntity>().EntityR2D.velocity = Vector2.zero;
                return true;
            }
        }

        return false;
    }

    public void OpenDialog(bool active)
    {
        _tutorialDialogPanel.gameObject.SetActive(active);
    }

    public void ResetTutorial()
    {
        _story.Clear();
        _story.AddRange(_dialogText);
        StartCoroutine(GamePrologue());
    }

    private IEnumerator GamePrologue()
    {
        OpenDialog(true);
        _hint.gameObject.SetActive(false);
        _inputHandler.InputLocked = true;
        while (_story.Count > 0)
        {
            string txtDialog = _story[0];
            _isDialogLineFinish = false;
            _story.RemoveAt(0);

            StartCoroutine(DialogAnimation(txtDialog));

            bool hitNextLine = false;
            while (!_isDialogLineFinish && !hitNextLine)
            {
                yield return null;
                hitNextLine = Input.touchCount > 0 || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);

                if (hitNextLine && !_isDialogLineFinish)
                    hitNextLine = false;
            }
        }

        _inputHandler.InputLocked = false;
        _hint.gameObject.SetActive(true);
        OpenDialog(false);

        if (!_isInTutorial)
        {
            UIGameSceneManager sceneManager = FindObjectOfType<UIGameSceneManager>();
            if (sceneManager != null)
                sceneManager.LoadScene(0);
        }
    }

    private IEnumerator DialogAnimation(string txt)
    {
        int txtLength = txt.Length;
        _textShow.text = "";
        bool isTapNextContinue = false;
        while (_textShow.text.Length < txtLength || !isTapNextContinue)
        {
            try
            {
                _textShow.text += txt[_textShow.text.Length];
            } 
            catch (System.IndexOutOfRangeException e)
            {
                // Do Nothing
            }
            
            yield return null;

            if ((Input.touchCount > 0 || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && _textShow.text.Length < txtLength)
                _textShow.text = txt;
            else if (Input.touchCount > 0 || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                isTapNextContinue = true;
        }

        _isDialogLineFinish = true;
    }
}
