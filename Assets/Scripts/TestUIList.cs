using UnityEngine;
using System.Collections;

namespace orisox.com
{
    public class TestUIList : MonoBehaviour
    {
        public UIList list;

        public void OnEnable()
        {
            if (null != list)
            {
                TestSingleSample();
            }
        }

        [ContextMenu("TestSingleSample")]
        void TestSingleSample()
        {
            list.Init();
            list.OnItemChanged = OnItemChanged;
            list.Show(100, 0);
        }

        [ContextMenu("TestMultiSample")]
        void TestMultiSample()
        {
            list.Init();
            list.OnItemChanged = OnItemChanged;
            list.Show(new UIListSampleData[5] { new UIListSampleData(0, 1, 0), new UIListSampleData(1, 2, 1), new UIListSampleData(2, 5, 0), new UIListSampleData(5, 8, 1), new UIListSampleData(8, 9, 0) }, 0);
        }

        void OnItemChanged(int RealIndex, GameObject Obj)
        {
            Obj.GetComponentInChildren<UILabel>().text = string.Format("Item{0}", RealIndex);
        }
    }
}
