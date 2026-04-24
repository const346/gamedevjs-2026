using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    [SerializeField] private PageController _pageController;
    [SerializeField] private EnemyTarget _enemyTarget; 
    [SerializeField] private Camera _camera;

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

        while (true) // move camera to enemy target
        {
            var a = _camera.transform.position.x;
            var b = _enemyTarget.transform.position.x;

            if (Mathf.Abs(b - a) < 0.5f)
            {
                break;
            }

            var p = _camera.transform.position;
            p.x = Mathf.MoveTowards(a, b, 25f * Time.deltaTime);
            _camera.transform.position = p;

            yield return null;
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
}

public interface IGameTask
{
    IEnumerator Running(EnemyTarget _enemyTarget);
}