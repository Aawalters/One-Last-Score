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

    private TextMeshProUGUI wager_text;
    private TextMeshProUGUI quota_text; 

    private GameObject PlayScreen;
    private GameObject WagerScreen;
    private GameObject PauseScreen;
    private GameObject DeathScreen;
    private GameObject WinScreen;
    private GameObject IBuild;

    private bool paused = false;

    void Awake()
    {
        PlayScreen = Canvas.transform.Find("Play Screen").gameObject;
        WagerScreen = Canvas.transform.Find("Wager Screen").gameObject;
        PauseScreen = Canvas.transform.Find("Pause Screen").gameObject;
        DeathScreen = Canvas.transform.Find("Death Screen").gameObject;
        WinScreen = Canvas.transform.Find("Win Screen").gameObject;
        IBuild = Canvas.transform.Find("InclusivityBuildPanel").gameObject;

        playerController = Player.GetComponent<PlayerController>();
        wager_text = PlayScreen.transform.Find("Wager").gameObject.GetComponent<TextMeshProUGUI>();
        quota_text = PlayScreen.transform.Find("Quota").gameObject.GetComponent<TextMeshProUGUI>();
        wager_text.text = "Wager:" + wager.ToString();

        audioSource = GetComponent<AudioSource>();
        
        UICard = PlayScreen.transform.Find("Card").gameObject.GetComponent<Image>();
        CooldownImg = PlayScreen.transform.Find("Card").gameObject.GetComponentInChildren<Image>();
        StatusEffectManager = PlayScreen.transform.Find("Card").GetComponent<StatusEffectManager>();

        healthBar = PlayScreen.GetComponentInChildren<Slider>();

        iOSPanel = IBuild.transform.Find("iOS Panel").gameObject;
    }

    void Update()
    {
        if (GameEnemyManager.TotalNumberOfEnemiesLeft <= 0)
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
        wager_text.text = "Wager:" + total_score.ToString();
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
        updateWager();
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
        SceneManager.LoadScene(scene_name); // "DiegoTestingScene"
    }
    public void Map()
    {
        SceneManager.LoadScene("MapMenuScene");
    }
    public void Menu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}