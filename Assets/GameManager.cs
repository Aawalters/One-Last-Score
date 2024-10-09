using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("General")]
    public GameEnemyManager GameEnemyManager;
    public GameObject Player;
    public GameObject Canvas;
    public GameObject Camera;

    [Header("Art/Audio")]
    public AudioSource audioSource;
    public AudioClip KickAudio;
    public AudioClip MissAudio;
    public AudioClip CardPullAudio;
    public AudioClip GoodPullAudio;
    public AudioClip BadPullAudio;
    public AudioClip DeckShuffle;

    [Header("Cards")]
    public DeckController deckController;
    public float cardCDTime = 5.0f, cardCDTimer = 0;
    public bool cardIsOnCD = false;
    private Image UICard;
    private Image CooldownImg;
    private StatusEffectManager StatusEffectManager;

    [Header("Health")]
    public int healthCurrent;       // Current health of the player
    public int healthMax = 100;     // Maximum health of the player
    [HideInInspector]
    public Slider healthBar;       // UI Slider for health bar

    [Header("Money")]
    public float wager = 500;
    public float multiplier = 1f; 
    public float quota;

    [Header("Mobile")]
    public bool mobile;
    [HideInInspector]
    public GameObject iOSPanel;

    [HideInInspector]
    public Player p;
    [HideInInspector]
    public PlayerController playerController;

    public TextMeshProUGUI wager_text;
    private TextMeshProUGUI quota_text; 

    private GameObject PlayScreen;
    private GameObject WagerScreen;
    private GameObject PauseScreen;
    private GameObject DeathScreen;
    private GameObject WinScreen;
    private GameObject IBuild;

    private bool paused = false;

    [Header("FX")]
    // hit fx
    public bool InHitStop = false;
    public AnimationCurve Curve;

    private static GameManager instance;

    void Awake()
    {
        //if (instance == null)
        //{
        //    instance = this;
        //    DontDestroyOnLoad(gameObject);
        //    SceneManager.sceneLoaded += OnSceneLoaded; // Add listener for scene change
        //}
        //else
        //{
        //    Destroy(gameObject); // Prevent duplicate instances
        //}

        AssignReferences();

    }

    public static GameManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("GameManager instance is not assigned.");
        }
        return instance;
    }

    // Called when a scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignReferences(); // Reassign UI references
    }

    void AssignReferences()
    {
        Canvas = GameObject.FindGameObjectWithTag("Canvas");
        Player = GameObject.FindGameObjectWithTag("Player");
        Camera = GameObject.FindGameObjectWithTag("MainCamera");

        if (Player)
        {
            playerController = Player.GetComponent<PlayerController>();
        }

        if (Canvas)
        {
            PlayScreen = Canvas.transform.Find("Play Screen")?.gameObject;
            WagerScreen = Canvas.transform.Find("Wager Screen")?.gameObject;
            PauseScreen = Canvas.transform.Find("Pause Screen")?.gameObject;
            DeathScreen = Canvas.transform.Find("Death Screen")?.gameObject;
            WinScreen = Canvas.transform.Find("Win Screen")?.gameObject;
            IBuild = Canvas.transform.Find("InclusivityBuildPanel")?.gameObject;
        }

        if (PlayScreen)
        {
            wager_text = PlayScreen.transform.Find("Wager")?.gameObject.GetComponent<TextMeshProUGUI>();
            quota_text = PlayScreen.transform.Find("Quota")?.gameObject.GetComponent<TextMeshProUGUI>();
            UICard = PlayScreen.transform.Find("Card")?.gameObject.GetComponent<Image>();
            CooldownImg = PlayScreen.transform.Find("Card")?.gameObject.GetComponentInChildren<Image>();
            StatusEffectManager = PlayScreen.transform.Find("Card")?.GetComponent<StatusEffectManager>();
            healthBar = PlayScreen.GetComponentInChildren<Slider>();

            wager_text.text = "Wager:" + wager.ToString();
        }

        if (DeathScreen || WinScreen || PauseScreen)
        {
            Button MenuButton = DeathScreen.transform.Find("Menu")?.gameObject.GetComponent<Button>();
            if (MenuButton)
            {
                MenuButton.onClick.AddListener(Menu);
            }

            if (DeathScreen)
            {
                Button restartButton = DeathScreen.transform.Find("Restart")?.gameObject.GetComponent<Button>();
                if (restartButton)
                {
                    restartButton.onClick.AddListener(() => Restart(SceneManager.GetActiveScene().name));
                }
            }
            if (WinScreen)
            {
                Button MapButton = WinScreen.transform.Find("Map")?.gameObject.GetComponent<Button>();
                if (MapButton)
                {
                    MapButton.onClick.AddListener(Map);
                }
            }
            if (PauseScreen)
            {
                Button resumeButton = PauseScreen.transform.Find("Resume")?.gameObject.GetComponent<Button>();
                if (resumeButton)
                {
                    resumeButton.onClick.AddListener(Pause);
                }
            }

        }

        if (IBuild)
        {
            iOSPanel = IBuild.transform.Find("iOS Panel")?.gameObject;
        }

        audioSource = GetComponent<AudioSource>();
        GameEnemyManager = GetComponentInChildren<GameEnemyManager>();
    }

    void Update()
    {
        if (GameEnemyManager.currentWave == GameEnemyManager.waveConfigurations.Count)
        {
            int total_score = ((int)(wager * multiplier));
            if (total_score >= quota)
            {
                Win();
            }
            else
            {
                Death();
            }
        }
        if (cardIsOnCD)
        {
            ApplyCooldown();
        }
        updateWager();
    }

    public void useCard()
    {
        if (cardIsOnCD)
        { //don't do anything if the card is on CD
            return;
        }
        else
        {
            cardIsOnCD = true;
            cardCDTimer = cardCDTime;
            Card card = deckController.infinDrawCard(deckController.currentDeck);
            StartCoroutine(playCardSound(card));
            card.use(this);
            CooldownImg.sprite = card.cardImage;
            StatusEffectManager.AddStatusEffect(card.effectImage);
        }
    }

    public void ApplyCooldown()
    {
        cardCDTimer -= Time.deltaTime;

        if (cardCDTimer < 0)
        {
            cardIsOnCD = false;
            cardCDTimer = 0;
            UICard.GetComponentInChildren<TextMeshProUGUI>().text = " ";
            audioSource.clip = DeckShuffle;
            audioSource.Play();
        }
        else
        {
            UICard.GetComponentInChildren<TextMeshProUGUI>().text = Mathf.RoundToInt(cardCDTimer).ToString();
            CooldownImg.fillAmount = cardCDTimer / cardCDTime;
        }
    }

    IEnumerator playCardSound(Card card)
    {
        audioSource.clip = CardPullAudio;
        audioSource.Play();
        yield return new WaitForSeconds(CardPullAudio.length);
        if (card.cardType == CardType.Multiplier || card.cardType == CardType.PlayerBuff)
        {
            audioSource.clip = GoodPullAudio;
        }
        else
        {
            audioSource.clip = BadPullAudio;
        }
        audioSource.Play();
    }

    public void freeze(bool status)
    {
        if (status)
        {
            playerController.SetControls(false);
            Time.timeScale = 0f;
        } 
        else
        {
            playerController.SetControls(true);
            Time.timeScale = 1f;
        }   
    }
    public void updateWager()
    {
        int total_score = ((int)(wager * multiplier));
        if (wager_text != null)
        {
            wager_text.text = "Wager:" + total_score.ToString();
        }
    }
    public void Wager()
    {
        freeze(true);
        PlayScreen.SetActive(false);
        WagerScreen.SetActive(true);
    }
    public void WagerChoice(int value)
    {
        freeze(false);
        wager = value;
        PlayScreen.SetActive(true);
        WagerScreen.SetActive(false);
    }
    public void Pause()
    {
        if (!paused)
        {
            // pause
            freeze(true);
            paused = true;
            PlayScreen.SetActive(false);
            PauseScreen.SetActive(true);
        }
        else
        {
            // unpause
            freeze(false);
            paused = false;
            PlayScreen.SetActive(true);
            PauseScreen.SetActive(false);
        }
    }
    public void Death()
    {
        playerController.SetControls(false);
        // animate death here
        int final_payout = (int)(wager * multiplier - quota);
        DeathScreen.SetActive(true);
        TextMeshProUGUI ScoreText = DeathScreen.GetComponentInChildren<TextMeshProUGUI>();
        ScoreText.text = "Final Payout: " + final_payout.ToString();
    }
    public void Win()
    {
        playerController.SetControls(false);
        // animate win here (gangnam style)
        int final_payout = (int)(wager * multiplier - quota);
        WinScreen.SetActive(true);
        TextMeshProUGUI ScoreText = WinScreen.GetComponentInChildren<TextMeshProUGUI>();
        ScoreText.text = "Final Payout: " + final_payout.ToString();
    }
    public void Restart(string scene_name)
    {
        SceneManager.LoadScene(scene_name);
    }
    public void Map()
    {
        SceneManager.LoadScene("MapMenuScene");
    }
    public void Menu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #region FX
    // hit stop (scaling) + screen shake (if strong enough)
    public IEnumerator HitStop(float totalTime) {
        if (!InHitStop) {
            Time.timeScale = 0.01f; // FREEEZE TIME!
            InHitStop = true;
            yield return new WaitForSecondsRealtime(totalTime);
            Time.timeScale = 1.0f; // unfreeze time :(
            InHitStop = false;
        }
    }

    public IEnumerator ScreenShake(float totalTime, float shakeMultiplier) {
        Vector3 startPos = Camera.transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < totalTime) {
            elapsedTime += Time.unscaledDeltaTime;
            float strength = Curve.Evaluate(elapsedTime / totalTime);
            Camera.transform.position = startPos + UnityEngine.Random.insideUnitSphere * strength * shakeMultiplier;
            yield return null;
        }

        Camera.transform.position = startPos;
    }
    #endregion
}