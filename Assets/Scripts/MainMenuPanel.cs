using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主菜单/登录面板
/// 账号密码登录，新玩家自动注册，老玩家读取存档
/// </summary>
public class MainMenuPanel : MonoBehaviour
{
    [Header("登录UI")]
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text feedbackText;
    [SerializeField] private Text titleText;

    public static string PlayerName => AccountManager.CurrentUsername;
    public static bool GameStarted { get; private set; }

    private void Awake()
    {
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLogin);
        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegister);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (usernameInput != null) usernameInput.Select();
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Enter 键快速登录
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (loginButton != null && loginButton.interactable)
                OnLogin();
        }
    }

    private void OnLogin()
    {
        string username = usernameInput?.text?.Trim() ?? "";
        string password = passwordInput?.text ?? "";

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowFeedback("请输入账号");
            return;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            ShowFeedback("请输入密码");
            return;
        }

        if (AccountManager.Instance == null)
        {
            ShowFeedback("系统初始化失败");
            return;
        }

        AudioEvents.Center.Trigger<string>(AudioEvent.PlaySFX, "UIClick");
        if (AccountManager.Instance.Login(username, password))
        {
            // 老玩家 → 读取存档
            SaveManager.Instance.SetSlotIndex(AccountManager.CurrentSlotIndex);
            SaveManager.Instance.LoadGame(AccountManager.CurrentSlotIndex);
            StartGame();
        }
        else
        {
            ShowFeedback("账号或密码错误");
        }
    }

    private void OnRegister()
    {
        string username = usernameInput?.text?.Trim() ?? "";
        string password = passwordInput?.text ?? "";

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowFeedback("请输入账号");
            return;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            ShowFeedback("请输入密码");
            return;
        }
        if (password.Length < 3)
        {
            ShowFeedback("密码至少3位");
            return;
        }

        if (AccountManager.Instance == null)
        {
            ShowFeedback("系统初始化失败");
            return;
        }

        AudioEvents.Center.Trigger<string>(AudioEvent.PlaySFX, "UIClick");
        if (AccountManager.Instance.Register(username, password))
        {
            // 新玩家 → 直接开始新游戏
            SaveManager.Instance.SetSlotIndex(AccountManager.CurrentSlotIndex);
            StartGame();
        }
        else
        {
            ShowFeedback("账号已存在");
        }
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowFeedback(string msg)
    {
        if (feedbackText != null)
        {
            feedbackText.text = msg;
            feedbackText.gameObject.SetActive(true);
        }
    }

    private void StartGame()
    {
        GameStarted = true;
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        AudioEvents.Center.Trigger<string>(AudioEvent.PlayBGM, "Spring");
        PlayerEvents.Center.Trigger(PlayerEvent.GameStarted);
    }
}
