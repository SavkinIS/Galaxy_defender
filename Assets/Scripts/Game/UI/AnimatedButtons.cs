using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedButtons : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Color[] _colors;
    [SerializeField] private float _maxDuration;
    [SerializeField] private float _minDuration;
    private Coroutine _animatedRoutine;

    private void Awake()
    {
        _animatedRoutine = StartCoroutine(ChangeColorRoutine());
    }

    private IEnumerator ChangeColorRoutine()
    {
        while (gameObject.activeInHierarchy)
        {
            _image.color = _colors[Random.Range(0, _colors.Length)];
            yield return new WaitForSeconds(Random.Range(_minDuration, _maxDuration));
        }
    }

    private void OnDestroy()
    {
        if (_animatedRoutine != null)
        {
            StopCoroutine(_animatedRoutine);
        }
    }
}