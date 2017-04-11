using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace orisox.com
{
    [CustomEditor(typeof(UIListCircle))]
    public class UIListCircleEditor : UIListEditor
    {
        public enum CircleHorizontolAlign
        {
            top = UIList.Align.top,
            bottom = UIList.Align.bottom,
        }

        public enum CircleVerticalAlign
        {
            left = UIList.Align.left,
            right = UIList.Align.right,
        }

        public override void OnInspectorGUI()
        {
            var Obj = (UIListCircle)target;

            List<string> hideProperties = new List<string>();
            hideProperties.Add("layout");
            hideProperties.Add("align");

            Obj.layout = (UIList.Layout)EditorGUILayout.EnumPopup("Layout", Obj.layout);
            if (UIList.Layout.horizontol == Obj.layout)
            {
                if (System.Enum.IsDefined(typeof(CircleHorizontolAlign), (int)Obj.align))
                {
                    CircleHorizontolAlign NewAlign = (CircleHorizontolAlign)Obj.align;
                    Obj.align = (UIList.Align)EditorGUILayout.EnumPopup("Align", NewAlign);
                }
                else
                {
                    Obj.align = UIList.Align.top;
                }
            }
            else
            {
                if (System.Enum.IsDefined(typeof(CircleVerticalAlign), (int)Obj.align))
                {
                    CircleVerticalAlign NewAlign = (CircleVerticalAlign)Obj.align;
                    Obj.align = (UIList.Align)EditorGUILayout.EnumPopup("Layout", NewAlign);
                }
                else
                {
                    Obj.align = UIList.Align.left;
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