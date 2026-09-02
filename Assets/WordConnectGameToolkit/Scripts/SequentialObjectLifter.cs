using System;
using DG.Tweening;
using UnityEngine;

namespace WordConnectGameToolkit
{
    /// <summary>
    /// Sequentially lifts target objects by the configured distance and then returns them back.
    /// The animation runs in a loop and the objects are animated one after another.
    /// </summary>
    public class SequentialObjectLifter : MonoBehaviour
    {
        [Header("Targets")] [Tooltip("Ordered list of objects that will be animated.")]
        [SerializeField] private Transform[] targets = Array.Empty<Transform>();

        [Header("Movement")] [Tooltip("Distance in pixels/units that each object will be lifted by.")]
        [SerializeField] private float liftDistance = 20f;
        [Tooltip("Duration of the upward movement in seconds.")]
        [SerializeField] private float liftDuration = 0.3f;
        [Tooltip("Duration of the return movement in seconds.")]
        [SerializeField] private float returnDuration = 0.3f;
        [Tooltip("Delay between the start of lifts for consecutive objects in seconds.")]
        [SerializeField] private float delayBetween = 0.1f;
        [Tooltip("Pause applied after every full cycle before the loop restarts.")]
        [SerializeField] private float cyclePause = 0.25f;
        [Header("Easing")] [SerializeField] private Ease liftEase = Ease.OutQuad;
        [SerializeField] private Ease returnEase = Ease.InQuad;

        private Sequence _sequence;
        private Vector3[] _initialLocalPositions = Array.Empty<Vector3>();
        private Vector2[] _initialAnchoredPositions = Array.Empty<Vector2>();
        private bool[] _usesRectTransform = Array.Empty<bool>();

        private void OnEnable()
        {
            if (targets == null || targets.Length == 0)
            {
                return;
            }

            CacheInitialPositions();
            BuildSequence();
        }

        private void OnDisable()
        {
            KillSequence();
            RestoreInitialPositions();
        }

        private void CacheInitialPositions()
        {
            int count = targets.Length;
            _initialLocalPositions = new Vector3[count];
            _initialAnchoredPositions = new Vector2[count];
            _usesRectTransform = new bool[count];

            for (int i = 0; i < count; i++)
            {
                Transform target = targets[i];
                if (target == null)
                {
                    continue;
                }

                if (target is RectTransform rectTransform)
                {
                    _usesRectTransform[i] = true;
                    _initialAnchoredPositions[i] = rectTransform.anchoredPosition;
                }
                else
                {
                    _initialLocalPositions[i] = target.localPosition;
                }
            }
        }

        private void BuildSequence()
        {
            KillSequence();

            _sequence = DOTween.Sequence();
            bool hasValidTarget = false;

            for (int i = 0; i < targets.Length; i++)
            {
                Transform target = targets[i];
                if (target == null)
                {
                    continue;
                }

                float startTime = i * delayBetween;
                if (_usesRectTransform[i])
                {
                    RectTransform rectTransform = (RectTransform)target;

                    Sequence targetSequence = DOTween.Sequence();
                    targetSequence.Append(rectTransform
                        .DOAnchorPosY(liftDistance, liftDuration)
                        .SetRelative()
                        .SetEase(liftEase));
                    targetSequence.Append(rectTransform
                        .DOAnchorPosY(-liftDistance, returnDuration)
                        .SetRelative()
                        .SetEase(returnEase));
                    _sequence.Insert(startTime, targetSequence);
                    hasValidTarget = true;
                }
                else
                {
                    Sequence targetSequence = DOTween.Sequence();
                    targetSequence.Append(target
                        .DOLocalMoveY(liftDistance, liftDuration)
                        .SetRelative()
                        .SetEase(liftEase));
                    targetSequence.Append(target
                        .DOLocalMoveY(-liftDistance, returnDuration)
                        .SetRelative()
                        .SetEase(returnEase));
                    _sequence.Insert(startTime, targetSequence);
                    hasValidTarget = true;
                }
            }

            if (!hasValidTarget)
            {
                _sequence.Kill();
                _sequence = null;
                return;
            }

            if (cyclePause > 0f)
            {
                _sequence.AppendInterval(cyclePause);
            }

            _sequence.SetLoops(-1, LoopType.Restart);
        }

        private void KillSequence()
        {
            if (_sequence == null)
            {
                return;
            }

            _sequence.Kill();
            _sequence = null;
        }

        private void RestoreInitialPositions()
        {
            for (int i = 0; i < targets.Length; i++)
            {
                Transform target = targets[i];
                if (target == null)
                {
                    continue;
                }

                if (_usesRectTransform[i])
                {
                    ((RectTransform)target).anchoredPosition = _initialAnchoredPositions[i];
                }
                else
                {
                    target.localPosition = _initialLocalPositions[i];
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            liftDistance = Mathf.Max(0f, liftDistance);
            liftDuration = Mathf.Max(0f, liftDuration);
            returnDuration = Mathf.Max(0f, returnDuration);
            delayBetween = Mathf.Max(0f, delayBetween);
            cyclePause = Mathf.Max(0f, cyclePause);
        }
#endif
    }
}
