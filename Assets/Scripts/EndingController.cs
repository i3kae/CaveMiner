using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndingController : MonoBehaviour
{
    [SerializeField] Text EarnedCost;
    private void Start()
    {
        BaseController baseController = GameObject.FindAnyObjectByType<BaseController>();
        EarnedCost.text = "You've earned a total of " + baseController.GetTotalCost().ToString() + " Cost";
    }
}
