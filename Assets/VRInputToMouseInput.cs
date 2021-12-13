using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems
{
    public class VRInputToMouseInput : PointerInputModule
    {
        [SerializeField] new Camera camera = null;
        [SerializeField] Hand hand = null;

        bool pointerDownLastFrame;

        protected override void Awake()
        {
            base.Awake();

            Assert.IsNotNull(camera);
            Assert.IsNotNull(hand);
        }

        public override void Process()
        {
            if (hand.hitCanvas == null)
            {
                pointerDownLastFrame = false;
                return;
            }

            Vector3 worldPoint = hand.hitCanvasWorldPoint;
            Vector3 screenPoint = camera.WorldToScreenPoint(worldPoint);

            PointerEventData pointerEventData = new PointerEventData(eventSystem);

            pointerEventData.position = screenPoint;
            pointerEventData.delta = Vector2.zero;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerEventData, raycastResults);
            pointerEventData.pointerCurrentRaycast = FindFirstRaycast(raycastResults);
            ProcessMove(pointerEventData);

            GameObject handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointerEventData.pointerEnter);

            if (handler != null
                && hand.pointerDown
                && hand.pointerDown != pointerDownLastFrame)
            {
                ExecuteEvents.ExecuteHierarchy(handler, pointerEventData, ExecuteEvents.pointerDownHandler);
            }
            else if (handler != null
                && !hand.pointerDown
                && hand.pointerDown != pointerDownLastFrame)
            {
                ExecuteEvents.ExecuteHierarchy(handler, pointerEventData, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.ExecuteHierarchy(handler, pointerEventData, ExecuteEvents.pointerClickHandler);
            }

            pointerDownLastFrame = hand.pointerDown;
        }



    }
}
