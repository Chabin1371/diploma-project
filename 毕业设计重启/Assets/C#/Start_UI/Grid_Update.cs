using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Grid_Update : Grid
{
    public GameObject image;
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        image.SetActive(false);
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        image.SetActive(false);
        UIManager.Instance.MainMenu_C.OpenSkillTree(name_);
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        image.SetActive(true);
    }
}
