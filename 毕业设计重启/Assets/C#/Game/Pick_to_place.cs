using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pick_to_place : MonoBehaviour, IPointerClickHandler
{
    public static event System.Action<Pick_to_place> OnAnySelected;

    [SerializeField] private Image selected_UI;
    private bool is_selected = false;

    [HideInInspector] public Tower Tower;
    void OnEnable()
    {
        // 订阅静态事件
        OnAnySelected += HandleOtherSelected;
    }

    void OnDisable()
    {
        // 取消订阅，防止内存泄漏
        OnAnySelected -= HandleOtherSelected;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnAnySelected?.Invoke(this);
        selected_UI.gameObject.SetActive(true);

        GameManager_in_game.Instance.mouse.is_selected_any_grid = true;

        GameManager_in_game.Instance.mouse.highlightTile = Tower.Tile;
        GameManager_in_game.Instance.mouse.current_Tower = Tower;
    }

    private void HandleOtherSelected(Pick_to_place selectedObj)
    {
        if (selectedObj == this)
        {
            // 自己是选中者 → 设为选中状态
            SetSelected(true);
        }
        else
        {
            // 其他物体被选中 → 自己取消选中
            SetSelected(false);
        }
    }

    private void SetSelected(bool selected)
    {
        if (is_selected == selected) return; // 避免重复操作
        selected_UI.gameObject.SetActive(selected);
        is_selected = selected;
    }
}
