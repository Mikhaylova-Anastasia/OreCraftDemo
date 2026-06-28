using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfilePopupController : MonoBehaviour
{
    private const string NicknameKey = "profile.nickname";
    private const string AvatarIndexKey = "profile.avatarIndex";
    private const string DefaultNickname = "Player";
    private const int NicknameCharacterLimit = 16;

    [Header("Popup")]
    [SerializeField] private GameObject profilePopupRoot;
    [SerializeField] private Button openProfileButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private bool hideOnStart = true;

    [Header("Profile UI")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Image selectedAvatarImage;
    [SerializeField] private Button saveButton;

    [Header("Avatar Selection")]
    [SerializeField] private Button[] avatarButtons;

    private int selectedAvatarIndex;

#if UNITY_ANDROID || UNITY_IOS
    private TouchScreenKeyboard nicknameKeyboard;
    private bool isNicknameKeyboardOpen;
#endif

    public static string GetSavedNickname()
    {
        return PlayerPrefs.GetString(NicknameKey, DefaultNickname);
    }

    public static int GetSavedAvatarIndex()
    {
        return PlayerPrefs.GetInt(AvatarIndexKey, 0);
    }

    private void Awake()
    {
        ConfigureNicknameInput();
        BindButtons();

        selectedAvatarIndex = GetSavedAvatarIndex();

        if (hideOnStart)
        {
            SetPopupVisible(false);
        }
        else
        {
            StartCoroutine(LoadProfileAfterPopupIsVisible());
        }
    }

    private void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        UpdateNativeNicknameKeyboard();
#endif
    }

    private void ConfigureNicknameInput()
    {
        if (nicknameInput == null)
            return;

        nicknameInput.interactable = true;
        nicknameInput.contentType = TMP_InputField.ContentType.Standard;
        nicknameInput.lineType = TMP_InputField.LineType.SingleLine;
        nicknameInput.characterLimit = NicknameCharacterLimit;
        nicknameInput.shouldHideMobileInput = false;
        nicknameInput.onFocusSelectAll = false;

#if UNITY_ANDROID || UNITY_IOS
        TouchScreenKeyboard.hideInput = false;

        // На мобилке НЕ даём TMP самому редактировать текст.
        // Он теперь только отображает ник, а реальный ввод идёт через TouchScreenKeyboard.Open().
        nicknameInput.readOnly = true;

        BindNativeNicknameClick();
#else
        nicknameInput.readOnly = false;
#endif
    }

    private void BindNativeNicknameClick()
    {
        if (nicknameInput == null)
            return;

        EventTrigger eventTrigger = nicknameInput.GetComponent<EventTrigger>();

        if (eventTrigger == null)
        {
            eventTrigger = nicknameInput.gameObject.AddComponent<EventTrigger>();
        }

        eventTrigger.triggers.Clear();

        EventTrigger.Entry pointerClickEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };

        pointerClickEntry.callback.AddListener(_ => OpenNativeNicknameKeyboard());
        eventTrigger.triggers.Add(pointerClickEntry);
    }

    private void BindButtons()
    {
        if (openProfileButton != null)
        {
            openProfileButton.onClick.RemoveAllListeners();
            openProfileButton.onClick.AddListener(OpenProfilePopup);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseProfilePopup);
        }

        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(SaveProfile);
        }

        if (avatarButtons == null)
            return;

        for (int i = 0; i < avatarButtons.Length; i++)
        {
            int avatarIndex = i;
            Button button = avatarButtons[i];

            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectAvatar(avatarIndex));
        }
    }

    public void OpenProfilePopup()
    {
        SetPopupVisible(true);
        StartCoroutine(LoadProfileAfterPopupIsVisible());
    }

    public void CloseProfilePopup()
    {
#if UNITY_ANDROID || UNITY_IOS
        CloseNativeNicknameKeyboard();
#endif

        ReleaseNicknameInputFocus();
        SetPopupVisible(false);
    }

    private void SetPopupVisible(bool visible)
    {
        if (profilePopupRoot != null)
        {
            profilePopupRoot.SetActive(visible);
        }
    }

    private IEnumerator LoadProfileAfterPopupIsVisible()
    {
        yield return null;

        LoadProfile();
        ReleaseNicknameInputFocus();
    }

    private void ReleaseNicknameInputFocus()
    {
        if (nicknameInput != null)
        {
            nicknameInput.DeactivateInputField();
        }

        if (EventSystem.current != null &&
            nicknameInput != null &&
            EventSystem.current.currentSelectedGameObject == nicknameInput.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

#if UNITY_ANDROID || UNITY_IOS
    private void OpenNativeNicknameKeyboard()
    {
        if (nicknameInput == null)
            return;

        TouchScreenKeyboard.hideInput = false;

        string currentNickname = nicknameInput.text;

        if (string.IsNullOrEmpty(currentNickname))
        {
            currentNickname = PlayerPrefs.GetString(NicknameKey, "");
        }

        nicknameKeyboard = TouchScreenKeyboard.Open(
            currentNickname,
            TouchScreenKeyboardType.Default,
            false,
            false,
            false,
            false,
            "Enter nickname",
            NicknameCharacterLimit
        );

        isNicknameKeyboardOpen = true;
    }

    private void UpdateNativeNicknameKeyboard()
    {
        if (!isNicknameKeyboardOpen || nicknameKeyboard == null)
            return;

        string keyboardText = nicknameKeyboard.text ?? "";

        if (keyboardText.Length > NicknameCharacterLimit)
        {
            keyboardText = keyboardText.Substring(0, NicknameCharacterLimit);
        }

        ApplyNicknameTextToField(keyboardText);

        if (nicknameKeyboard.status == TouchScreenKeyboard.Status.Done ||
            nicknameKeyboard.status == TouchScreenKeyboard.Status.Canceled ||
            nicknameKeyboard.status == TouchScreenKeyboard.Status.LostFocus)
        {
            isNicknameKeyboardOpen = false;
            nicknameKeyboard = null;
            ReleaseNicknameInputFocus();
        }
    }

    private void CloseNativeNicknameKeyboard()
    {
        if (nicknameKeyboard != null)
        {
            nicknameKeyboard.active = false;
            nicknameKeyboard = null;
        }

        isNicknameKeyboardOpen = false;
    }
#endif

    private void ApplyNicknameTextToField(string nickname)
    {
        if (nicknameInput == null)
            return;

        nicknameInput.SetTextWithoutNotify(nickname);
        nicknameInput.ForceLabelUpdate();
    }

    public void SelectAvatar(int avatarIndex)
    {
        if (avatarButtons == null || avatarIndex < 0 || avatarIndex >= avatarButtons.Length)
        {
            Debug.LogWarning($"[ProfilePopupController] Avatar index {avatarIndex} is out of range.");
            return;
        }

        Sprite avatarSprite = GetAvatarSpriteFromButton(avatarButtons[avatarIndex]);

        if (avatarSprite == null)
        {
            Debug.LogWarning($"[ProfilePopupController] Avatar sprite not found on button {avatarIndex}.");
            return;
        }

        selectedAvatarIndex = avatarIndex;

        if (selectedAvatarImage != null)
        {
            selectedAvatarImage.sprite = avatarSprite;
        }

        Debug.Log($"[ProfilePopupController] Selected avatar: {selectedAvatarIndex}");
    }

    public void SaveProfile()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (isNicknameKeyboardOpen && nicknameKeyboard != null)
        {
            ApplyNicknameTextToField(nicknameKeyboard.text ?? "");
            CloseNativeNicknameKeyboard();
        }
#endif

        string nickname = nicknameInput != null ? nicknameInput.text.Trim() : "";

        if (string.IsNullOrEmpty(nickname))
        {
            nickname = DefaultNickname;
        }

        PlayerPrefs.SetString(NicknameKey, nickname);
        PlayerPrefs.SetInt(AvatarIndexKey, selectedAvatarIndex);
        PlayerPrefs.Save();

        ApplyNicknameTextToField(nickname);
        ReleaseNicknameInputFocus();

        Debug.Log($"[ProfilePopupController] Saved profile. Nickname: {nickname}, Avatar: {selectedAvatarIndex}");
    }

    public void LoadProfile()
    {
        string nickname = PlayerPrefs.GetString(NicknameKey, "");

        ApplyNicknameTextToField(nickname);

        int maxAvatarIndex = avatarButtons != null && avatarButtons.Length > 0
            ? avatarButtons.Length - 1
            : 0;

        selectedAvatarIndex = Mathf.Clamp(PlayerPrefs.GetInt(AvatarIndexKey, 0), 0, maxAvatarIndex);

        if (avatarButtons != null && avatarButtons.Length > selectedAvatarIndex)
        {
            Sprite avatarSprite = GetAvatarSpriteFromButton(avatarButtons[selectedAvatarIndex]);

            if (avatarSprite != null && selectedAvatarImage != null)
            {
                selectedAvatarImage.sprite = avatarSprite;
            }
        }
    }

    public Sprite GetAvatarSpriteByIndex(int avatarIndex)
    {
        if (avatarButtons == null || avatarButtons.Length == 0)
            return null;

        avatarIndex = Mathf.Clamp(avatarIndex, 0, avatarButtons.Length - 1);

        return GetAvatarSpriteFromButton(avatarButtons[avatarIndex]);
    }

    public Sprite GetSavedAvatarSprite()
    {
        return GetAvatarSpriteByIndex(GetSavedAvatarIndex());
    }

    private Sprite GetAvatarSpriteFromButton(Button button)
    {
        if (button == null)
            return null;

        Transform iconTransform = button.transform.Find("AvatarIcon");

        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();

            if (iconImage != null)
                return iconImage.sprite;
        }

        Image fallbackImage = button.GetComponent<Image>();

        if (fallbackImage != null)
            return fallbackImage.sprite;

        return null;
    }
}