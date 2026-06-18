using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Standalone (Zenject-free) port of the original UILocalizedText for the demo project.
// Keeps the SAME GUID and serialized fields (m_localizationKey, m_uppercase) as the
// original so prefab components bind without re-wiring. Localization is loaded directly
// from Resources/GameData/localization.csv instead of a DI-injected LocalizationManager.
public class UILocalizedText : Text
{
    [SerializeField] private string m_localizationKey;
    [SerializeField] private bool m_uppercase;

    private object[] m_param;

    public override string text
    {
        get { return base.text; }
        set
        {
            if (value != null && value.StartsWith("#"))
                SetKey(value);
            else
            {
                m_localizationKey = null;
                m_param = null;
                base.text = value;
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateText();
    }

    public void UpdateText()
    {
        if (string.IsNullOrEmpty(m_localizationKey))
            return;

        var value = DemoLocalization.Localize(m_localizationKey);

        if (m_param != null && m_param.Length > 0)
        {
            value = string.Format(value, m_param);
        }

        if (m_uppercase) value = value.ToUpper();

        base.text = value;
    }

    public void SetKey(string key)
    {
        m_localizationKey = key;
        m_param = null;
        UpdateText();
    }

    public void SetKey(string key, params object[] param)
    {
        if (param == null || param.Length == 0)
        {
            SetKey(key);
            return;
        }
        m_localizationKey = key;
        m_param = param;
        UpdateText();
    }
}

// Lightweight localization store for the demo: parses localization.csv once and serves
// translations by key. The full game uses LocalizationManager/LocalizationLoader instead.
public static class DemoLocalization
{
    public enum Language { English = 3, Russian = 4, German = 5, Spanish = 6, Portuguese = 7, Japanese = 8, Korean = 9 }

    // Change this to switch the rendered language across all UILocalizedText components.
    public static Language language = Language.English;

    private const string ResourcePath = "GameData/localization";

    private static Dictionary<string, string[]> s_rows; // key -> raw columns

    public static string Localize(string key)
    {
        EnsureLoaded();
        if (s_rows != null && s_rows.TryGetValue(key, out var cols))
        {
            var idx = (int)language;
            if (idx < cols.Length && !string.IsNullOrEmpty(cols[idx]))
                return Unescape(cols[idx]);
            // fall back to English when the chosen language is empty
            if ((int)Language.English < cols.Length && !string.IsNullOrEmpty(cols[(int)Language.English]))
                return Unescape(cols[(int)Language.English]);
        }
        return key; // missing key — show it so it is visible in the demo
    }

    // Mirrors LocalizationLoader: literal "\n" in the CSV becomes a real line break.
    private static string Unescape(string value)
    {
        return value.Replace("\\n", "\n");
    }

    private static void EnsureLoaded()
    {
        if (s_rows != null)
            return;

        s_rows = new Dictionary<string, string[]>();

        var asset = Resources.Load<TextAsset>(ResourcePath);
        if (asset == null)
        {
            Debug.LogWarning($"[DemoLocalization] localization.csv not found at Resources/{ResourcePath}");
            return;
        }

        foreach (var row in ParseCsv(asset.text))
        {
            // Raw columns: 0=Area, 1=Type, 2=Key, 3=English, 4=Russian, ...
            if (row.Count < 4) continue;
            var key = row[2];
            if (string.IsNullOrEmpty(key) || !key.StartsWith("#")) continue;
            s_rows[key] = row.ToArray();
        }
    }

    // RFC-4180-ish CSV parser: handles quoted fields, escaped quotes ("") and
    // commas/newlines inside quotes.
    private static IEnumerable<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var field = new System.Text.StringBuilder();
        var current = new List<string>();
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { current.Add(field.ToString()); field.Clear(); }
                else if (c == '\r') { /* ignore */ }
                else if (c == '\n')
                {
                    current.Add(field.ToString()); field.Clear();
                    rows.Add(current); current = new List<string>();
                }
                else field.Append(c);
            }
        }

        // flush last field/row
        if (field.Length > 0 || current.Count > 0)
        {
            current.Add(field.ToString());
            rows.Add(current);
        }

        return rows;
    }
}
