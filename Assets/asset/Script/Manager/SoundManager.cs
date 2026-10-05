using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioClip buySound;
    public AudioClip sellSound;
    public AudioClip equipSound;
    public AudioClip refreshSound;
    public AudioClip ChoiceSound;
    public AudioClip GameStartSound;
    public AudioClip GameQuitSound;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayBuySound() => PlaySound(buySound);
    public void PlaySellSound() => PlaySound(sellSound);
    public void PlayEquipSound() => PlaySound(equipSound);
    public void PlayRefreshSound() => PlaySound(refreshSound);
    public void PlayChoiceSound() => PlaySound(ChoiceSound);
    public void PlayGameStartSound() => PlaySound(GameStartSound);
    public void PlayGameQuitSound() => PlaySound(GameQuitSound);

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}
