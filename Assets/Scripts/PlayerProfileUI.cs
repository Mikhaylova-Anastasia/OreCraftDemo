using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfileUI : MonoBehaviour
{
    private const string NicknameKey = "player_nickname";
    private const string AvatarKey = "player_avatar_id";

    [Header("Main Screen")]
    [SerializeField] private Image profileButtonImage;

    [Header("Profile Popup")]
    [SerializeField] private GameObject profilePopup;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Image currentAvatarImage;

    [Header("Leaderboard Screen")]
    [SerializeField] private GameObject leaderboardScreen;
    [SerializeField] private Image leaderboardPlayerAvatar;
    [SerializeField] private TMP_Text leaderboardPlayerNickname;

    [Header("Event Screen")]
    [SerializeField] private GameObject eventScreen;
    [SerializeField] private Image eventPlayerAvatar;
    [SerializeField] private TMP_Text eventPlayerNickname;

    [Header("Avatars")]
    [SerializeField] private Sprite[] avatarSprites;

    private int selectedAvatarId;

    private void Start()
    {
        LoadProfile();

        CloseProfilePopup();
        CloseLeaderboardScreen();
        CloseEventScreen();
    }

    public void OpenProfilePopup()
    {
        selectedAvatarId = PlayerPrefs.GetInt(AvatarKey, 0);

        if (nicknameInput != null)
            nicknameInput.text = PlayerPrefs.GetString(NicknameKey, "Player");

        UpdateAvatarPreview();

        if (profilePopup != null)
            profilePopup.SetActive(true);
    }

    public void CloseProfilePopup()
    {
        if (profilePopup != null)
            profilePopup.SetActive(false);
    }

    public void SelectAvatar(int avatarId)
    {
        if (avatarId < 0 || avatarId >= avatarSprites.Length)
            return;

        selectedAvatarId = avatarId;
        UpdateAvatarPreview();
    }

    public void SaveProfile()
    {
        string nickname = "Player";

        if (nicknameInput != null)
        {
            nickname = nicknameInput.text.Trim();

            if (string.IsNullOrEmpty(nickname))
                nickname = "Player";
        }

        PlayerPrefs.SetString(NicknameKey, nickname);
        PlayerPrefs.SetInt(AvatarKey, selectedAvatarId);
        PlayerPrefs.Save();

        LoadProfile();
        CloseProfilePopup();
    }

    public void OpenLeaderboardScreen()
    {
        UpdateLeaderboardPlayerData();

        if (leaderboardScreen != null)
            leaderboardScreen.SetActive(true);
    }

    public void CloseLeaderboardScreen()
    {
        if (leaderboardScreen != null)
            leaderboardScreen.SetActive(false);
    }

    public void OpenEventScreen()
    {
        UpdateEventPlayerData();

        if (eventScreen != null)
            eventScreen.SetActive(true);
    }

    public void CloseEventScreen()
    {
        if (eventScreen != null)
            eventScreen.SetActive(false);
    }

    private void LoadProfile()
    {
        selectedAvatarId = PlayerPrefs.GetInt(AvatarKey, 0);

        UpdateMainAvatar();
        UpdateLeaderboardPlayerData();
        UpdateEventPlayerData();
    }

    private void UpdateMainAvatar()
    {
        Sprite avatar = GetSelectedAvatar(selectedAvatarId);

        if (profileButtonImage != null && avatar != null)
            profileButtonImage.sprite = avatar;
    }

    private void UpdateAvatarPreview()
    {
        Sprite avatar = GetSelectedAvatar(selectedAvatarId);

        if (currentAvatarImage != null && avatar != null)
            currentAvatarImage.sprite = avatar;
    }

    private void UpdateLeaderboardPlayerData()
    {
        string nickname = PlayerPrefs.GetString(NicknameKey, "Player");
        int avatarId = PlayerPrefs.GetInt(AvatarKey, 0);
        Sprite avatar = GetSelectedAvatar(avatarId);

        if (leaderboardPlayerNickname != null)
            leaderboardPlayerNickname.text = nickname;

        if (leaderboardPlayerAvatar != null && avatar != null)
            leaderboardPlayerAvatar.sprite = avatar;
    }

    private void UpdateEventPlayerData()
    {
        string nickname = PlayerPrefs.GetString(NicknameKey, "Player");
        int avatarId = PlayerPrefs.GetInt(AvatarKey, 0);
        Sprite avatar = GetSelectedAvatar(avatarId);

        if (eventPlayerNickname != null)
            eventPlayerNickname.text = nickname;

        if (eventPlayerAvatar != null && avatar != null)
            eventPlayerAvatar.sprite = avatar;
    }

    private Sprite GetSelectedAvatar(int avatarId)
    {
        if (avatarSprites == null || avatarSprites.Length == 0)
            return null;

        if (avatarId < 0 || avatarId >= avatarSprites.Length)
            avatarId = 0;

        return avatarSprites[avatarId];
    }
}