using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] Grunts;
    public AudioClip[] Die;
    public AudioClip[] Fail;
    public AudioClip[] Succeed;
    public AudioClip[] Idle;
    public AudioClip[] StartLevel;
    public AudioClip[] EndLevel;
    public AudioClip[] OneRoom;
    public AudioClip[] Explain;
    public AudioClip[] OneRoomExp;
    public AudioClip[] Music;
    public AudioClip HappyMusic;

    public AudioSource MainAudioSource;
    public AudioSource GruntLoopAudioSource;
    public AudioSource MusicAudioSource;

    public Vector2 IdleTimeRange = new Vector2(5f, 10f);
    public float GruntFadeSpeed = 4f;

    public Vector2 MusicTimeRange = new Vector2(8f, 12f);

    // Use this for initialization
    void Start ()
    {
        ResetIdle();
        musicCount = Random.Range(MusicTimeRange.x, MusicTimeRange.y);
    }

    void ResetIdle()
    {
        idleCounter = Random.Range(IdleTimeRange.x, IdleTimeRange.y);
    }

    private float musicCount;

    private float idleCounter;

    private int oddOrEven = 0;

    public void PlayWinMusic()
    {
        musicCount = 100;
        MusicAudioSource.clip = HappyMusic;
        MusicAudioSource.Play();
    }

	void Update ()
    {
        // Occasionally play idle sounds.	
        idleCounter -= Time.deltaTime;
        musicCount -= Time.deltaTime;

        if (musicCount <= 0f)
        {
            // Play random music
            AudioClip clip = Music[Random.Range(0, Music.Length)];
            MusicAudioSource.clip = clip;
            MusicAudioSource.Play();


            musicCount = Random.Range(MusicTimeRange.x, MusicTimeRange.y);
        }

        if (idleCounter <= 0f)
        {
            if (!playingPushingSound)
            {
                if (needToExplain)
                {
                    if (oddOrEven == 0)
                    {
                        PlayRandom(OneRoom);
                    }
                    else
                    {
                        PlayRandom(Explain);
                    }
                    oddOrEven++;
                    oddOrEven %= 2;
                }
                else
                {
                    PlayRandom(Idle, false); // Don't usurp existing playing sounds.
                }
            }
            ResetIdle();
        }

        // Adjust grunt
        float gruntVolume = GruntLoopAudioSource.volume;
        if (gruntVolume != gruntTargetVolume)
        {
            float sign = Mathf.Sign(gruntTargetVolume - gruntVolume);
            gruntVolume += sign * Time.deltaTime * GruntFadeSpeed;
            gruntVolume = Mathf.Clamp01(gruntVolume);
            GruntLoopAudioSource.volume = gruntVolume;


        }
    }

    public void PlayStart()
    {
        PlayRandom(StartLevel);
        ResetIdle();
    }

    public void PlayEnd()
    {
        PlayRandom(EndLevel);
        ResetIdle();
    }

    public void PlayGrunt()
    {
        PlayRandom(Grunts);
        ResetIdle();
    }

    private float gruntTargetVolume = 0f;

    private bool needToExplain = true;
    public void NeedToExplain(bool need)
    {
        needToExplain = need;
    }

    public void PlayPushing()
    {
        if (!playingPushingSound)
        {
            gruntTargetVolume = 1f;
            ResetIdle();
            playingPushingSound = true;
        }
    }
    public void StopPushing()
    {
        if (playingPushingSound)
        {
            gruntTargetVolume = 0f;
            playingPushingSound = false;
        }
    }
    public void PlayDie()
    {
        PlayRandom(Die);
        ResetIdle();
    }
    public void PlaySuccess()
    {
        PlayRandom(Succeed, false); // Don't interrupt...
        ResetIdle();
    }
    public void PlayFail()
    {
        if ((Level.LevelNumber <= 1) && (Random.Range(0, 2) == 0))
        {
            // Provide a little explanation
            PlayRandom(OneRoomExp, false); // Don't usurp... could be called a lot.
        }
        else
        {
            PlayRandom(Fail, false); // Don't usurp... could be called a lot.
        }
        ResetIdle();
    }

    bool playingPushingSound = false;

    private void PlayRandom(AudioClip[] clips, bool usurp = true)
    {
        // Bail if we don't want to override the current one (unless it's a loop)
        if (!usurp)
        {
            if (MainAudioSource.isPlaying)
            {
                return;
            }
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];



        MainAudioSource.clip = clip;
        MainAudioSource.Play();

        StopPushing();
    }
}
