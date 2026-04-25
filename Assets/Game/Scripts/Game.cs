using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    [SerializeField] private CameraController2D _cameraController;
    [SerializeField] private PageController _pageController;
    [SerializeField] private EnemyTarget _enemyTarget; 

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            child.gameObject.SetActive(false);
        }

        StartCoroutine(Running());
    }

    private IEnumerator Running()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.TryGetComponent<IGameTask>(out var task))
            {
                child.gameObject.SetActive(true);
                yield return task.Running(_enemyTarget);
                child.gameObject.SetActive(false);

                if (!_enemyTarget.IsLive)
                {
                    break;
                }
            }
        }

        yield return Ending();
    }

    private IEnumerator Ending()
    {
        _pageController.HideAll();

        var cameraTarget = _enemyTarget.transform.position.x;
        yield return _cameraController.MoveTo(cameraTarget);

        if (!_enemyTarget.IsLive)
        {
            _enemyTarget.Death();
            yield return new WaitForSeconds(2f);
        }

        yield return new WaitForSeconds(2f);

        if (_enemyTarget.IsLive)
        {
            _pageController.Show("Win");
        }
        else
        {
            _pageController.Show("Lose");
        }
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(0);
    }

    public void PauseGame()
    {
        GetComponent<TimeScaler>().TimeScale = 0;
    }

    public void ResumeGame()
    {
        GetComponent<TimeScaler>().TimeScale = 1;
    }
}

public interface IGameTask
{
    IEnumerator Running(EnemyTarget _enemyTarget);
}