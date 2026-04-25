using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _gearHook;
    [SerializeField] private Gear _gearAnchor;
    [Space]
    [SerializeField] private Color _priceColor;
    [SerializeField] private Color _lockedPriceColor;
    [Space]
    [SerializeField] private AudioClip _buySound;
    [SerializeField] private AudioClip _sellSound;
    [Space]
    [SerializeField] private bool _isSale;
    [SerializeField] private float _respawnTime = 3;
    [SerializeField] private float _showingTime = 0.5f;
    [Space]
    [SerializeField] private Gear[] _gearPrefabs;

    private Gear _currentGear;
    private Wallet _wallet;

    private void Start()
    {
        _wallet = FindAnyObjectByType<Wallet>();

        _label.color = _priceColor;

        if (_isSale)
        {
            StartCoroutine(SellProcess());
        }
        else
        {
            StartCoroutine(BuyProcess());
        }
    }

    private IEnumerator SellProcess()
    {
        _animator.SetFloat("Time", 1f); 
        yield return null;

        _gearAnchor.gameObject.SetActive(true);

        var sellGear = default(Gear);
        yield return new WaitUntil(() =>
        {
            var gears = _gearAnchor.GearGraph.Get(_gearAnchor.Position);
            sellGear = gears.FirstOrDefault(x => x != _gearAnchor);

            if (sellGear != null)
            {
                var price = sellGear.GetPrice() / 2;
                _label.text = price.ToString();
            }
            else
            {
                _label.text = string.Empty;
            }

            return sellGear != null && !sellGear.IsDragging;
        });

        sellGear.IsDraggable = false;
        sellGear.transform.parent = _gearHook;
        sellGear.transform.localPosition = Vector3.zero;

        var price = sellGear.GetPrice() / 2;
        _wallet.Add(price);

        AudioSource.PlayClipAtPoint(_sellSound, transform.position);

        _gearAnchor.gameObject.SetActive(false);

        for (var t = 0f; t < 1f; t += _showingTime * Time.deltaTime)
        {
            _animator.SetFloat("Time", 1f - t);
            yield return null;
        }

        Destroy(sellGear.gameObject);
        _label.text = string.Empty;

        StartCoroutine(SellProcess());
    }

    private IEnumerator BuyProcess()
    {
        _label.text = string.Empty;

        yield return new WaitForSeconds(_respawnTime);
        _currentGear = GenerateGear();

        var price = _currentGear.GetPrice();

        // showing
        for (var t = 0f; t < 1f; t += _showingTime * Time.deltaTime)
        {
            _animator.SetFloat("Time", t);
            yield return null;
        }

        _label.text = price.ToString();
        _currentGear.transform.parent = null;
        _gearAnchor.gameObject.SetActive(true);


        yield return new WaitUntil(() =>
        {
            var isAvailable = _wallet.Balance >= price;

            _label.color = isAvailable ?
                _priceColor : _lockedPriceColor;

            _currentGear.IsDraggable = isAvailable;

            if (isAvailable)
            {
                var distance = Vector2.Distance(_currentGear.Position, _gearAnchor.Position);
                if (distance > 0.1f)
                {
                    return true;
                }
            }

            return false;
        });

        _gearAnchor.gameObject.SetActive(false);
        _wallet.TrySpend(price);

        AudioSource.PlayClipAtPoint(_buySound, transform.position);

        StartCoroutine(BuyProcess());
    }

    private Gear GenerateGear()
    {
        var gearPrefab = _gearPrefabs[Random.Range(0, _gearPrefabs.Length)];
        var gear = Instantiate(gearPrefab, _gearHook.transform);

        var seek = Random.Range(0, 100000);
        var numberOfTeeth = 6 + Random.Range(0, 4) * 4;
        var brokenTeethRatio = Random.value > 0.5f ? Random.value * 0.5f : 0;

        gear.Rebuild(seek, numberOfTeeth, brokenTeethRatio);
        gear.IsDraggable = false;

        return gear;
    }
}
