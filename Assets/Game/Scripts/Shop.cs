using System.Collections;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private Gear[] _gearPrefabs;
    [SerializeField] private Vector2 _spawnStart;
    [SerializeField] private Vector2 _spawnEnd;
    [SerializeField] private float _respawnTime = 3;
    
    private Gear _currentGear;
    private bool _isSpawning;

    private void Update()
    {
        if (_isSpawning) 
        {
            return; 
        }

        if (_currentGear == null)
        {
            StartCoroutine(SpawnProductYield());
        }
        else 
        {
            var p1 = transform.position + (Vector3)_spawnEnd;
            var p2 = _currentGear.transform.position;

            if (Vector3.Distance(p1, p2) > 0.1f)
            {
                _currentGear = null;
            }
        }
    }

    private IEnumerator SpawnProductYield()
    {
        _isSpawning = true;
        _label.text = "";
        
        yield return new WaitForSeconds(_respawnTime);

        var gearPrefab = _gearPrefabs[Random.Range(0, _gearPrefabs.Length)];

        var spawnStart = transform.position + (Vector3)_spawnStart;
        var spawnEnd = transform.position + (Vector3)_spawnEnd;

        _currentGear = Instantiate(gearPrefab, spawnStart, Quaternion.identity);
        _currentGear.Randomize();
        _currentGear.IsDraggable = false;

        for (var t = 0f; t < 1f; t += Time.deltaTime * 0.3f)
        {
            _currentGear.transform.position = Vector3.Lerp(spawnStart, spawnEnd, t);
            _currentGear.transform.rotation = Quaternion.Euler(0, 0, t * 360 * 2);
            yield return null;
        }

        _label.text = "100";

        _currentGear.transform.position = spawnEnd;
        _currentGear.IsDraggable = true;

        _isSpawning = false;
    }
}
