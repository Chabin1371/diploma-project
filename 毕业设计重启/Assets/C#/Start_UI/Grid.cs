using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class Grid : MonoBehaviour, IPointerExitHandler, IPointerClickHandler, IPointerEnterHandler
{
    private string Grid_name;
    public string name_ => Grid_name;
    private Image Grid_icon;
    public void Set_name(string name)
    {
        Grid_name = name;
        GetComponentInChildren<TextMeshProUGUI>().text = name;
    }
    public void Set_icon(Sprite sprite)
    {
        Grid_icon = GetComponent<Image>();
        Grid_icon.sprite = sprite;
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        
    }

}
