using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{
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
                yield return task.Running();
                child.gameObject.SetActive(false);
            }
        }
    }
}

public interface IGameTask
{
    IEnumerator Running();
}