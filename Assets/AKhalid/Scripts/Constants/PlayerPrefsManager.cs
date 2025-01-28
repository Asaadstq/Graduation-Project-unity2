using UnityEngine;
namespace Constants
{
public static class PlayerPrefsManager
{
    /// <summary>
    /// Saves a string value to PlayerPrefs.
    /// </summary>
    public static void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save(); // Ensure the changes are saved
    }

    /// <summary>
    /// Retrieves a string value from PlayerPrefs. Returns an empty string if the key does not exist.
    /// </summary>
    public static string GetString(string key, string defaultValue = "")
    {
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : defaultValue;
    }

    /// <summary>
    /// Deletes a specific key from PlayerPrefs.
    /// </summary>
    public static void DeleteKey(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Checks if a key exists in PlayerPrefs.
    /// </summary>
    public static bool KeyExists(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    /// <summary>
    /// Clears all saved keys from PlayerPrefs.
    /// </summary>
    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
}