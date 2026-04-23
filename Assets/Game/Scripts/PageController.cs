using System;
using UnityEngine;

public class PageController : MonoBehaviour
{
    [SerializeField] private RectTransform[] _pages;

    public void Show(string name)
    {
        foreach (var page in _pages)
        {
            var comparisonType = StringComparison.OrdinalIgnoreCase;
            var isActive = string.Equals(page.name, name, comparisonType);

            page.gameObject.SetActive(isActive);
        }
    }

    public void HideAll()
    {
        for (var i = 0; i < _pages.Length; i++)
        {
            _pages[i].gameObject.SetActive(false);
        }
    }

    public void ShowDefault()
    {
        for (var i = 0; i < _pages.Length; i++)
        {
            _pages[i].gameObject.SetActive(i == 0);
        }
    }
}
