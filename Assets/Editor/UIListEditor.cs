using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace orisox.com
{
    [CustomEditor(typeof(UIList))]
    public class UIListEditor : Editor
    {
        public enum HorizontolAlign
        {
            center = UIList.Align.center,
            top = UIList.Align.top,
            bottom = UIList.Align.bottom,
        }

        public enum VerticalAlign
        {
            center = UIList.Align.center,
            left = UIList.Align.left,
            right = UIList.Align.right,
        }

        public override void OnInspectorGUI()
        {
            var Obj = (UIList)target;

            List<string> hideProperties = new List<string>();
            hideProperties.Add("layout");
            hideProperties.Add("align");

            Obj.layout = (UIList.Layout)EditorGUILayout.EnumPopup("Layout", Obj.layout);
            if (UIList.Layout.horizontol == Obj.layout)
            {
                if (System.Enum.IsDefined(typeof(HorizontolAlign), (int)Obj.align))
                {
                    HorizontolAlign NewAlign = (HorizontolAlign)Obj.align;
                    Obj.align = (UIList.Align)EditorGUILayout.EnumPopup("Align", NewAlign);
                }
                else
                {
                    Obj.align = UIList.Align.center;
                }
            }
            else
            {
                if (System.Enum.IsDefined(typeof(VerticalAlign), (int)Obj.align))
                {
                    VerticalAlign NewAlign = (VerticalAlign)Obj.align;
                    Obj.align = (UIList.Align)EditorGUILayout.EnumPopup("Align", NewAlign);
                }
                else
                {
                    Obj.align = UIList.Align.center;
                }
            }

            serializedObject.Update();
            if (!Obj.overMove)
            {
                hideProperties.Add("overMoveBackTime");
                hideProperties.Add("overMoveDamping");
                hideProperties.Add("overMoveMinOffset");
                hideProperties.Add("overMoveMaxOffset");
            }
            DrawPropertiesExcluding(serializedObject, hideProperties.ToArray());
            serializedObject.ApplyModifiedProperties();
        }
    }
}
