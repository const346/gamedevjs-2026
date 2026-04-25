using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private Gear _gear;
    [SerializeField] private Thrower _thrower;
    [Space]
    [SerializeField] private float _spawnInterval = 30f;
    [SerializeField] private float throwAngle;
    [SerializeField] private float throwForce = 5f;
    [SerializeField] private float throwAngleVariance = 75f;
    [SerializeField] private float throwForceVariance = 2f;

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

            var aV = Random.Range(-throwAngleVariance, throwAngleVariance);
            var fV = Random.Range(-throwForceVariance, throwForceVariance);
            var angle = (throwAngle + aV) * Mathf.Deg2Rad;
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var force = (fV + throwForce) * direction;

            _thrower.Throw(force);
        }
    }
}