using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public UIbase Main;
    public UIbase Start_;
    public UIbase Shop;
    public UIbase Update;
    public UIbase Shooter_SkillTree;
    public UIbase Producer_SkillTree;
    public Pick_UI Pick;

    public void OpenPick(string Scene_name)
    {
        Pick.Scene_Name = Scene_name;
        UIManager.Instance.OpenNewUI(Pick);
    }
    public void Shop_refresh()
    {
        Shop.Refresh();
    }
    public void all_refresh()
    {
        Shooter_SkillTree.Refresh();
        Producer_SkillTree.Refresh();
    }
    private void Start()
    {
        UIManager.Instance.OpenNewUI(Main);
    }
    public void OpenStart()
    {
        UIManager.Instance.OpenNewUI(Start_);
    }
    public void OpenShop()
    {
        UIManager.Instance.OpenNewUI(Shop);
    }
    public void OpenUpdate()
    {
        UIManager.Instance.OpenNewUI(Update);
    }
    public void OpenSkillTree(string name)
    {
        if (name == "Shooter")
            UIManager.Instance.OpenNewUI(Shooter_SkillTree);

        if (name == "Producer")
            UIManager.Instance.OpenNewUI(Producer_SkillTree);
    }
}
