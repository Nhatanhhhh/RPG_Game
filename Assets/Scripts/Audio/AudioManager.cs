using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioMixer masterMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";

    // Unity vẫn sẽ hiển thị mảng này vì 'Sound' là public và [System.Serializable]
    public Sound[] sounds;

    private Dictionary<string, Sound> soundMap;
    private string currentMusicPlaying = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneChanged;

        soundMap = new Dictionary<string, Sound>();

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            // 's.type' và 'AudioType.Music' vẫn hoạt động hoàn hảo
            if (s.type == AudioType.Music && musicGroup != null)
            {
                s.source.outputAudioMixerGroup = musicGroup;
            }
            else if (s.type == AudioType.SFX && sfxGroup != null)
            {
                s.source.outputAudioMixerGroup = sfxGroup;
            }

            if (!soundMap.ContainsKey(s.name))
            {
                soundMap.Add(s.name, s);
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }

    void Start()
    {
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1.0f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1.0f);

        if (masterMixer != null)
        {
            masterMixer.SetFloat("MusicVolume", ConvertToDecibel(musicVolume));
            masterMixer.SetFloat("SFXVolume", ConvertToDecibel(sfxVolume));
        }
    }

    void OnSceneChanged(Scene current, Scene next)
    {
        string newSceneName = next.name;

        switch (newSceneName)
        {
            case "Main_Menu":
                PlayMusic("Back_Ground");
                break;
            case "MAP1":
                PlayMusic("Music_Map1");
                break;
            case "MAP2":
                PlayMusic("Music_Map2");
                break;
            case "MAP3":
                PlayMusic("Music_Map3");
                break;
            case "MAP4":
                PlayMusic("Music_Map4");
                break;
            case "MAP5":
                PlayMusic("Music_Map5");
                break;
            default:
                StopAllMusic();
                break;
        }
    }

    public void PlayMusic(string name)
    {
        if (name == currentMusicPlaying)
        {
            return;
        }

        if (soundMap.TryGetValue(name, out Sound s))
        {
            if (s.type == AudioType.Music)
            {
                StopAllMusic();
                s.source.Play();
                currentMusicPlaying = name;
            }
            else
            {
                Debug.LogWarning("AudioManager: " + name + " is not an 'Music' type sound.");
            }
        }
        else
        {
            Debug.LogWarning("AudioManager: Sound name " + name + " not found!");
        }
    }

    public void PlaySFX(string name)
    {
        if (soundMap.TryGetValue(name, out Sound s))
        {
            if (s.type == AudioType.SFX)
            {
                s.source.PlayOneShot(s.clip);
            }
            else
            {
                Debug.LogWarning("AudioManager: " + name + " is not an 'SFX' type sound.");
            }
        }
        else
        {
            Debug.LogWarning("AudioManager: Sound name " + name + " not found!");
        }
    }

    private void StopAllMusic()
    {
        currentMusicPlaying = "";
        foreach (Sound s in sounds)
        {
            if (s.type == AudioType.Music && s.source.isPlaying)
            {
                s.source.Stop();
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat("MusicVolume", ConvertToDecibel(volume));
        }
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat("SFXVolume", ConvertToDecibel(volume));
        }
        PlayerPrefs.SetFloat(SFX_VOL_KEY, volume);
        PlayerPrefs.Save();
    }

    private float ConvertToDecibel(float volume)
    {
        if (volume <= 0.0001f)
        {
            return -80.0f;
        }
        return Mathf.Log10(volume) * 20;
    }
}