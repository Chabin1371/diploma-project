using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Producer : Tower_Controller
{
    public bool skill_2001 = false;
    public int Coin_get_for_skill_2001 = 5; 
    public float Coin_get_for_skill_2001_every_time = 10f;
    public override void Start_()
    {
        base.Start_();
        is_working = true;
        GameManager_in_game.Instance.current_energy += GameManager_in_game.Instance.Towers[ID].EnergyProduce;
    }
    public override void Dead()
    {
        base.Dead();
        GameManager_in_game.Instance.current_energy -= GameManager_in_game.Instance.Towers[ID].EnergyProduce;
    }
    private void Update()
    {
        if (skill_2001)
            StartCoroutine(Money_plus());
    }

    IEnumerator Money_plus()
    {
        yield return new WaitForSeconds(Coin_get_for_skill_2001_every_time);
        GameManager_in_game.Instance.Coin_get += Coin_get_for_skill_2001;
    }
}
