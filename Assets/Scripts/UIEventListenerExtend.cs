using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace orisox.com
{
    public class UIEventListenerExtend : MonoBehaviour
    {
        public delegate void VoidDelegate(GameObject go);
        public delegate void BoolDelegate(GameObject go, bool state);
        public delegate void FloatDelegate(GameObject go, float delta);
        public delegate void VectorDelegate(GameObject go, Vector2 delta);
        public delegate void ObjectDelegate(GameObject go, GameObject obj);
        public delegate void KeyCodeDelegate(GameObject go, KeyCode key);
        public delegate void IntDelegate(GameObject go, int value);

        public VoidDelegate onSubmit;
        public VoidDelegate onClick;
        public VoidDelegate onDoubleClick;
        public BoolDelegate onHover;
        public BoolDelegate onPress;
        public BoolDelegate onSelect;
        public FloatDelegate onScroll;
        public VoidDelegate onDragStart;
        public VectorDelegate onDrag;
        public VoidDelegate onDragOver;
        public VoidDelegate onDragOut;
        public VoidDelegate onDragEnd;
        public ObjectDelegate onDrop;
        public KeyCodeDelegate onKey;
        public BoolDelegate onTooltip;
        public IntDelegate onClickLua;

        UIWidget currentWidget;

        public bool passThrough = true;

        protected virtual void Awake()
        {
            currentWidget = gameObject.GetComponent<UIWidget>();
        }

        public static void SendMsgToNextWidget(UIWidget CurrentWidget, string Msg, System.Object Param, System.Action<Transform> OnSucceed)
        {
            var Ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
            var Hits = Physics.RaycastAll(Ray);

            var Children = new List<UIWidget>();

            foreach (var Hit in Hits)
            {
                var Widget = Hit.collider.gameObject.GetComponent<UIWidget>();
                if (null != Widget)
                {
                    Children.Add(Widget);
                }
            }

            if (0 < Children.Count)
            {
                Children.Sort((x, y) => y.raycastDepth.CompareTo(x.raycastDepth));
                for (var i = 0; i < Children.Count; ++i)
                {
                    if (Children[i] == CurrentWidget)
                    {
                        if (i + 1 < Children.Count)
                        {
                            var Child = Children[i + 1];
                            Child.transform.SendMessage(Msg, Param, SendMessageOptions.DontRequireReceiver);
                            if (null != OnSucceed)
                            {
                                OnSucceed(Child.transform);
                            }
                        }
                        break;
                    }
                }
            }
        }

        protected virtual void SendMsgToNextWidget(string Msg, System.Object Param)
        {
            if (passThrough)
            {
                SendMsgToNextWidget(currentWidget, Msg, Param, null);
            }
        }

        protected virtual void OnSubmit() { if (onSubmit != null) onSubmit(gameObject); SendMsgToNextWidget("OnSubmit", null); }
        protected virtual void OnClick() { if (onClick != null) onClick(gameObject); SendMsgToNextWidget("OnClick", null); }
        protected virtual void OnDoubleClick() { if (onDoubleClick != null) onDoubleClick(gameObject); SendMsgToNextWidget("OnDoubleClick", null); }
        protected virtual void OnHover(bool isOver) { if (onHover != null) onHover(gameObject, isOver); }
        protected virtual void OnPress(bool isPressed) { if (onPress != null) onPress(gameObject, isPressed); SendMsgToNextWidget("OnPress", isPressed); }
        protected virtual void OnSelect(bool selected) { if (onSelect != null) onSelect(gameObject, selected); SendMsgToNextWidget("OnSelect", selected); }
        protected virtual void OnScroll(float delta) { if (onScroll != null) onScroll(gameObject, delta); SendMsgToNextWidget("OnScroll", delta); }
        protected virtual void OnDragStart() { if (onDragStart != null) onDragStart(gameObject); SendMsgToNextWidget("OnDragStart", null); }
        protected virtual void OnDrag(Vector2 delta) { if (onDrag != null) onDrag(gameObject, delta); SendMsgToNextWidget("OnDrag", delta); }
        protected virtual void OnDragOver() { if (onDragOver != null) onDragOver(gameObject); SendMsgToNextWidget("OnDragOver", null); }
        protected virtual void OnDragOut() { if (onDragOut != null) onDragOut(gameObject); SendMsgToNextWidget("OnDragOut", null); }
        protected virtual void OnDragEnd() { if (onDragEnd != null) onDragEnd(gameObject); SendMsgToNextWidget("OnDragEnd", null); }
        protected virtual void OnDrop(GameObject go) { if (onDrop != null) onDrop(gameObject, go); SendMsgToNextWidget("OnDrop", go); }
        protected virtual void OnKey(KeyCode key) { if (onKey != null) onKey(gameObject, key); SendMsgToNextWidget("OnKey", key); }
        protected virtual void OnTooltip(bool show) { if (onTooltip != null) onTooltip(gameObject, show); SendMsgToNextWidget("OnTooltip", show); }
    }
}
