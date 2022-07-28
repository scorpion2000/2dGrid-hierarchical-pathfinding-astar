using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI progressText;

    int neededProgress = 0;
    int currentProgress = 0;

    public int SetNeededProgress { set { neededProgress = value; } }

    public void AddToProgress(int amount)
    {
        currentProgress += amount;
        progressText.text = currentProgress + "/" + neededProgress;
    }
}
