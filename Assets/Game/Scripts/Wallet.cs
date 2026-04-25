using TMPro;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [SerializeField] private TMP_Text _balanceText;
    [SerializeField] private int _balance = 10;

    private float _lastSyncTime;
    private int _lastVisualBalance;

    private int _currentVisualBalance;

    public int Balance 
    {
        get => _balance;
        private set => _balance = value;
    }

    public void Add(int amount)
    {
        Balance += amount;
    }

    public bool TrySpend(int amount)
    {
        if (Balance < amount)
        {
            return false;
        }

        Balance -= amount;

        return true;
    }

    private void Start()
    {
        _currentVisualBalance = _balance;
        _balanceText.text = _currentVisualBalance.ToString();
    }

    private void OnValidate()
    {
        _currentVisualBalance = _balance;
        _balanceText.text = _currentVisualBalance.ToString();
    }

    private void Update()
    {
        if (_balance != _currentVisualBalance)
        {
            var t = (Time.time - _lastSyncTime) * 1.5f;
            var v = Mathf.Lerp(_lastVisualBalance, _balance, t);

            _currentVisualBalance = Mathf.FloorToInt(v);
            _balanceText.text = _currentVisualBalance.ToString();
        }
        else
        {
            _lastSyncTime = Time.time;
            _lastVisualBalance = _currentVisualBalance;
        }
    }
}