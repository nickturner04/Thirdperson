using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.InputSystem.DualShock;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    private VisualElement vseCharSelect;
    private VisualElement vseButtons;
    private VisualElement vseMission;

    private Button btnChar1;
    private Button btnChar2;

    private Stack<VisualElement> openElements = new();

    private void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        vseCharSelect = document.rootVisualElement.Q("vseCharSelect");
        vseMission = document.rootVisualElement.Q("vseMission");
        vseMission.SetEnabled(false);
        vseMission.style.display = DisplayStyle.None;
        vseCharSelect.style.display = DisplayStyle.None;
        vseCharSelect.SetEnabled(false);
        vseButtons = document.rootVisualElement.Q("vseButtons");
        document.rootVisualElement.Q<Button>("btnNewGame").clicked += NewGame;
        document.rootVisualElement.Q<Button>("btnM1").clicked += StartGame;
        document.rootVisualElement.Q<Button>("btnQuit").clicked += Application.Quit;
        document.rootVisualElement.Q<Button>("btnQuit").clicked += Application.Quit;
        btnChar1 = document.rootVisualElement.Q<Button>("btnChar1");
        btnChar2 = document.rootVisualElement.Q<Button>("btnChar2");
        btnChar1.clicked += SetChar1;
        btnChar2.clicked += SetChar2;
    }
    public void NewGame()
    {
        vseCharSelect.style.display = DisplayStyle.Flex;
        vseCharSelect.SetEnabled(true);
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    private void SetChar1()
    {
        GameData.chosencharacter = 0;
        btnChar2.style.opacity = .5f;
        btnChar1.style.opacity = 1;
        vseMission.style.display = DisplayStyle.Flex;
        vseMission.SetEnabled(true);
        
    }

    private void SetChar2()
    {
        GameData.chosencharacter = 1;
        btnChar1.style.opacity = .5f;
        btnChar2.style.opacity = 1;
        vseMission.style.display = DisplayStyle.Flex;
        vseMission.SetEnabled(true);
    }

    
}
