using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfilePopupController : MonoBehaviour
{
    private const string NicknameKey = "profile.nickname";
    private const string AvatarIndexKey = "profile.avatarIndex";
    private const string DefaultNickname = "Player";

    [Header("Popup")]
    [SerializeField] private GameObject profilePopupRoot;
    [SerializeField] private Button openProfileButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private bool hideOnStart = true;

    [Header("Profile UI")]
    [SerializeField] private InputField nicknameInput;
    [SerializeField] private Image selectedAvatarImage;
    [SerializeField] private Button saveButton;

    [Header("Avatar Selection")]
    [SerializeField] private Button[] avatarButtons;

    private int selectedAvatarIndex;

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
        BindButtons();
        LoadProfile();

        if (hideOnStart)
        {
            SetPopupVisible(false);
        }
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

        BindNicknameInputEvents();

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

    private void BindNicknameInputEvents()
    {
        if (nicknameInput == null)
            return;

        EventTrigger eventTrigger = nicknameInput.GetComponent<EventTrigger>();

        if (eventTrigger == null)
        {
            eventTrigger = nicknameInput.gameObject.AddComponent<EventTrigger>();
        }

        eventTrigger.triggers.Clear();

        AddInputEventTrigger(eventTrigger, EventTriggerType.PointerClick);
        AddInputEventTrigger(eventTrigger, EventTriggerType.Select);
    }

    private void AddInputEventTrigger(EventTrigger eventTrigger, EventTriggerType eventType)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType
        };

        entry.callback.AddListener(_ => StartCoroutine(SelectNicknameNextFrame()));
        eventTrigger.triggers.Add(entry);
    }

    public void OpenProfilePopup()
    {
        LoadProfile();
        SetPopupVisible(true);
    }

    public void CloseProfilePopup()
    {
        SetPopupVisible(false);
    }

    private void SetPopupVisible(bool visible)
    {
        if (profilePopupRoot != null)
        {
            profilePopupRoot.SetActive(visible);
        }
    }

    private IEnumerator SelectNicknameNextFrame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        SelectNicknameText();
    }

    private void SelectNicknameText()
    {
        if (nicknameInput == null)
            return;

        if (!nicknameInput.gameObject.activeInHierarchy)
            return;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(nicknameInput.gameObject);
        }

        nicknameInput.ActivateInputField();

        int length = nicknameInput.text.Length;

        nicknameInput.caretPosition = length;
        nicknameInput.selectionAnchorPosition = 0;
        nicknameInput.selectionFocusPosition = length;

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
        string nickname = nicknameInput != null ? nicknameInput.text.Trim() : "";

        if (string.IsNullOrEmpty(nickname))
        {
            nickname = DefaultNickname;
        }

        PlayerPrefs.SetString(NicknameKey, nickname);
        PlayerPrefs.SetInt(AvatarIndexKey, selectedAvatarIndex);
        PlayerPrefs.Save();

        if (nicknameInput != null)
        {
            nicknameInput.text = nickname;
        }

        Debug.Log($"[ProfilePopupController] Saved profile. Nickname: {nickname}, Avatar: {selectedAvatarIndex}");
    }

    public void LoadProfile()
    {
        string nickname = PlayerPrefs.GetString(NicknameKey, "");

        if (nicknameInput != null)
        {
            nicknameInput.text = nickname;
        }

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