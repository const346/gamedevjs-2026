using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private Gear _gear;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Transform _spawnAnchor;
    [SerializeField] private float _spawnInterval = 30f; 
    [SerializeField] private float _destroyDelay = 15f;
    [Space]
    [SerializeField] private float pushDirection;
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private float pushDirectionVariance = 75f;
    [SerializeField] private float pushForceVariance = 2f;
    [SerializeField] private bool rotateToPushDirection;

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
            Destroy(obj, _destroyDelay);

            if (obj.TryGetComponent<Rigidbody2D>(out var body))
            {
                var dv = Random.Range(-pushDirectionVariance, pushDirectionVariance);
                var fv = Random.Range(-pushForceVariance, pushForceVariance);
                var angle = (pushDirection + dv) * Mathf.Deg2Rad;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                body.AddForce((fv + pushForce) * direction, ForceMode2D.Impulse);

                if (rotateToPushDirection)
                {
                    obj.transform.rotation = Quaternion.Euler(0f, 0f, pushDirection + dv);
                }
            }
        }
    }
}