using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BGMSetting
{
    public float bgmVolume = 0.5f;
    public float bgmSpeed = 0.7f;
    public bool bgmOnOff = true;
}

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;  // 싱글톤 (다른 스크립트에서도 접근 가능)

    [Header("Audio Source")]
    public AudioSource bgmSource;       // BGM 오디오 소스

    [Header("UI Controls (선택사항)")]
    public Slider volumeSlider;         // 볼륨 조절 슬라이더
    public Toggle bgmToggle;            // On/Off 토글

    public BGMSetting bgmSetting = new BGMSetting();
    private string filePath;
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject); // 씬 전환시 삭제 안됨
        filePath = Path.Combine(Application.persistentDataPath, "bgmData.json");
    }

    private void Start()
    {
        bgmSetting = JsonManager.LoadData1(filePath, bgmSetting);
        // 무한 반복
        bgmSource.loop = true;
        bgmSource.pitch = bgmSetting.bgmSpeed;
        bgmSource.volume = bgmSetting.bgmVolume;
        bgmToggle.isOn = bgmSetting.bgmOnOff;

        // 슬라이더 초기값 설정
        if (volumeSlider != null)
        {
            volumeSlider.value = bgmSource.volume;
            //volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // 토글 초기값 설정
        if (bgmToggle != null)
        {
            bgmToggle.isOn = bgmSource.isPlaying;
            //bgmToggle.onValueChanged.AddListener(SetBGMOnOff);
        }
    }

    // 🎵 BGM 재생
    public void PlayBGM()
    {
        if (!bgmSource.isPlaying)
            bgmSource.Play();
    }

    // ⏸️ BGM 정지
    public void StopBGM()
    {
        if (bgmSource.isPlaying)
            bgmSource.Stop();
    }

    // 🔊 볼륨 조절
    public void SetVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    // 🎚️ On/Off (토글용)
    public void SetBGMOnOff(bool isOn)
    {
        if (isOn) PlayBGM();
        else StopBGM();
    }
    private void InitData()
    {
        bgmSetting.bgmVolume = bgmSource.volume;
        bgmSetting.bgmOnOff = bgmToggle.isOn;
    }
    public void SaveBGMSetting()
    {
        InitData();
        JsonManager.SaveData(bgmSetting, filePath);
    }
}
