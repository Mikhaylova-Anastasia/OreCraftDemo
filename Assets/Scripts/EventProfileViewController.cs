using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EventProfileViewController : MonoBehaviour
{
    [Header("Open / Close")]
    [SerializeField] private Button openEventButton;
    [SerializeField] private GameObject eventFullView;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button claimAllAndLeaveButton;
    [SerializeField] private bool hideOnStart = true;

    [Header("Profile Source")]
    [SerializeField] private ProfilePopupController profileController;

    [Header("Player Info")]
    [SerializeField] private Text playerInfoNameText;
    [SerializeField] private Image playerInfoAvatarImage;

    [Header("Event Player Row")]
    [SerializeField] private Text eventRowNameText;
    [SerializeField] private Image eventRowAvatarImage;

    private void Awake()
    {
        if (openEventButton != null)
        {
            openEventButton.onClick.RemoveAllListeners();
            openEventButton.onClick.AddListener(OpenEvent);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseEvent);
        }

        if (claimAllAndLeaveButton != null)
        {
            claimAllAndLeaveButton.onClick.RemoveAllListeners();
            claimAllAndLeaveButton.onClick.AddListener(ClaimAllAndLeave);
        }

        if (hideOnStart && eventFullView != null)
        {
            eventFullView.SetActive(false);
        }
    }

    public void OpenEvent()
    {
        if (eventFullView != null)
        {
            eventFullView.SetActive(true);
        }

        ApplyProfileToEvent();
        StartCoroutine(ApplyProfileNextFrame());
    }

    public void CloseEvent()
    {
        if (eventFullView != null)
        {
            eventFullView.SetActive(false);
        }
    }

    public void ClaimAllAndLeave()
    {
        
        Debug.Log("[EventProfileViewController] Claim All And Leave clicked.");

        CloseEvent();
    }

    private IEnumerator ApplyProfileNextFrame()
    {
        yield return null;
        ApplyProfileToEvent();
    }

    public void ApplyProfileToEvent()
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

        if (eventRowNameText != null)
        {
            eventRowNameText.text = nickname;
        }
    }

    private void ApplyAvatar(Sprite avatarSprite)
    {
        if (playerInfoAvatarImage != null)
        {
            playerInfoAvatarImage.sprite = avatarSprite;
            playerInfoAvatarImage.color = Color.white;
        }

        if (eventRowAvatarImage != null)
        {
            eventRowAvatarImage.sprite = avatarSprite;
            eventRowAvatarImage.color = Color.white;
        }
    }
}