using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace BNEGame
{
    public class EntityUIInformation : MonoBehaviour
    {
        [SerializeField] private UIObjectFollower _uiFollowerPrefab = null;
        [SerializeField] private RectTransform _gui = null;

        [Header("Placeholders")]
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private LivingEntity _entity = null;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Transform _placeholderMaster = null;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Slider _healthGauge = null;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Slider _drawingTimeGauge = null;

        public float HealthValue
        {
            set
            {
                if (_healthGauge != null)
                    _healthGauge.value = value;
            }
        }

        public float MaxHealthValue
        {
            set
            {
                if (_healthGauge != null)
                    _healthGauge.maxValue = value;
            }
        }

        public float DrawingTimeValue
        {
            set
            {
                if (_drawingTimeGauge != null)
                    _drawingTimeGauge.value = value;
            }
        }

        #region Unity BuiltIn Methods
        // Start is called before the first frame update
        private void Awake()
        {
            _entity = GetComponent<LivingEntity>();
            if (_gui == null)
            {
                if (FindObjectOfType<UIManager>() != null)
                    _gui = FindObjectOfType<UIManager>().GetDefaultGamePanel();
            }

            _uiFollowerPrefab.TargetFollow = _entity.transform;

            // Set placeholders from static prefab, check indexes in the follower prefab
            _placeholderMaster = Instantiate(_uiFollowerPrefab, _gui.transform).transform;
            _placeholderMaster.name = $"{name} --> {_placeholderMaster.name}";

            Transform drawTimeGauge = _placeholderMaster.GetChild(0);
            if (drawTimeGauge != null)
            {
                if (_entity.tag == "Player")
                    _drawingTimeGauge = drawTimeGauge.GetComponent<Slider>();
                else
                    drawTimeGauge.gameObject.SetActive(false);
            }

            Transform healthGauge = _placeholderMaster.GetChild(1);
            if (healthGauge != null)
                _healthGauge = healthGauge.GetComponent<Slider>();

            // Set target follow
            _uiFollowerPrefab.TargetFollow = _entity.transform;
        }
        #endregion

        public void ShowUIFollower(bool show)
        {
            if (_placeholderMaster != null)
                _placeholderMaster.gameObject.SetActive(show);
        }
    }

}
