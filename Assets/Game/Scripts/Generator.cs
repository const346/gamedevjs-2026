using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private Gear _gear;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _spawnAnchor;
    [SerializeField] private float _spawnInterval = 30f;

    private float _accumulated;

    private void Awake()
    {
        _gear.OnSimulate.AddListener(OnGearSimulate);
    }

    private void OnGearSimulate(float step)
    {
        _accumulated += Mathf.Abs(step);
        if (_accumulated >= _spawnInterval)
        {
            _accumulated -= _spawnInterval;
            var obj = Instantiate(_prefab, _spawnAnchor.position, Quaternion.identity);

            if (obj.TryGetComponent<Rigidbody2D>(out var body))
            {
                var force = Vector2.up * 5f;
                force.x = Random.Range(-2f, 2f);

                body.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }
}