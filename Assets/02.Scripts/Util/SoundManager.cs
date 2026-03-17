using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private SkillSFXClipInfo[] _skillSfxClips;

    [Header("Monster SFX")]
    [SerializeField] private AudioSource _monsterSfxPlayer;
    [SerializeField] private MonsterSFXClipInfo[] _monsterSfxClips;

    private List<int> _shuffleOrder = new List<int>();
    private int _currentBGMIndex = 0;
    private int _shuffleIndex = 0;

    private Dictionary<BGMName, AudioClip> _bgmDict = new Dictionary<BGMName, AudioClip>();
    private Dictionary<SkillSFXName, AudioClip> _skillSfxDict = new Dictionary<SkillSFXName, AudioClip>();
    private Dictionary<MonsterSFXName, AudioClip> _monsterSfxDict = new Dictionary<MonsterSFXName, AudioClip>();

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

        foreach (var info in _skillSfxClips)
            _skillSfxDict[info.name] = info.clip;

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

    public void PlaySkillSFX(SkillSFXName sfxName)
    {
        if (_skillSfxDict.TryGetValue(sfxName, out AudioClip clip))
            _skillSfxPlayer.PlayOneShot(clip);
    }

    public void PlaySkillSFX(SkillSFXName sfxName, float volume)
    {
        if (_skillSfxDict.TryGetValue(sfxName, out AudioClip clip))
            _skillSfxPlayer.PlayOneShot(clip, volume);
    }

    public void SetSkillSFXVolume(float volume) => _skillSfxPlayer.volume = Mathf.Clamp01(volume);

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
}