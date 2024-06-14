using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Update()
    {
        scoreText.text = DeliveryManager.Instance.GetSuccessfulRecipesAmount().ToString();
    }
}
