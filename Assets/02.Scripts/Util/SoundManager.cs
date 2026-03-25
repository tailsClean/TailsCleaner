using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BGMName
{
    Stage_BGM_1,
    Stage_BGM_2,
    Stage_BGM_3,
}

public enum SkillSFXName
{
    Skill_Fire,
    Skill_Ice,
    Skill_Thunder,
    Hit,
    Die,
}

public enum MonsterSFXName
{
    Monster_Attack,
    Monster_Die,
    Monster_Hit,
    Monster_Roar,
}

[System.Serializable]
public struct BGMClipInfo
{
    public BGMName name;
    public AudioClip clip;
}

[System.Serializable]
public struct SkillSFXClipInfo
{
    public SkillSFXName name;
    public AudioClip clip;
}

[System.Serializable]
public struct MonsterSFXClipInfo
{
    public MonsterSFXName name;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance { get => _instance; private set => _instance = value; }

    [Header("BGM")]
    [SerializeField] private AudioSource _bgmPlayer;
    [SerializeField] private BGMClipInfo[] _bgmClips;
    [SerializeField] private bool _isLoop = true;
    [SerializeField] private bool _isShuffle = false;

    [Header("Skill SFX")]
    [SerializeField] private AudioSource _skillSfxPlayer;
    //[SerializeField] private SkillSFXClipInfo[] _skillSfxClips;     // 사용 X

    [Header("Monster SFX")]
    [SerializeField] private AudioSource _monsterSfxPlayer;
    [SerializeField] private MonsterSFXClipInfo[] _monsterSfxClips;

    [Header("같은 클립 최소 재생 간격")]
    [SerializeField] private float _debounceInterval = 0.1f;

    private List<int> _shuffleOrder = new List<int>();
    private int _currentBGMIndex = 0;
    private int _shuffleIndex = 0;

    private Dictionary<BGMName, AudioClip> _bgmDict = new Dictionary<BGMName, AudioClip>();
    //private Dictionary<SkillSFXName, AudioClip> _skillSfxDict = new Dictionary<SkillSFXName, AudioClip>();          // 사용 X
    private Dictionary<MonsterSFXName, AudioClip> _monsterSfxDict = new Dictionary<MonsterSFXName, AudioClip>();

    // 스킬 관련
    private List<AudioSource> _loopPool = new();                                     // 루프 
    private Dictionary<int, (AudioSource source, int count)> _loopChannels = new();  // 루프 채널   Key : MainTag  /  Value : (AudioSource, 카운트)
    private Dictionary<AudioClip, float> _lastPlayTimes = new();                     // 원샷 마지막 재생 시간


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var info in _bgmClips)
            _bgmDict[info.name] = info.clip;

        //foreach (var info in _skillSfxClips)
        //    _skillSfxDict[info.name] = info.clip;

        foreach (var info in _monsterSfxClips)
            _monsterSfxDict[info.name] = info.clip;
    }

    private void Start()
    {
        PlayBGMFromStart();
    }

    private void Update()
    {
        if (!_bgmPlayer.isPlaying && _bgmClips.Length > 0)
            PlayNext();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 시 스킬 효과음 관련 전부 중지, 제거
        ClearAllSkillLoopSFX();
    }

    public void PlayBGMFromStart()
    {
        if (_bgmClips.Length == 0) return;

        if (_isShuffle)
        {
            GenerateShuffleOrder();
            _currentBGMIndex = _shuffleOrder[0];
            _shuffleIndex = 0;
        }
        else
        {
            _currentBGMIndex = 0;
        }

        PlayBGMAt(_currentBGMIndex);
    }

    public void PlayBGM(BGMName bgmName)
    {
        if (_bgmDict.TryGetValue(bgmName, out AudioClip clip))
        {
            _bgmPlayer.clip = clip;
            _bgmPlayer.Play();
        }
    }

    private void PlayBGMAt(int index)
    {
        _bgmPlayer.clip = _bgmClips[index].clip;
        _bgmPlayer.loop = false;
        _bgmPlayer.Play();
    }

    private void PlayNext()
    {
        if (_isShuffle)
        {
            _shuffleIndex++;

            if (_shuffleIndex >= _shuffleOrder.Count)
            {
                if (!_isLoop) return;
                GenerateShuffleOrder();
                _shuffleIndex = 0;
            }

            _currentBGMIndex = _shuffleOrder[_shuffleIndex];
        }
        else
        {
            _currentBGMIndex++;

            if (_currentBGMIndex >= _bgmClips.Length)
            {
                if (!_isLoop) return;
                _currentBGMIndex = 0;
            }
        }

        PlayBGMAt(_currentBGMIndex);
    }

    private void GenerateShuffleOrder()
    {
        _shuffleOrder.Clear();
        for (int i = 0; i < _bgmClips.Length; i++)
            _shuffleOrder.Add(i);

        for (int i = _shuffleOrder.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (_shuffleOrder[i], _shuffleOrder[rand]) = (_shuffleOrder[rand], _shuffleOrder[i]);
        }
    }

    public void SetShuffle(bool value)
    {
        _isShuffle = value;
        if (_isShuffle) GenerateShuffleOrder();
    }

    public void SetLoop(bool value) => _isLoop = value;
    public void StopBGM() => _bgmPlayer.Stop();
    public void PauseBGM() => _bgmPlayer.Pause();
    public void ResumeBGM() => _bgmPlayer.UnPause();
    public void SetBGMVolume(float volume) => _bgmPlayer.volume = Mathf.Clamp01(volume);


    // 스킬 시전 클립 재생
    public void PlaySkillActiveSFX(SkillSoundData data)
    {
        if (data == null) return;
        PlayRandomSkillClip(data.onCast);
    }
    // 스킬 만료 클립 재생
    public void PlaySkillExpireSFX(SkillSoundData data)
    {
        if (data == null) return;
        PlayRandomSkillClip(data.onExpire);
    }
    // 스킬 특수 클립 재생
    public void PlaySkillSpecialSFX(SkillSoundData data)
    {
        if (data == null) return;
        PlayRandomSkillClip(data.onSpecial);
    }


    // 랜덤 클립 재생
    public void PlayRandomSkillClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        PlaySkillClip(clips[Random.Range(0, clips.Length)]);
    }

    // 클립 재생
    public void PlaySkillClip(AudioClip clip)
    {
        if (clip == null) return;

        float time = Time.time;

        /// debounceInterval 이내에 같은 클립 재생 시도 시 스킵
        if (_lastPlayTimes.TryGetValue(clip, out float last) && time - last < _debounceInterval)
            return;

        // 시간 기록 후 재생
        _lastPlayTimes[clip] = time;
        _skillSfxPlayer.PlayOneShot(clip);
    }



    // 스킬 지속 효과음 재생
    public void PlaySkillLoopSFX(int mainTag, SkillSoundData data)
    {
        if (data == null || data.loopClip == null) return;

        // 루프 채널 재생 중이면
        if (_loopChannels.TryGetValue(mainTag, out var entry))
        {
            // 카운트만 증가
            _loopChannels[mainTag] = (entry.source, entry.count + 1);
            return;
        }

        // 빈 슬롯 탐색
        AudioSource slot = FindFreeSlot();

        // 없으면 새로 생성해서 추가
        if (slot == null)
        {
            slot = _skillSfxPlayer.gameObject.AddComponent<AudioSource>();
            slot.playOnAwake = false;
            slot.loop = true;
            _loopPool.Add(slot);
        }

        // 슬롯 세팅 후 재생
        slot.clip = data.loopClip;
        slot.volume = 1f;
        slot.Play();

        _loopChannels[mainTag] = (slot, 1);
    }

    // 스킬 지속 효과음 중지
    public void StopSkillLoopSFX(int mainTag)
    {
        if (_loopChannels.TryGetValue(mainTag, out var entry) == false) return;

        // 하나 빼기
        int newCount = entry.count - 1;

        // 전부 다 중지되면
        if (newCount <= 0)
        {
            // 멈추고 비우기
            if (entry.source != null)
            {
                entry.source.Stop();
                entry.source.clip = null;
                _loopChannels.Remove(mainTag);
            }

            // 채널에서 제거
            _loopChannels.Remove(mainTag);
        }
        // 아니면 카운트 갱신
        else
        {
            _loopChannels[mainTag] = (entry.source, newCount);
        }
    }

    public void SetSkillSFXVolume(float volume)
    {
        float skillVolume = Mathf.Clamp01(volume);
        // 단일 볼륨
        _skillSfxPlayer.volume = skillVolume;

        // 루프 볼륨
        foreach (var source in _loopPool)
            source.volume = skillVolume;
    }

    // 빈 슬롯 찾기
    private AudioSource FindFreeSlot()
    {
        foreach (var source in _loopPool)
        {
            if (source.isPlaying == false) return source;
        }
        return null;
    }

    public void PlayMonsterSFX(MonsterSFXName sfxName)
    {
        if (_monsterSfxDict.TryGetValue(sfxName, out AudioClip clip))
            _monsterSfxPlayer.PlayOneShot(clip);
    }

    public void PlayMonsterSFX(MonsterSFXName sfxName, float volume)
    {
        if (_monsterSfxDict.TryGetValue(sfxName, out AudioClip clip))
            _monsterSfxPlayer.PlayOneShot(clip, volume);
    }

    public void SetMonsterSFXVolume(float volume) => _monsterSfxPlayer.volume = Mathf.Clamp01(volume);


    // 스킬 효과음 관련 전부 제거, 중지
    private void ClearAllSkillLoopSFX()
    {
        // 재생 중인 채널 전부 정지
        foreach (var entry in _loopChannels.Values)
        {
            if (entry.source != null)
                entry.source.Stop();
        }

        _loopChannels.Clear();

        // AudioSource 컴포넌트 전부 제거
        foreach (var source in _loopPool)
        {
            if (source != null)
                Destroy(source);
        }
        _loopPool.Clear();
    }
}