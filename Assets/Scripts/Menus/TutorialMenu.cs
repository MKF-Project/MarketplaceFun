using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMenu : MonoBehaviour
{
    // Events
    public delegate void OnExitTutorialDelegate();
    public static event OnExitTutorialDelegate OnExitTutorial;

    public List<Sprite> TutorialPages;

    private int _currentPage = 0;
    [SerializeField] private Image _pageImage;

    private void Awake()
    {
        GameMenu.OnViewTutorial += OpenTutorial;
    }

    private void OnDestroy()
    {
        GameMenu.OnViewTutorial -= OpenTutorial;
    }

    private void OpenTutorial()
    {
        _currentPage = 0;
        _pageImage.sprite = TutorialPages[0];

        this.toggleMenu();
    }

    // Button Actions
    public void Next()
    {
        if(_currentPage == TutorialPages.Count - 1)
        {
            OnExitTutorial?.Invoke();
            return;
        }

        _currentPage++;
        _pageImage.sprite = TutorialPages[_currentPage];
    }

    public void Back()
    {
        if(_currentPage == 0)
        {
            OnExitTutorial?.Invoke();
            return;
        }

        _currentPage--;
        _pageImage.sprite = TutorialPages[_currentPage];
    }
}
