using System;
using System.Collections;
using Core.Ship;
using DG.Tweening;
using UnityEngine;

public class ShipView : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private int _size;
    [SerializeField] private BulletsCounterView _currentBullets;
    [SerializeField, Min(0.01f)] private float _waitingMoveDuration = 0.5f;
    [SerializeField, Min(0.01f)] private float _battleMoveDuration = 0.5f;

    private Coroutine _waitingMovement;
    private Coroutine _waitingRoutine;
    private Tween _battleMovement;
    private ShipData _shipData;
    private RimPlacement _placement;

    public int Size => _size;
    public Renderer VisualRenderer => _meshRenderer;
    public ShipData Data => _shipData;
    public RimPlacement Placement => _placement;

    public void SetData(ShipData data)
    {
        _shipData = data;
        _meshRenderer.material.color = _shipData.Color;
        gameObject.name = $"{gameObject.name}_bullets{_shipData.CurrentBullets}";
        _currentBullets.SetCount(_shipData.CurrentBullets);
    }

    public void SetPlacement(RimPlacement placement)
    {
        _placement = placement;
    }

    public void Refresh()
    {
        _currentBullets.SetCount(_shipData.CurrentBullets);
    }

    public void EnableCounter()
    {
        _currentBullets.Enable();
    }

    public void StartWaitingRoutine(IEnumerator routine)
    {
        StopWaitingRoutine();
        _waitingRoutine = StartCoroutine(routine);
    }

    public void StopWaitingRoutine()
    {
        if (_waitingRoutine == null)
            return;

        StopCoroutine(_waitingRoutine);
        _waitingRoutine = null;
    }

    public void MoveToBattle(Transform target, Action onComplete)
    {
        StopWaitingMovement();
        StopBattleMovement();
        var duration = Mathf.Max(0.01f, _battleMoveDuration);
        _battleMovement = DOTween.Sequence()
            .Append(transform.DOMove(target.position, duration))
            .Join(transform.DORotateQuaternion(target.rotation, duration))
            .SetEase(Ease.InOutSine)
            .SetAutoKill(true)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                _battleMovement = null;
                onComplete?.Invoke();
            });
    }

    public void StopBattleMovement()
    {
        if (_battleMovement == null)
            return;

        var movement = _battleMovement;
        _battleMovement = null;
        movement.Kill();
    }

    public void MoveToWaitingZone(Transform target, Action onComplete)
    {
        StopWaitingMovement();
        _waitingMovement = StartCoroutine(MoveToWaitingZoneRoutine(target, onComplete));
    }

    public void StopWaitingMovement()
    {
        if (_waitingMovement == null)
            return;

        StopCoroutine(_waitingMovement);
        _waitingMovement = null;
    }

    private IEnumerator MoveToWaitingZoneRoutine(Transform target, Action onComplete)
    {
        var startPosition = transform.position;
        var startRotation = transform.rotation;
        var duration = Mathf.Max(0.01f, _waitingMoveDuration);
        var elapsed = 0f;

        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(elapsed / duration);
            transform.SetPositionAndRotation(
                Vector3.Lerp(startPosition, target.position, progress),
                Quaternion.Slerp(startRotation, target.rotation, progress));
        }

        _waitingMovement = null;
        onComplete?.Invoke();
    }

    public void CompleteMission()
    {
        StopWaitingRoutine();
        StopBattleMovement();
        StopWaitingMovement();
        gameObject.SetActive(false);
    }
}