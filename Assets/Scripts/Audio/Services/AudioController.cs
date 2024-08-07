using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : SingletonMonoBehaviour<AudioController>
{
    private const string MusicVolumeKey = "CandyManiaMusicVolumeKey";
    private const string SfxVolumeKey = "CandyManiaSfxVolumeKey";

    [SerializeField] private AudioSettings _settings;

    private AudioSource _musicTrackSource;
    
    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;

    private readonly List<GameAudioSource> _activeSfxSources = new List<GameAudioSource>();

    public float MusicVolume { get => _musicVolume; set{ _musicVolume = value; _musicTrackSource.volume = _musicVolume; } }
    public float SfxVolume
    {
        get => _sfxVolume; 
        set
        {
            _sfxVolume = value; 
            foreach (GameAudioSource sfxSource in _activeSfxSources)
            {
                sfxSource.SetVolume(_sfxVolume);
            }
        } 
    }

    protected override void Awake()
    {
        base.Awake();
        
        ReadAudioParams();

        _musicTrackSource = GetComponent<AudioSource>();
        _musicTrackSource.volume = _musicVolume;
        _musicTrackSource.loop = false;

        AddEventHandlers();
        // PlayRandomTrack();
    }

    private void OnDestroy()
    {
        RemoveEventHandlers();
        
        SaveAudioParams();
    }

/*    private void Update()
    {
        if (_musicTrackSource.isPlaying)
            return;

        PlayNextTrack();
    }*/

    private void AddEventHandlers()
    {
        EventBus.Get.Subscribe<GameItemDetonationStartedEvent>(HandleGameItemDetonation);
        EventBus.Get.Subscribe<LevelCompletedEvent>(HandleLevelCompleted);
        EventBus.Get.Subscribe<LevelFailedEvent>(HandleLevelFailed);
        EventBus.Get.Subscribe<LevelTimeExpiringEvent>(HandleLevelTimeExpiring);
        EventBus.Get.Subscribe<ScoreCountingEvent>(HandleScoreCounting);
        EventBus.Get.Subscribe<ScoreChangedEvent>(HandleScoreChanged);
        EventBus.Get.Subscribe<UIEvents.ButtonClicked>(HandleUIButtonClicked);
        EventBus.Get.Subscribe<UIEvents.ViewMoving>(HandleUIViewMoving);
    }

    private void RemoveEventHandlers()
    {
        EventBus.Get.Unsubscribe<GameItemDetonationStartedEvent>(HandleGameItemDetonation);
        EventBus.Get.Unsubscribe<LevelCompletedEvent>(HandleLevelCompleted);
        EventBus.Get.Unsubscribe<LevelFailedEvent>(HandleLevelFailed);
        EventBus.Get.Unsubscribe<LevelTimeExpiringEvent>(HandleLevelTimeExpiring);
        EventBus.Get.Unsubscribe<ScoreCountingEvent>(HandleScoreCounting);
        EventBus.Get.Unsubscribe<ScoreChangedEvent>(HandleScoreChanged);
        EventBus.Get.Subscribe<UIEvents.ButtonClicked>(HandleUIButtonClicked);
        EventBus.Get.Subscribe<UIEvents.ViewMoving>(HandleUIViewMoving);
    }

    private void PlaySfx(SfxType sfxType, Transform targetTransform = null)
    {
        if ((targetTransform != null) && (!targetTransform.gameObject.activeSelf))
        {
            Debug.LogWarning("Target gameobject for sfx is not active !");
            return;
        }

        GameAudioSource audioSrc = CreateAudioSource(targetTransform);
        SetupAudioSource(audioSrc, _settings.GetSfxInfo(sfxType));
        audioSrc.Play();
    }

    private GameAudioSource CreateAudioSource(Transform targetTransform)
    {
        Transform transformForAudioSrc = (targetTransform == null) ? transform : targetTransform;
        GameAudioSource audioSrc = transformForAudioSrc.gameObject.AddComponent<GameAudioSource>();
        audioSrc.SetOnKillCallback(OnAudioSourceKilled);
        _activeSfxSources.Add(audioSrc);

        return audioSrc;
    }

    private void OnAudioSourceKilled(GameAudioSource aSource)
    {
        _activeSfxSources.Remove(aSource);
    }

    private void SetupAudioSource(GameAudioSource audioSrc, SfxInfo sfxInfo)
    {
        audioSrc.Setup(sfxInfo, _sfxVolume);
    }

    private AudioClip GetAudioClip(SfxType type)
    {
        return _settings.GetAudioClip(type);
    }

    private void PlayRandomTrack()
    {
        _musicTrackSource.clip = _settings.GetRandomMusicTrack();
        _musicTrackSource.Play();
    }

    private void PlayNextTrack()
    {
        _musicTrackSource.clip = _settings.GetNextMusicTrack();
        _musicTrackSource.Play();
    }

    private void ReadAudioParams()
    {
        if (PlayerPrefs.HasKey(MusicVolumeKey))
            _musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey);
        if (PlayerPrefs.HasKey(SfxVolumeKey))
            _sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey);
    }

    private void SaveAudioParams()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, _musicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, _sfxVolume);
    }
    
    private void HandleGameItemDetonation(GameItemDetonationStartedEvent ev)
    {
        GameItem item = ev.Item;
        
        if (item.IsUsual())
        {
            PlaySfx(SfxType.ItemDetonation);
        }
        else
        {
            PlaySfx(item.IsStar() ? SfxType.StarDetonation : SfxType.BombDetonation);
        }
    }
    
    private void HandleLevelCompleted()
    {
        PlaySfx(SfxType.LevelCompleted);
    }
    
    private void HandleLevelFailed()
    {
        PlaySfx(SfxType.LevelFailed);
    }
    
    private void HandleLevelTimeExpiring()
    {
        PlaySfx(SfxType.TimeTick);
    }
    
    private void HandleUIButtonClicked()
    {
        PlaySfx(SfxType.ButtonClick);
    }
    
    private void HandleUIViewMoving()
    {
        PlaySfx(SfxType.UIPanelSlide);
    }
    
    private void HandleScoreCounting()
    {
        PlaySfx(SfxType.ScoreCount);
    }
    
    private void HandleScoreChanged(ScoreChangedEvent ev)
    {
        PlaySfx(SfxType.ScoreAdd);
    }
}
