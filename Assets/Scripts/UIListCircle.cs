using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace orisox.com
{
    public class UIListCircle : UIList
    {
        public int radius = 500;
        public Vector2 center;

        protected virtual float GetItemPositionCircle(int RealIndex)
        {
            float A = GetItemAbsolutePosition(RealIndex);

            if (radius < A)
            {
                A = radius;
            }
            else if (A < -radius)
            {
                A = -radius;
            }

            var B = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(A, 2));

            if (Layout.vertical == layout)
            {
                return B + center.x - (panel.width) / 2;
            }
            else
            {
                return B + center.y - (panel.height) / 2;
            }
        }

        public override float GetX(int RealIndex)
        {
            float X = 0;

            if (Layout.vertical == layout)
            {
                switch (align)
                {
                    case Align.left:
                        X = GetItemPositionCircle(RealIndex);
                        break;
                    case Align.right:
                        X = -GetItemPositionCircle(RealIndex);
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

        public override float GetY(int RealIndex)
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
                        Y = -GetItemPositionCircle(RealIndex);
                        break;
                    case Align.bottom:
                        Y = GetItemPositionCircle(RealIndex);
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
    }
}
