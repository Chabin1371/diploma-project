using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIbase : MonoBehaviour
{
    public virtual void OnEnter()
    {
        gameObject.SetActive(true);
    }
    public virtual void OnExit()
    {
        gameObject.SetActive(false);
    }
    public virtual void Refresh()
    {

    }
    public virtual void Back()
    {
        UIManager.Instance.BackToLastUI(this);
    }
}
