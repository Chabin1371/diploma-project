using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pick_UI : UIbase
{
    public GameObject Grid;
    public GameObject Grid_Parent;
    public string Scene_Name;

    List<Grid_Pick> pick_towers = new List<Grid_Pick>();

    public override void Refresh()
    {
        base.Refresh();

        if (pick_towers.Count == GameManager.Instance.Towers_dict.Count)
            return;

        foreach (var tower in GameManager.Instance.Towers_dict)
        {
            GameObject tower_Grid = Instantiate(Grid, Grid_Parent.transform);
            Grid_Pick grid = tower_Grid.GetComponent<Grid_Pick>();
            grid.icon.sprite = tower.Value.Icon;
            grid.tower = tower.Value;
            grid.tower_id = tower.Key;
            pick_towers.Add(grid);
        }
    }

    public void Start_the_game()
    {
        GameManager.Instance.Keeper.TowerList.Clear();
        foreach (var Grid in pick_towers)
        {
            GameManager.Instance.Keeper.TowerList.Add(Grid.tower);
        }
        StartCoroutine(LoadYourAsyncScene(Scene_Name));
    }
    IEnumerator LoadYourAsyncScene(string sceneName)
    {
        // 异步加载场景，不会阻塞主线程
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 等待加载完成
        while (!asyncLoad.isDone)
        {
            // 可以在这里获取加载进度 asyncLoad.progress
            // 例如，更新UI进度条的数值
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log("加载进度: " + (progress * 100) + "%");

            yield return null; // 等待一帧
        }
    }
}
