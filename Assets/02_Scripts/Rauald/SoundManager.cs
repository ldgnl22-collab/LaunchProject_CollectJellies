using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    // BGM 오디오 소스
    [SerializeField] private AudioSource _bgm;
    // SFX 오디오 소스 풀링용
    [SerializeField] private AudioSource _sfx;

    // BGM 모음
    [SerializeField] private AudioClip[] _arrBGM;
    // SFX 모음
    [SerializeField] private AudioClip[] _arrSFX;

    // BGM 검색
    private Dictionary<BGMType, AudioClip> _dicBGM;
    // SFX 검색
    private Dictionary<SFXType, AudioClip> _dicSFX;

    // 현재 BGM
    private BGMType _curBGM;

    // BGM 볼륨
    private float _bgmVolume;
    // SFX 볼륨
    private float _sfxVolume;

    // BGM 음소거
    private bool _isMuteBGM;
    private bool _isMuteSFX;

    private void Awake()
    {
        SetSingleton();
    }

    // 초기화
    public void Init()
    {
        // 현재 로컬 저장x
        // 로컬 저장을 만들면 뮤트 불러오기
        _isMuteBGM = false;
        _isMuteSFX = false;

        // 배경음 dic에 넣기
        for (int i = 0; i < _arrBGM.Length; i++)
        {
            _dicBGM.Add((BGMType)i, _arrBGM[i]);
        }
        // 효과음 dic에 넣기
        for (int i = 0; i < _arrSFX.Length; i++)
        {
            _dicSFX.Add((SFXType)i, _arrSFX[i]);
        }
    }

    /// <summary> 배경음 플레이 </summary>
    /// <param name="bgm"> 배경음 종류 </param>
    public void PlayBGM(BGMType bgm)
    {
        // 현재 배경음이랑 같으면 스킵
        if (_curBGM == bgm)
            return;

        // 실행할 오디오
        AudioClip clip;
        
        // 오디오가 있는지 확인
        if(_dicBGM.TryGetValue(bgm, out clip))
        {
            // 있다면 멈추고
            _bgm.Stop();
            // 넣고
            _curBGM = bgm;
            _bgm.clip = clip;

            // 배경음이 뮤트인지 확인
            if (!_isMuteBGM)
            {
                // 뮤트가 아니라면 배경음 플레이
                _bgm.Play();
            }
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogError("배경음을 찾을 수 없습니다.");
        }
#endif
    }

    /// <summary> 배경음 볼륨 조절 </summary>
    /// <param name="volume"> 볼륨 </param>
    public void BGMVolume(float volume)
    {
        _bgmVolume = volume;
        _bgm.volume = _bgmVolume;
    }

    /// <summary> 배경음 음소거 (환경설정) </summary>
    public void MuteBGM()
    {
        // 음소거 ON / OFF
        _isMuteBGM = !_isMuteBGM;
        _bgm.mute = _isMuteBGM;

        if (_isMuteBGM)
            _bgm.Stop();
        else
            _bgm.Play();
    }

    /// <summary> 효과음 플레이 </summary>
    /// <param name="sfx"> 효과음 종류 </param>
    public void PlaySFX(SFXType sfx)
    {
        // SFX가 음소거 모드라면 틀지 않는다.
        if (_isMuteSFX)
            return;

        // 실행할 오디오
        AudioClip clip;

        // 오디오가 있는지 확인
        if(_dicSFX.TryGetValue(sfx, out clip))
        {
            // 있다면 넣기
            _sfx.PlayOneShot(clip);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogError("효과음이 없습니다.");
        }
#endif
    }

    /// <summary> 효과음 볼륨 조절 </summary>
    /// <param name="volume"> 볼륨 </param>
    public void SFXVolume(float volume)
    {
        _sfxVolume = volume;
        _sfx.volume = _sfxVolume;
    }

    /// <summary> 효과음 음소거 (환경설정) </summary>
    public void MuteSFX()
    {
        _isMuteSFX = !_isMuteSFX;
        _sfx.mute = _isMuteSFX;
    }
}