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
    [SerializeField] private bool _isSale;
    [SerializeField] private float _respawnTime = 3;
    [SerializeField] private float _showingTime = 0.5f;
    [Space]
    [SerializeField] private Gear[] _gearPrefabs;

    //.........
    [SerializeField] private Vector2 _spawnStart;
    [SerializeField] private Vector2 _spawnEnd;

    private Gear _currentGear;
    private Wallet _wallet;

    private void Start()
    {
        _wallet = FindAnyObjectByType<Wallet>();

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

        // check balance


        _currentGear.IsDraggable = true;
        _currentGear.transform.parent = null;

        // waiting
        _gearAnchor.gameObject.SetActive(true);

        yield return new WaitUntil(() => Vector2.Distance(_currentGear.Position, _gearAnchor.Position) > 0.1f);
        _gearAnchor.gameObject.SetActive(false);

        // payment
        _wallet.TrySpend(price);
        
        // restart
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
