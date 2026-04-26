using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField] private CameraController2D _cameraController;
    [SerializeField] private PageController _pageController;
    [SerializeField] private EnemyTarget _enemyTarget;
    [SerializeField] private TMP_Text _waveInfo;
    [SerializeField] private Image _hp;

    public int CurrentWave { get; private set; }
    public int TotalWave { get; private set; }

    private static bool _isShowedTutorial;

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            child.gameObject.SetActive(false);
        }

        if (!_isShowedTutorial)
        {
            _pageController.Show("Tutorial");
            _isShowedTutorial = true;

            PauseGame();
        }

        StartCoroutine(Running());
    }

    private void Update()
    {
        _hp.fillAmount = _enemyTarget.HealthNormalize;
    }

    private IEnumerator Running()
    {
        TotalWave = transform.childCount;

        _waveInfo.text = $"{CurrentWave + 1}/{TotalWave}";

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.TryGetComponent<IGameTask>(out var task))
            {
                child.gameObject.SetActive(true);
                yield return task.Running(_enemyTarget);
                child.gameObject.SetActive(false);

                CurrentWave++;
                _waveInfo.text = $"{CurrentWave + 1}/{TotalWave}";

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