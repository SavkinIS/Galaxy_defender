using System;
using System.Collections;
using Galaxy.Infrastructure.Routing;
using UnityEngine;
using VContainer;

public class LoadingCurtain : MonoBehaviour
{
   [SerializeField] private CanvasGroup _canvasGroup;
   [SerializeField] private float _fadeDuration = 0.3f;

   private Coroutine _fadeRoutine;
   private float _targetAlpha;
   private IGlobalRouter _globalRouter;
   private bool _isStarted;

   public bool IsHidden => _canvasGroup.alpha <= 0f;

   private void OnEnable()
   {
      if (_isStarted == false)
         return;
      
      _globalRouter.OnSceneStartLoading += OnSceneStartLoading;
      _globalRouter.OnSceneLoaded += OnSceneLoaded;
   }

   private void Start()
   {
      _isStarted = true;
      _globalRouter.OnSceneStartLoading += OnSceneStartLoading;
      _globalRouter.OnSceneLoaded += OnSceneLoaded;
   }

   private void OnDisable()
   {
      _globalRouter.OnSceneStartLoading -= OnSceneStartLoading;
      _globalRouter.OnSceneLoaded -= OnSceneLoaded;
   }

   [Inject]
   public void Construct(IGlobalRouter globalRouter)
   {
      _globalRouter = globalRouter;
   } 

   private void FadeTo(float targetAlpha)
   {
      _targetAlpha = targetAlpha;

      if (_fadeRoutine != null)
         StopCoroutine(_fadeRoutine);

      _canvasGroup.blocksRaycasts = targetAlpha > 0f;

      _fadeRoutine = StartCoroutine(FadeRoutine());
   }

   private IEnumerator FadeRoutine()
   {
      float startAlpha = _canvasGroup.alpha;
      float elapsed = 0f;

      while (elapsed < _fadeDuration)
      {
         elapsed += Time.unscaledDeltaTime;
         float t = Mathf.Clamp01(elapsed / _fadeDuration);

         float eased = t * t * (3f - 2f * t); 

         _canvasGroup.alpha = Mathf.Lerp(startAlpha, _targetAlpha, eased);
         yield return null;
      }
      
      _canvasGroup.alpha = _targetAlpha;
      _canvasGroup.blocksRaycasts = _targetAlpha > 0f;
      _fadeRoutine = null;
   }

   private void OnSceneStartLoading()
   {
      FadeTo(1f);
   }

   private void OnSceneLoaded(bool success)
   {
      FadeTo(0f);
   }
}
