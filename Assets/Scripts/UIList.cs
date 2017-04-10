using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace orisox.com
{
    public class UIListSampleData
    {
        public int startRealIndex;
        public int endRealIndex;
        public int sampleIndex;

        public UIListSampleData(int StartRealIndex, int EndRealIndex, int SampleIndex)
        {
            startRealIndex = StartRealIndex < 0 ? 0 : StartRealIndex;
            endRealIndex = EndRealIndex <= startRealIndex ? startRealIndex + 1 : EndRealIndex;
            sampleIndex = SampleIndex;
        }

        public int Range()
        {
            return endRealIndex - startRealIndex;
        }

        public int Range(int RealIndex)
        {
            if (startRealIndex <= RealIndex)
            {
                return RealIndex - startRealIndex + 1;
            }
            else
            {
                return 0;
            }
        }
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(UIPanel))]
    public class UIList : MonoBehaviour
    {
        public enum Align
        {
            center,
            left,
            right,
            top,
            bottom,
        }

        public enum Layout
        {
            vertical,
            horizontol,
        }

        public Align align = Align.center;
        public Layout layout = Layout.vertical;
        public List<UIWidget> itemSamples;
        public int space;
        public float moveSpeedRatio = 1f;
        public bool overMove = true;
        public float overMoveBackTime = 0.3f;
        public float overMoveDamping = 1;
        public float overMoveMinOffset = 0;
        public float overMoveMaxOffset = 0;
        public float decelerationDuration = 0.3f;
        public float decelerationSpeedRatio = 5f;
        public const float moveCenterDuration = 0.3f;
        public bool childMoveCenter = false;
        public bool childMoveCenterOnClick = false;

        public System.Action<int, GameObject> OnItemChanged;
        public System.Action<int, GameObject> OnItemCenter;

        protected SortedDictionary<int, UIListItem> items;
        protected UIPanel panel;
        protected BoxCollider moveCollider;
        protected UIPanel moveColliderPanel;
        protected UIWidget moveColliderWidget;
        protected Vector3 lastPosition = Vector3.zero;
        protected float offset;
        protected bool isInScrollView = false;
        protected bool isMove = false;
        protected int maxCount;
        protected UIListSampleData[] sampleDatas;
        protected SortedDictionary<int, List<UIListItem>> objectPool;

        public virtual void Init()
        {
            if (null == objectPool)
            {
                objectPool = new SortedDictionary<int, List<UIListItem>>();
            }

            if (null == itemSamples || 0 == itemSamples.Count)
            {
                if (0 != transform.childCount)
                {
                    itemSamples = new List<UIWidget>();
                    for (int i = 0; i < transform.childCount; ++i)
                    {
                        var Item = transform.GetChild(i).GetComponent<UIWidget>();
                        if (null != Item)
                        {
                            Item.gameObject.SetActive(false);
                            itemSamples.Add(Item);
                        }
                    }
                }
            }

            for (int i = 0; i < itemSamples.Count; ++i)
            {
                itemSamples[i].gameObject.SetActive(false);
            }

            if (null != itemSamples && 0 < itemSamples.Count)
            {
                if (null == items)
                {
                    items = new SortedDictionary<int, UIListItem>();
                }
                panel = GetComponent<UIPanel>();
                panel.cullWhileDragging = false;

                if (null == moveColliderPanel)
                {
                    moveColliderPanel = NGUITools.AddChild<UIPanel>(gameObject);
                    moveColliderPanel.clipping = UIDrawCall.Clipping.ConstrainButDontClip;
                    moveColliderPanel.name = "__UIListMoveColliderPanel__";

                    moveColliderWidget = NGUITools.AddChild<UIWidget>(moveColliderPanel.gameObject);
                    moveColliderWidget.gameObject.AddComponent<UIEventListenerExtend>();
                    moveColliderWidget.name = "MoveCollider";
                    moveColliderWidget.autoResizeBoxCollider = true;
                    moveColliderWidget.SetAnchor(moveColliderPanel.gameObject, 0, 0, 0, 0);
                    moveColliderWidget.updateAnchors = UIRect.AnchorUpdate.OnEnable;
                    moveCollider = moveColliderWidget.gameObject.AddComponent<BoxCollider>();
                }
                moveColliderPanel.SetRect(0, 0, panel.width, panel.height);
                moveColliderWidget.UpdateAnchors();
                moveColliderPanel.depth = panel.depth + 1;
                moveColliderWidget.ResizeCollider();
            }
            else
            {
                Debug.LogError(string.Format("{0} has no item sample", gameObject.name));
            }
        }

        protected virtual int GetItemSampleIndex(int RealIndex)
        {
            if (null == sampleDatas || 0 == sampleDatas.Length)
            {
                return 0;
            }
            else
            {
                int L = 0;
                int H = sampleDatas.Length - 1;

                while (L <= H)
                {
                    int M = L + ((H - L) >> 1);

                    if (sampleDatas[M].startRealIndex <= RealIndex && RealIndex < sampleDatas[M].endRealIndex)
                    {
                        return sampleDatas[M].sampleIndex;
                    }
                    else if (RealIndex < sampleDatas[M].startRealIndex)
                    {
                        H = M - 1;
                    }
                    else if (sampleDatas[M].endRealIndex <= RealIndex)
                    {
                        L = M + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                Debug.LogError("no sample index:" + RealIndex);
                return -1;
            }
        }

        protected virtual UIWidget GetItemSample(int RealIndex)
        {
            var SampleIndex = GetItemSampleIndex(RealIndex);

            if (-1 != SampleIndex && SampleIndex < itemSamples.Count)
            {
                return itemSamples[SampleIndex];
            }
            else
            {
                return null;
            }
        }

        protected virtual float GetItemSize(int RealIndex)
        {
            return GetItemSize(GetItemSample(RealIndex));
        }

        protected virtual float GetItemSize(UIWidget Sample)
        {
            if (null != Sample)
            {
                if (Layout.vertical == layout)
                {
                    return Sample.height + space;
                }
                else
                {
                    return Sample.width + space;
                }
            }
            else
            {
                return 0;
            }
        }

        protected virtual float GetItemOffset(int RealIndex)
        {
            if (RealIndex < 0 || null == sampleDatas || 0 == sampleDatas.Length)
            {
                return 0;
            }
            else
            {
                float Offset = 0;

                for (int i = 0; i < sampleDatas.Length; ++i)
                {
                    if (null != itemSamples && sampleDatas[i].sampleIndex < itemSamples.Count)
                    {
                        var ItemSize = GetItemSize(itemSamples[sampleDatas[i].sampleIndex]);
                        if (sampleDatas[i].startRealIndex <= RealIndex && RealIndex < sampleDatas[i].endRealIndex)
                        {
                            Offset += sampleDatas[i].Range(RealIndex) * ItemSize;
                            break;
                        }
                        else if (sampleDatas[i].endRealIndex <= RealIndex)
                        {
                            Offset += sampleDatas[i].Range() * ItemSize;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        Debug.LogError("no such item sample index:" + sampleDatas[i].sampleIndex);
                        return 0;
                    }
                }

                return Offset;
            }
        }

        protected virtual float GetPanelSize()
        {
            if (Layout.vertical == layout)
            {
                return panel.height;
            }
            else
            {
                return panel.width;
            }
        }

        public virtual void Show(int MaxCount, int StartRealIndex)
        {
            ResetAllItems();
            if (0 == MaxCount)
            {
                maxCount = 0;
                sampleDatas = null;
            }
            else
            {
                Show(new UIListSampleData[] { new UIListSampleData(0, MaxCount, 0) }, StartRealIndex);
            }
        }

        public virtual void Show(UIListSampleData[] SampleData, int StartRealIndex)
        {
            if (null != SampleData && 0 < SampleData.Length)
            {
                if (0 != SampleData[0].startRealIndex)
                {
                    Debug.LogError("invalid sample index:0");
                    return;
                }

                for (int i = 0; i < SampleData.Length; ++i)
                {
                    if (SampleData[i].endRealIndex <= SampleData[i].startRealIndex)
                    {
                        Debug.LogError("invalid sample index:" + i);
                        return;
                    }

                    if (itemSamples.Count <= SampleData[i].sampleIndex)
                    {
                        Debug.LogError("invalid sample index:" + i);
                        return;
                    }
                }

                for (int i = 1; i < SampleData.Length; ++i)
                {
                    if (SampleData[i - 1].endRealIndex != SampleData[i].startRealIndex)
                    {
                        Debug.LogError("invalid sample index:" + i);
                        return;
                    }
                }

                maxCount = SampleData[SampleData.Length - 1].endRealIndex;
            }
            else
            {
                Debug.LogError("no sample index");
                maxCount = 0;
                sampleDatas = null;
                return;
            }

            sampleDatas = SampleData;

            ResetAllItems();

            if (null != itemSamples && 0 <= StartRealIndex)
            {
                Move(GetItemOffset(StartRealIndex - 1));
            }
        }

        public virtual UIListItem GetItem(int RealIndex)
        {
            if (items.ContainsKey(RealIndex))
            {
                return items[RealIndex];
            }
            else
            {
                return null;
            }
        }

        protected virtual float MinOffset()
        {
            return 0;
        }

        protected virtual float MaxOffset()
        {
            return GetItemOffset(maxCount);
        }

        protected virtual float GetOverMoveMinOffset()
        {
            return MinOffset() + overMoveMinOffset;
        }

        protected virtual float GetOverMoveMaxOffset()
        {
            return MaxOffset() - GetPanelSize() + overMoveMaxOffset;
        }

        public virtual int GetTopItemRealIndex()
        {
            float Offset = 0;
            int TopItemRealIndex = 0;
            for (int i = 0; i < sampleDatas.Length; ++i)
            {
                var ItemSize = GetItemSize(itemSamples[sampleDatas[i].sampleIndex]);
                var TempOffset = Offset + sampleDatas[i].Range() * ItemSize;
                if (offset <= TempOffset)
                {
                    TopItemRealIndex = TopItemRealIndex + Mathf.FloorToInt((offset - Offset) / ItemSize);
                    TopItemRealIndex = TopItemRealIndex < 0 ? 0 : TopItemRealIndex;
                    TopItemRealIndex = maxCount <= TopItemRealIndex ? maxCount - 1 : TopItemRealIndex;
                    return TopItemRealIndex;
                }
                else
                {
                    TopItemRealIndex += sampleDatas[i].Range();
                    Offset = TempOffset;
                }
            }

            return -1;
        }

        public virtual int GetBottomItemRealIndex()
        {
            return GetBottomItemRealIndex(GetTopItemRealIndex());
        }

        protected virtual int GetBottomItemRealIndex(int TopItemRealIndex)
        {
            var BottomItemRealIndex = TopItemRealIndex;
            float BottomOffset = 0;
            for (var i = TopItemRealIndex + 1; i < maxCount; ++i)
            {
                BottomOffset += GetItemSize(i);
                BottomItemRealIndex = i;
                if (GetPanelSize() < BottomOffset)
                {
                    break;
                }
            }

            return BottomItemRealIndex;
        }

        public virtual bool Move(float Offset)
        {
            if ((MinOffset() - GetPanelSize() <= Offset) && (Offset <= MaxOffset()))
            {
                offset = Offset;
                var TopItemRealIndex = GetTopItemRealIndex();
                var BottomItemRealIndex = GetBottomItemRealIndex(TopItemRealIndex);

                List<int> RecycleItems = new List<int>();
                foreach (var Item in items)
                {
                    if (Item.Key < TopItemRealIndex || BottomItemRealIndex < Item.Key)
                    {
                        RecycleItems.Add(Item.Key);
                    }
                }

                for (int i = 0; i < RecycleItems.Count; ++i)
                {
                    RecycleObject(items[RecycleItems[i]]);
                    items.Remove(RecycleItems[i]);
                }

                for (var i = TopItemRealIndex; i <= BottomItemRealIndex; ++i)
                {
                    if (!items.ContainsKey(i))
                    {
                        var NewItem = GetObject(i);
                        if (null != NewItem)
                        {
                            NewItem.name = string.Format("Item{0}", i);
                            items.Add(i, NewItem);
                            OnItemChanged(i, NewItem.gameObject);
                        }
                    }
                }

                ResetPosition();

                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual UIListItem GetObject(int RealIndex)
        {
            var SampleIndex = GetItemSampleIndex(RealIndex);
            if (objectPool.ContainsKey(SampleIndex))
            {
                if (0 < objectPool[SampleIndex].Count)
                {
                    var Temp = objectPool[SampleIndex][0];
                    objectPool[SampleIndex].RemoveAt(0);
                    Temp.gameObject.SetActive(true);
                    return Temp;
                }
            }
            else
            {
                objectPool.Add(SampleIndex, new List<UIListItem>());
            }

            {
                var Sample = GetItemSample(RealIndex);
                var Temp = NGUITools.AddChild(gameObject, Sample.gameObject).AddComponent<UIListItem>();
                Temp.widget = Temp.GetComponent<UIWidget>();
                Temp.sampleIndex = SampleIndex;
                Temp.realIndex = RealIndex;
                Temp.gameObject.SetActive(true);
                return Temp;
            }
        }

        protected virtual void ResetAllItems()
        {
            foreach (var Item in items)
            {
                RecycleObject(Item.Value);
            }

            items.Clear();
        }

        protected virtual void RecycleObject(UIListItem Obj)
        {
            if (!objectPool.ContainsKey(Obj.sampleIndex))
            {
                objectPool.Add(Obj.sampleIndex, new List<UIListItem>());
            }

            Obj.gameObject.SetActive(false);
            objectPool[Obj.sampleIndex].Add(Obj);
        }

        protected virtual void ResetPosition()
        {
            foreach (var Item in items)
            {
                var X = GetX(Item.Key);
                var Y = GetY(Item.Key);
                Item.Value.transform.localPosition = new Vector3(X, Y, 0);
            }
        }

        protected virtual float GetItemAbsolutePosition(int RealIndex)
        {
            return GetItemRelativePosition(RealIndex) + (GetPanelSize() - GetItemSize(RealIndex)) / 2;
        }

        public virtual float GetItemRelativePosition(int RealIndex)
        {
            return offset - GetItemOffset(RealIndex - 1);
        }

        public virtual float GetX(int RealIndex)
        {
            float X = 0;

            if (Layout.vertical == layout)
            {
                switch (align)
                {
                    case Align.left:
                        X = (GetItemSample(RealIndex).width - panel.width) / 2;
                        break;
                    case Align.right:
                        X = (panel.width - GetItemSample(RealIndex).width) / 2;
                        break;
                    case Align.center:
                    case Align.top:
                    case Align.bottom:
                        X = 0;
                        break;
                }
            }
            else
            {
                X = -GetItemAbsolutePosition(RealIndex);
            }

            return X;
        }

        public virtual float GetY(int RealIndex)
        {
            float Y = 0;

            if (Layout.vertical == layout)
            {
                Y = GetItemAbsolutePosition(RealIndex);
            }
            else
            {
                switch (align)
                {
                    case Align.top:
                        Y = (panel.height - GetItemSample(RealIndex).height) / 2;
                        break;
                    case Align.bottom:
                        Y = (GetItemSample(RealIndex).height - panel.height) / 2;
                        break;
                    case Align.center:
                    case Align.left:
                    case Align.right:
                        Y = 0;
                        break;
                }
            }

            return Y;
        }

        protected virtual float GetMoveOffset(float SpeedRatio)
        {
            var Offset = 0f;
            var CurrentPosition = Input.mousePosition;
            if (Layout.vertical == layout)
            {
                Offset = (CurrentPosition.y - lastPosition.y) * SpeedRatio + offset;
            }
            else
            {
                Offset = -(CurrentPosition.x - lastPosition.x) * SpeedRatio + offset;
            }

            if (overMove)
            {
                if (IsOverMove(Offset))
                {
                    Offset = offset + (Offset - offset) / overMoveDamping;
                }
            }
            else
            {
                if (Offset <= MinOffset())
                {
                    Offset = MinOffset();
                }

                if (MaxOffset() < GetPanelSize())
                {
                    Offset = 0;
                }
                else
                {
                    if (MaxOffset() - GetPanelSize() <= Offset)
                    {
                        Offset = MaxOffset() - GetPanelSize();
                    }
                }
            }

            return Offset;
        }

        protected virtual bool IsOverMove(float Offset)
        {
            return GetOverMoveMaxOffset() <= Offset || Offset <= GetOverMoveMinOffset();
        }

        protected virtual bool IsHover()
        {
            if (null != UICamera.hoveredObject)
            {
                return UICamera.hoveredObject.gameObject == moveCollider.gameObject;
            }
            else
            {
                return false;
            }
        }

        public virtual void DecelerateMove(float From, float To, float T, System.Action OnMoveFinished)
        {
            StartCoroutine(DoDecelerateMove(From, To, T, OnMoveFinished));
        }

        public virtual void DecelerateMove(float Offset, float T)
        {
            DecelerateMove(offset, offset + Offset, T, null);
        }

        protected virtual IEnumerator DoDecelerateMove(float From, float To, float T, System.Action OnMoveFinished)
        {
            var S = To - From;

            if (Mathf.Abs(S) < 10f)
            {
                T = T * Mathf.Abs(S) / 10f;
            }

            var A = 2 * S / (T * T);
            var V = A * T;

            var StartTime = Time.time;
            while (Time.time - StartTime < T)
            {
                var DeltaT = (Time.time - StartTime);
                var Offset = From + V * DeltaT - A * DeltaT * DeltaT / 2;
                Move(Offset);
                yield return null;
            }

            Move(To);

            if (null != OnMoveFinished)
            {
                OnMoveFinished();
            }
        }

        protected virtual bool MoveBack()
        {
            var CurrentMaxOffset = GetOverMoveMaxOffset();
            if (CurrentMaxOffset < offset)
            {
                CurrentMaxOffset = (CurrentMaxOffset < 0) ? 0 : CurrentMaxOffset;
                DecelerateMove(offset, CurrentMaxOffset, overMoveBackTime, null);
                return true;
            }

            var CurrentMinOffset = GetOverMoveMinOffset();
            if (offset < CurrentMinOffset)
            {
                CurrentMinOffset = (0 < CurrentMinOffset) ? 0 : CurrentMinOffset;
                DecelerateMove(offset, CurrentMinOffset, overMoveBackTime, null);
                return true;
            }

            return false;
        }

        protected virtual bool IsLeftParentRightChild(Transform Parent, Transform Child)
        {
            if (null == Parent || null == Child)
            {
                return false;
            }
            else if (Parent == Child)
            {
                return true;
            }
            else
            {
                return IsLeftParentRightChild(Parent, Child.transform.parent);
            }
        }

        protected virtual void Update()
        {
            if (null == itemSamples || null == sampleDatas)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                isInScrollView = false;
                isMove = false;
                if (!IsHover())
                {
                    return;
                }

                isInScrollView = true;
                StopAllCoroutines();
                lastPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                if (isInScrollView)
                {
                    if (Layout.vertical == layout)
                    {
                        if (0.01f < Mathf.Abs(Input.mousePosition.y - lastPosition.y))
                        {
                            isMove = true;
                        }
                    }
                    else if (Layout.horizontol == layout)
                    {
                        if (0.01f < Mathf.Abs(Input.mousePosition.x - lastPosition.x))
                        {
                            isMove = true;
                        }
                    }

                    if (isMove)
                    {
                        Move(GetMoveOffset(moveSpeedRatio));
                    }
                    lastPosition = Input.mousePosition;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (isMove)
                {
                    if (childMoveCenter)
                    {
                        SetCenterItem(GetCenterItemRealIndex(), false);
                    }
                    else
                    {
                        var Offset = GetMoveOffset(decelerationSpeedRatio);

                        if (IsOverMove(Offset))
                        {
                            MoveBack();
                        }
                        else
                        {
                            DecelerateMove(offset, Offset, decelerationDuration, () => { MoveBack(); });
                        }
                    }
                }
                else
                {
                    if (childMoveCenterOnClick)
                    {
                        UIEventListenerExtend.SendMsgToNextWidget(moveColliderWidget, "OnClick", null,
                        (Child) =>
                        {
                            foreach (var Item in items)
                            {
                                if (IsLeftParentRightChild(Item.Value.transform, Child.transform))
                                {
                                    SetCenterItem(Item.Key, false);
                                    break;
                                }
                            }
                        });
                    }
                }

                lastPosition = Vector3.zero;
                isInScrollView = false;
                isMove = false;
            }
        }

        public virtual float GetItemPosition(int RealIndex)
        {
            if (items.ContainsKey(RealIndex))
            {
                return GetItemPosition(items[RealIndex]);
            }
            else
            {
                Debug.LogWarning("not in visible area:" + RealIndex);
                return 0;
            }
        }

        protected virtual float GetItemPosition(UIListItem Obj)
        {
            if (null != Obj)
            {
                if (Layout.vertical == layout)
                {
                    return Obj.transform.localPosition.y;
                }
                else
                {
                    return Obj.transform.localPosition.x;
                }
            }
            else
            {
                return Mathf.Infinity;
            }
        }

        public virtual void SetCenterItem(int RealIndex, bool Immediately)
        {
            float Target = GetItemOffset(RealIndex) - GetPanelSize() / 2 - GetItemSize(RealIndex) / 2;

            if (Immediately)
            {
                Move(Target);
                if (null != OnItemCenter)
                {
                    if (items.ContainsKey(RealIndex))
                    {
                        OnItemCenter(RealIndex, items[RealIndex].gameObject);
                    }
                }
            }
            else
            {
                DecelerateMove(offset, Target, moveCenterDuration, () =>
                {
                    if (null != OnItemCenter)
                    {
                        if (items.ContainsKey(RealIndex))
                        {
                            OnItemCenter(RealIndex, items[RealIndex].gameObject);
                        }
                    }
                });
            }
        }

        public virtual int GetCenterItemRealIndex()
        {
            foreach (var Item in items)
            {
                if (Item.Value.gameObject.activeSelf)
                {
                    var ItemSize = GetItemSize(Item.Value.widget);
                    float Position = GetItemPosition(Item.Value);
                    if (-ItemSize / 2 < Position && Position < ItemSize / 2)
                    {
                        return Item.Key;
                    }
                }
            }

            if (offset < 0)
            {
                return 0;
            }
            else
            {
                return maxCount - 1;
            }
        }

        public virtual void EnableDrag(bool Enable)
        {
            if (null != moveCollider)
            {
                moveCollider.enabled = Enable;
            }
        }

        public virtual bool IsEnableDrag()
        {
            if (null != moveCollider)
            {
                return moveCollider.enabled;
            }
            else
            {
                return false;
            }
        }
    }
}