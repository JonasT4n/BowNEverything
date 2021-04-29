using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNEGame
{
    public class WindowManager : MonoBehaviour
    {
        private Vector2Int _lastResize;
        private IEnumerator _resizeCheckRoutine;

        #region Unity BuiltIn Methods
        private void OnEnable()
        {
            // Get initial screen size
            _lastResize = new Vector2Int(Screen.width, Screen.height);

            _resizeCheckRoutine = ResizeCoroutine();
            StartCoroutine(_resizeCheckRoutine);
        }

        private void OnDisable()
        {
            StopCoroutine(_resizeCheckRoutine);
        }
        #endregion

        private IEnumerator ResizeCoroutine()
        {
            while (gameObject.activeSelf)
            {
                if (_lastResize.x != Screen.width || _lastResize.y != Screen.height)
                {
                    Vector2Int newSize = new Vector2Int(Screen.width, Screen.height);
                    EventHandler.CallWindowResizeEvent(_lastResize, newSize);
                    _lastResize = newSize;
                }
                yield return null;
            }
        }
    }
}