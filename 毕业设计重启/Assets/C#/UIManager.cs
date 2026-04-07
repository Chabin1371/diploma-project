using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("主菜单生成")]
    public GameObject MainMenu;
    private GameObject mainMenu;
    private MainMenu mainMenu_C;
    public MainMenu MainMenu_C => mainMenu_C;
    [Header("UI管理")]
    private Stack<UIbase> _uiStack = new Stack<UIbase>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        mainMenu = Instantiate(MainMenu);
        mainMenu_C = mainMenu.GetComponent<MainMenu>();
    }
    //打开新UI界面
    public void OpenNewUI(UIbase ui)
    {
        if (_uiStack.Count > 0)
            // 当前界面
            _uiStack.Peek().OnExit();

        _uiStack.Push(ui);
        ui.OnEnter();
        ui.Refresh();
    }
    //返回上一级UI
    public void BackToLastUI(UIbase ui)
    {
        ui.OnExit();
        _uiStack.Pop();
        _uiStack.Peek().OnEnter();
        _uiStack.Peek().Refresh();
    }
}
