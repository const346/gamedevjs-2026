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
    [SerializeField] private AudioClip _spinSound;
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

        _gearAnchor.gameObject.SetActive(false);

        AudioSource.PlayClipAtPoint(_sellSound, transform.position);
        AudioSource.PlayClipAtPoint(_spinSound, transform.position);
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

        AudioSource.PlayClipAtPoint(_spinSound, transform.position);
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
        var game = FindAnyObjectByType<Game>();
        var k = game.CurrentWave / (float) game.TotalWave;
        k = Mathf.Clamp01(k + 0.5f);

        var gearPrefabIndex = GetRandomIndex(_gearPrefabs.Length, k);
        var gearPrefab = _gearPrefabs[gearPrefabIndex];

        var gear = Instantiate(gearPrefab, _gearHook.transform);
        var seek = Random.Range(0, 100000);

        var sizes = new[] { 10, 10, 14, 14, 18, 18, 6, 6, 22 };
        var numberOfTeeth = sizes[GetRandomIndex(sizes.Length, k)];
        var brokenTeethRatio = Random.value > 0.75f ? Random.value * 0.5f : 0;

        gear.Rebuild(seek, numberOfTeeth, brokenTeethRatio);
        gear.IsDraggable = false;

        return gear;
    }

    private int GetRandomIndex(int count, float k)
    {
        var power = Mathf.Lerp(1f, 5f, k);
        var t = Mathf.Pow(Random.value, power);
        return Mathf.FloorToInt(t * count);
    }
}
