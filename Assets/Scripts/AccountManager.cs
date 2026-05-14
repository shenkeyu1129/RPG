using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// 账户数据
/// </summary>
[System.Serializable]
public class AccountData
{
    public string username;
    public string passwordHash;
    public int saveSlotIndex;
}

/// <summary>
/// 账户数据库
/// </summary>
[System.Serializable]
public class AccountDatabase
{
    public List<AccountData> accounts = new();
    public int nextSlotIndex;
}

/// <summary>
/// 账户管理器单例
/// 管理注册、登录、账户持久化
/// </summary>
public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }

    public static string CurrentUsername { get; private set; }
    public static int CurrentSlotIndex { get; private set; }

    private const string ACCOUNT_FILE = "accounts.json";
    private AccountDatabase _database;

    private string FilePath => $"{Application.persistentDataPath}/{ACCOUNT_FILE}";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadDatabase();
    }

    /// <summary>
    /// 注册新账户
    /// </summary>
    public bool Register(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return false;

        // 检查用户名是否已存在
        foreach (var acc in _database.accounts)
        {
            if (acc.username == username)
                return false;
        }

        var newAccount = new AccountData
        {
            username = username,
            passwordHash = HashPassword(password),
            saveSlotIndex = _database.nextSlotIndex++
        };

        _database.accounts.Add(newAccount);
        SaveDatabase();

        CurrentUsername = username;
        CurrentSlotIndex = newAccount.saveSlotIndex;
        return true;
    }

    /// <summary>
    /// 登录已有账户
    /// </summary>
    public bool Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;

        foreach (var acc in _database.accounts)
        {
            if (acc.username == username)
            {
                if (acc.passwordHash == HashPassword(password))
                {
                    CurrentUsername = username;
                    CurrentSlotIndex = acc.saveSlotIndex;
                    return true;
                }
                return false; // 密码错误
            }
        }
        return false; // 用户名不存在
    }

    /// <summary>
    /// 检查用户名是否已存在
    /// </summary>
    public bool UsernameExists(string username)
    {
        foreach (var acc in _database.accounts)
        {
            if (acc.username == username) return true;
        }
        return false;
    }

    private void LoadDatabase()
    {
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            _database = JsonUtility.FromJson<AccountDatabase>(json);
        }
        if (_database == null)
            _database = new AccountDatabase();
    }

    private void SaveDatabase()
    {
        string json = JsonUtility.ToJson(_database, true);
        File.WriteAllText(FilePath, json);
    }

    /// <summary>
    /// 对密码进行哈希（SHA256）
    /// </summary>
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
