using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Grid_Pick : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public Tower tower;
    [HideInInspector] public int tower_id; 

    private bool is_selected = false;

    public GameObject Selected_Ui;
    public Image icon;
    public bool Is_selected => is_selected;

    public void OnPointerClick(PointerEventData eventData)
    {
        is_selected = !is_selected;

        Selected_Ui.SetActive(is_selected);
    }
}
