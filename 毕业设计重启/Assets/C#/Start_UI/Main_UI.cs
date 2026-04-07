using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Main_UI : UIbase
{
    public TextMeshProUGUI EXP_value;
    public TextMeshProUGUI Coin_value;

    public override void Refresh()
    {
        base.Refresh();

        EXP_value.text = GameManager.Instance.exp.ToString();
        Coin_value.text = GameManager.Instance.coin.ToString();
    }
}
