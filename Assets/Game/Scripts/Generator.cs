using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private Gear _gear;
    [SerializeField] private Thrower _thrower;
    [SerializeField] private Animator _animator; 
    [SerializeField] private Transform _onlyActive;
    [Space]
    [SerializeField] private float _spawnInterval = 30f;
    [SerializeField] private float throwAngle;
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private float throwAngleVariance = 75f;
    [SerializeField] private float throwForceVariance = 2f;

    private float _m;
    private float _accumulated;
    private float _lastStepTime = -100;

    private void Awake()
    {
        _gear.OnSimulate.AddListener(OnGearSimulate);
    }

    private void OnGearSimulate(float step)
    {
        _m += step;

        if (Mathf.Abs(_m) > 10)
        {
            _accumulated += Mathf.Abs(_m);
            _m = 0;
        }

        if (_accumulated >= _spawnInterval)
        {
            _accumulated -= _spawnInterval;

            var aV = Random.Range(-throwAngleVariance, throwAngleVariance);
            var fV = Random.Range(-throwForceVariance, throwForceVariance);
            var angle = (throwAngle + aV) * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var force = (fV + throwForce) * direction;

            _thrower.Throw(force);
        }

        if (_animator != null)
        {
            var time = ((_accumulated + Mathf.Abs(_m)) % _spawnInterval) / _spawnInterval;
            _animator.SetFloat("Time", time);
        }

        _lastStepTime = Time.time;
    }

    private void Update()
    {
        var isActive = Time.time - _lastStepTime < 0.5f;

        if (_animator != null)
        {
            _animator.SetFloat("Active", isActive ? 1 : 0);
        }

        if (_onlyActive != null)
        {
            _onlyActive.gameObject.SetActive(isActive);
        }
    }
}