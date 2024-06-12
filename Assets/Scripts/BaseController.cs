using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BaseController : MonoBehaviour
{
    [SerializeField] private static int[] minerals = new int[4];
    [SerializeField] private static int totalCost = 0;

    [SerializeField] private Text Coal;
    [SerializeField] private Text Iron;
    [SerializeField] private Text Gold;
    [SerializeField] private Text Diamond;
    [SerializeField] private Text TotalCost;

    private string nextLevel = "Ending";

    public void Start()
    {
        if (SceneManager.GetActiveScene().name == "Base")
        {
            if (totalCost >= 10000) MoveToNextLevel();
            UIUpdate();
        }
    }

    public void PlusMinerals(int[] miningMineral)
    {
        for (int i = 0; i < 4; i++) minerals[i] += miningMineral[i];
        CalcCost();
    }

    private void CalcCost()
    {
        int[] mineralCost = {
            10, 50, 200, 5000
        };
        totalCost = 0;
        for (int i = 0; i < 4; i++) totalCost += mineralCost[i] * minerals[i];
    }

    private void UIUpdate()
    {
        Coal.text = "Coal : " + minerals[0].ToString();
        Iron.text = "Iron : " + minerals[1].ToString();
        Gold.text = "Gold : " + minerals[2].ToString();
        Diamond.text = "Diamond : " + minerals[3].ToString();
        TotalCost.text = "TotalCost : " + totalCost.ToString();
    }

    private void MoveToNextLevel()
    {
        SceneManager.LoadScene(nextLevel);
    }

    public int GetTotalCost()
    {
        return totalCost;
    }
}
