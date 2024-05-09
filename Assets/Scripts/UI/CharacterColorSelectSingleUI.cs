using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectGameObject;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.ChangedPlayerColor(colorId);
        });
    }

    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        image.color =  KitchenGameMultiplayer.Instance.GetPlayerColor(colorId);
        UpdateSelected();
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateSelected();
    }

    private void UpdateSelected()
    {
        if(KitchenGameMultiplayer.Instance.GetPlayerData().colorId == colorId)
        {
            selectGameObject.SetActive(true);
        }
        else
        {
            selectGameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
