using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMultiplayerUI : MonoBehaviour
{
    private void Start()
    {
        KitchenGameManager.Instance.OnLocalMultiplayerGamePaused += KitchenGameManager_OnLocalMultiplayerGamePaused;
        KitchenGameManager.Instance.OnLocalMultiplayerGameUnpaused += KitchenGameManager_OnLocalMultiplayerGameUnpaused;

        Hide();
    }

    private void KitchenGameManager_OnLocalMultiplayerGameUnpaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void KitchenGameManager_OnLocalMultiplayerGamePaused(object sender, System.EventArgs e)
    {
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
