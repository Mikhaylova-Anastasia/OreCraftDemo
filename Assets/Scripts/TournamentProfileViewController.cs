using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TournamentProfileViewController : MonoBehaviour
{
    [Header("Open / Close")]
    [SerializeField] private Button openRankButton;
    [SerializeField] private GameObject tournamentFullView;
    [SerializeField] private Button backButton;
    [SerializeField] private bool hideOnStart = true;

    [Header("Profile Source")]
    [SerializeField] private ProfilePopupController profileController;

    [Header("Player Info")]
    [FormerlySerializedAs("playerHudNameText")]
    [SerializeField] private Text playerInfoNameText;

    [FormerlySerializedAs("playerHudAvatarImage")]
    [SerializeField] private Image playerInfoAvatarImage;

    [Header("Leaderboard Player Row")]
    [SerializeField] private Text leaderboardNameText;
    [SerializeField] private Image leaderboardAvatarImage;

    private void Awake()
    {
        if (openRankButton != null)
        {
            openRankButton.onClick.RemoveAllListeners();
            openRankButton.onClick.AddListener(OpenTournament);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(CloseTournament);
        }

        if (hideOnStart && tournamentFullView != null)
        {
            tournamentFullView.SetActive(false);
        }
    }

    public void OpenTournament()
    {
        if (tournamentFullView != null)
        {
            tournamentFullView.SetActive(true);
        }

        ApplyProfileToTournament();
        StartCoroutine(ApplyProfileNextFrame());
    }

    public void CloseTournament()
    {
        if (tournamentFullView != null)
        {
            tournamentFullView.SetActive(false);
        }
    }

    private IEnumerator ApplyProfileNextFrame()
    {
        yield return null;
        ApplyProfileToTournament();
    }

    public void ApplyProfileToTournament()
    {
        string nickname = ProfilePopupController.GetSavedNickname();

        if (string.IsNullOrWhiteSpace(nickname))
        {
            nickname = "Player";
        }

        ApplyNickname(nickname);

        if (profileController == null)
            return;

        Sprite savedAvatar = profileController.GetSavedAvatarSprite();

        if (savedAvatar == null)
            return;

        ApplyAvatar(savedAvatar);
    }

    private void ApplyNickname(string nickname)
    {
        if (playerInfoNameText != null)
        {
            playerInfoNameText.text = nickname;
        }

        if (leaderboardNameText != null)
        {
            leaderboardNameText.text = nickname;
        }
    }

    private void ApplyAvatar(Sprite avatarSprite)
    {
        if (playerInfoAvatarImage != null)
        {
            playerInfoAvatarImage.sprite = avatarSprite;
            playerInfoAvatarImage.color = Color.white;
        }

        if (leaderboardAvatarImage != null)
        {
            leaderboardAvatarImage.sprite = avatarSprite;
            leaderboardAvatarImage.color = Color.white;
        }
    }
}