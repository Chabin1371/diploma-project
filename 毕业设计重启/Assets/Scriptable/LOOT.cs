using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Loot", menuName = "Game/Loot")]
public class LOOT : ScriptableObject
{
    [SerializeField] private int EXP_value;
    [SerializeField] private int Coin_value;

    public void spend_coin(int coin)
    {
        Coin_value -= coin;
    }

    public void spend_exp(int exp)
    {
        EXP_value -= exp;
    }
    public int EXP => EXP_value;
    public int Coin => Coin_value;
}
