using TMPro;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [SerializeField] private TMP_Text _balanceText;
    [SerializeField] private int _balance = 10;

    public int Balance 
    {
        get => _balance;
        private set => _balance = value;
    }

    public void Add(int amount)
    {
        Balance += amount;
        UpdateView();
    }

    public bool TrySpend(int amount)
    {
        if (Balance < amount)
        {
            return false;
        }

        Balance -= amount;
        UpdateView();

        return true;
    }

    private void UpdateView()
    {
        _balanceText.text = Balance.ToString();
    }

    private void Start()
    {
        UpdateView();
    }

    private void OnValidate()
    {
        UpdateView();
    }
}