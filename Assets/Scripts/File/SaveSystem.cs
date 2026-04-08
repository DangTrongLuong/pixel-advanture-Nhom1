using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string KEY = "MyGameKey123456!"; 
    private static readonly string IV = "GameIV78GameIV78";

    private static string SavePath => Path.Combine(Application.persistentDataPath, "progress.dat");

    // ── SAVE ──────────────────────────────────────────────
    public static void SaveProgress(GameProgress progress)
    {
        try
        {
            string json = JsonUtility.ToJson(progress);
            string encrypted = Encrypt(json);
            File.WriteAllText(SavePath, encrypted);
            Debug.Log("[Save] Đã lưu: " + SavePath);
        }
        catch (Exception e)
        {
            Debug.LogError("[Save] Lỗi khi lưu: " + e.Message);
        }
    }

    // ── LOAD ──────────────────────────────────────────────
    public static GameProgress LoadProgress()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[Save] Chưa có file save → tạo mới.");
                return new GameProgress();
            }

            string encrypted = File.ReadAllText(SavePath);
            string json = Decrypt(encrypted);
            return JsonUtility.FromJson<GameProgress>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Save] Lỗi khi đọc file → reset. " + e.Message);
            return new GameProgress();
        }
    }

    // ── UNLOCK MAP ────────────────────────────────────────
    public static void UnlockMap(int mapIndex)
    {
        GameProgress p = LoadProgress();
        if (mapIndex > p.highestUnlockedMap)
        {
            p.highestUnlockedMap = mapIndex;
            SaveProgress(p);
            Debug.Log("[Save] Đã unlock đến Map" + mapIndex);
        }
    }

    // ── AES ENCRYPT ───────────────────────────────────────
    private static string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(KEY);
            aes.IV = Encoding.UTF8.GetBytes(IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor())
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
                sw.Close();
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    // ── AES DECRYPT ───────────────────────────────────────
    private static string Decrypt(string cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(KEY);
            aes.IV = Encoding.UTF8.GetBytes(IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] buffer = Convert.FromBase64String(cipherText);
            using (var decryptor = aes.CreateDecryptor())
            using (var ms = new MemoryStream(buffer))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}