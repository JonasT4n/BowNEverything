using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNEGame;

public class SoundManager : MonoBehaviour
{
    public static SoundManager _instance;

    [SerializeField] private AudioSource _audioMasterSource = null;

    [Space, Header("All Sounds")]
    public AudioClip _backgroundMusic = null;
    public AudioClip _hitEntitySound = null;
    public AudioClip _hitBlockSound = null;

    #region Unity BuiltIn Methods
    private void Awake()
    {
        if (_instance)
        {
            Debug.Log($"Deleted multiple object of singleton behaviour: {name}");
            Destroy(this);
            return;
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        _audioMasterSource.Play();

        // Subscribe events
        EventHandler.OnArrowHitEvent += ArrowHitSmh;
    }

    private void OnDestroy()
    {
        // Subscribe events
        EventHandler.OnArrowHitEvent -= ArrowHitSmh;
    }
    #endregion

    private void ArrowHitSmh(ArrowHitEventArgs args)
    {
        if (args.VictimHit.GetComponent<LivingEntity>())
            _audioMasterSource.PlayOneShot(_hitEntitySound);
        else
            _audioMasterSource.PlayOneShot(_hitBlockSound);
    }
}
