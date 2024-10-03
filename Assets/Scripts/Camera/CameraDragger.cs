using System;
using Connection;
using Events;
using Player.ActionHandlers;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.MonoBehaviourUtils;
using Utils.Singleton;

namespace Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraDragger : DontDestroyMonoBehaviourSingleton<CameraDragger>
    {
        [SerializeField] private CameraDraggerConfig config;
        private ClickHandler _clickHandler;
        private UnityEngine.Camera _camera;
        private Coroutine _dragRoutine;
        private ColorConnectionManager _connectionManager;
        private bool _isDrag;
        private Vector3 _dragOriginPosition;
        private CameraDragBounds _bounds;
        private Vector3 _cameraBasePosition;

        public void SetConnectorManager(ColorConnectionManager connectionManager, CameraDragBounds bounds)
        {
            _connectionManager = connectionManager;
            _bounds = bounds;
            _clickHandler.ClearEvents(OnStartDrag, OnEndDrag);
            _clickHandler.AddDragEventHandlers(OnStartDrag, OnEndDrag);
        }

        private void Start()
        {
            _clickHandler = ClickHandler.Instance;
            _camera = CameraHolder.Instance.MainCamera;
            _cameraBasePosition = _camera.transform.position;
            EventsController.Subscribe<EventModels.Game.TargetColorNodesFilled>(this, OnLevelChange);
        }

        private void OnDisable()
        {
            _clickHandler.ClearEvents(OnStartDrag, OnEndDrag);
        }

        private void OnStartDrag(Vector3 startPosition)
        {
            if (_connectionManager == null) return;
            if (IsPointerOverUIObject()) return;
            _connectionManager.TryGetColorNodeInPosition(startPosition, out var node);
            if (node != null) return;
            _isDrag = true;
            _dragOriginPosition = startPosition;
            _dragOriginPosition.z = transform.position.z;
            _dragRoutine = Coroutines.RepeatEveryUpdateCycle(Drag);
        }

        private void OnEndDrag(Vector3 endPosition)
        {
            if (!_isDrag) return;
            _isDrag = false;
            Coroutines.Stop(_dragRoutine);
        }

        private void OnLevelChange(EventModels.Game.TargetColorNodesFilled e)
        {
            _camera.transform.position = _cameraBasePosition;
        }

        //мог подзаморочится и попробовать запихнуть эту логику в ивенты, в ClickObserver ClickHandler как то обработать, но не заморочился ЫЫы
        private bool IsPointerOverUIObject()
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }

        private void Drag()
        {
            var currentPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            currentPosition.z = transform.position.z;
            var difference = _dragOriginPosition - currentPosition;

            var targetPosition = transform.position;
            targetPosition += difference * (config.DragSpeed * Time.fixedDeltaTime);
            targetPosition.x = Mathf.Clamp(targetPosition.x, _bounds.minPosition.x - config.PaddingRange,
                _bounds.maxPosition.x + config.PaddingRange);
            targetPosition.y = Mathf.Clamp(targetPosition.y, _bounds.minPosition.y - config.PaddingRange,
                _bounds.maxPosition.y + config.PaddingRange);
            transform.position = targetPosition;
            _dragOriginPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        }
    }
}