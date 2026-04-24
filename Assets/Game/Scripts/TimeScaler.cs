using UnityEngine;

public class TimeScaler : MonoBehaviour
{
    [SerializeField][Range(0f, 10f)] private float _timeScale = 1.0f;

    private void Update()
    {
        Time.timeScale = _timeScale;
    }
}