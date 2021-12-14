using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems
{
    public class VRInputModule : PointerInputModule
    {
        [SerializeField] new Camera camera = null;
        [SerializeField] Hand hand = null;

        bool pointerDownLastFrame = false;
        Canvas pointerDownCanvas;
        CanvasVisualPointerHolder visualPointerHolder;

        PointerEventData storedPointerEvent = null;

        protected override void Awake()
        {
            base.Awake();

            Assert.IsNotNull(camera);
            Assert.IsNotNull(hand);
        }

        public override void Process()
        {
            PointerEventData pointerEvent = storedPointerEvent;
            if (pointerEvent == null)
            {
                pointerEvent = new PointerEventData(eventSystem);
                storedPointerEvent = pointerEvent;
            }

            Canvas canvas = hand.hitCanvas;
            bool pointerDownThisFrame = hand.pointerDown && !pointerDownLastFrame;

            if (canvas == null)
            {
                if (pointerDownCanvas != null)
                {
                    canvas = pointerDownCanvas;
                }
                else
                {
                    if (visualPointerHolder != null)
                    {
                        visualPointerHolder.SetPointerActive(false);
                        visualPointerHolder = null;
                    }

                    storedPointerEvent = null;
                    pointerDownLastFrame = false;
                    return;
                }
            }

            Plane plane = new Plane(canvas.transform.forward, 0f);
            Vector3 rayOrigin = hand.transform.position - canvas.transform.position;
            Vector3 rayDir = hand.transform.forward;
            Ray ray = new Ray(rayOrigin, rayDir);

            float dist = 0f;
            plane.Raycast(ray, out dist);

            Vector3 worldPoint =  canvas.transform.position + rayOrigin + rayDir * dist;
            Vector3 screenPoint = camera.WorldToScreenPoint(worldPoint);


            visualPointerHolder = canvas.GetComponent<CanvasVisualPointerHolder>();
            if (visualPointerHolder != null)
            {
                visualPointerHolder.SetPointerActive(true);

                Vector2 distFromCentre = worldPoint - canvas.transform.position;

                distFromCentre = Quaternion.Inverse(canvas.transform.rotation) * distFromCentre;

                distFromCentre.x /= canvas.transform.localScale.x;
                distFromCentre.y /= canvas.transform.localScale.y;

                visualPointerHolder.SetPointerPos(distFromCentre);
            }

            pointerEvent.position = screenPoint;
            pointerEvent.delta = pointerEvent.position - pointerEvent.pressPosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerEvent, raycastResults);
            
            pointerEvent.pointerCurrentRaycast = FindFirstRaycast(raycastResults);

            GameObject currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            if (pointerDownThisFrame)
            {
                ProcessPointerPress(pointerEvent, currentOverGo);
                pointerDownCanvas = canvas;
            }

            bool pointerReleasedThisFrame = !hand.pointerDown && pointerDownLastFrame;

            if (pointerReleasedThisFrame)
            {
                ProcessPointerRelease(pointerEvent, currentOverGo);
                pointerDownCanvas = null;
            }

            pointerDownLastFrame = hand.pointerDown;

            ProcessMove(pointerEvent);
            ProcessDrag(pointerEvent);

        }

        void ProcessPointerPress(PointerEventData pointerEvent, GameObject currentOverGo)
        {
            pointerEvent.pressPosition = pointerEvent.position;
            pointerEvent.eligibleForClick = true;
            pointerEvent.dragging = false;
            pointerEvent.useDragThreshold = true;
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

            DeselectIfSelectionChanged(currentOverGo, pointerEvent);

            GameObject newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);
            GameObject newClick = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            if (newPressed == null)
                newPressed = newClick;

            float time = Time.unscaledTime;

            if (newPressed == pointerEvent.lastPress)
            {
                float diffTime = time - pointerEvent.clickTime;
                if (diffTime < 0.3f)
                    ++pointerEvent.clickCount;
                else
                    pointerEvent.clickCount = 1;

                pointerEvent.clickTime = time;
            }
            else
            {
                pointerEvent.clickCount = 1;
            }

            pointerEvent.pointerPress = newPressed;
            pointerEvent.rawPointerPress = currentOverGo;
            pointerEvent.pointerClick = newClick;

            pointerEvent.clickTime = time;

            pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

            if (pointerEvent.pointerDrag != null)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
        }

        void ProcessPointerRelease(PointerEventData pointerEvent, GameObject currentOverGo)
        {
            ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

            var pointerClickHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

            // PointerClick and Drop events
            if (pointerEvent.pointerClick == pointerClickHandler && pointerEvent.eligibleForClick)
            {
                ExecuteEvents.Execute(pointerClickHandler, pointerEvent, ExecuteEvents.pointerClickHandler);
            }
            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
            {
                ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
            }

            pointerEvent.eligibleForClick = false;
            pointerEvent.pointerPress = null;
            pointerEvent.rawPointerPress = null;
            pointerEvent.pointerClick = null;

            if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

            pointerEvent.dragging = false;
            pointerEvent.pointerDrag = null;

            // redo pointer enter / exit to refresh state
            // so that if we moused over something that ignored it before
            // due to having pressed on something else
            // it now gets it.
            if (currentOverGo != pointerEvent.pointerEnter)
            {
                HandlePointerExitAndEnter(pointerEvent, null);
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
            }

        }

        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            bool moving = pointerEvent.IsPointerMoving();

            if (moving && pointerEvent.pointerDrag != null
                && !pointerEvent.dragging
                && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging && moving && pointerEvent.pointerDrag != null)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }

    }
}
