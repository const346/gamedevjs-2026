using UnityEngine;

public class Motor : MonoBehaviour
{
    [SerializeField] private Gear _gear;
    [SerializeField] private float _speed;

    private void Update()
    {
        _gear.Simulate(_gear, _speed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, transform.position + Vector3.down * 0.2f);
        Gizmos.DrawLine(transform.position + Vector3.left * 0.2f, transform.position + Vector3.right * 0.2f);
    }
}