using System;
using System.Drawing;
using Learnland.DotNetCore.Extensions;

namespace Learnland.DotNetCore.PptxAnalysis.Image
{
    /// <summary>
    ///     设置透明色
    /// </summary>
    public class SetTransparentColor
    {
        /// <summary>
        ///     将图片<paramref name="bitmap"/>上指定的颜色<paramref name="colorA"/>替换为颜色<paramref name="colorB"/>
        /// </summary>
        /// <param name="bitmap">图片</param>
        /// <param name="colorA">要被替换的颜色</param>
        /// <param name="colorB">要将<paramref name="colorA"/>替换的成颜色</param>
        public void ReplaceColor(Bitmap bitmap, Color colorA, Color colorB)
        {
            //这里是遍历图片中的每一个像素
            bitmap.PerPixelProcess(color =>
            {
                //如果当前的颜色和颜色colorA近似，则进行替换
                var isSimilar = IsSimilarColors(color, colorA);
                return isSimilar ? colorB : color;
            });
        }

        /// <summary>
        ///     是否是近似颜色
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="accuracy">Rgb通道允许的误差</param>
        /// <returns></returns>
        private bool IsSimilarColors(Color x, Color y, int accuracy = 36)
        {
            var offsetA = x.A - y.A;
            var offsetR = x.R - y.R;
            var offsetG = x.G - y.G;
            var offsetB = x.B - y.B;

            if (Math.Abs(offsetA) > 1)
            {
                return false;
            }

            if (offsetR == offsetG && offsetR == offsetB)
            {
                if (Math.Abs(offsetR) > 1)
                {
                    return ColorDifference(x, y) <= accuracy / 3d;
                }
            }

            var difference = ColorDifference(x, y);
            return difference <= accuracy;
        }

        /// <summary>
        /// 在RGB空间上通过公式计算出加权的欧式距离
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private double ColorDifference(Color x, Color y)
        {
            var m = (x.R + y.R) / 2.0;
            var r = Math.Pow(x.R - y.R, 2);
            var g = Math.Pow(x.G - y.G, 2);
            var b = Math.Pow(x.B - y.B, 2);

            return Math.Sqrt((2 + m / 256) * r + 4 * g + (2 + (255 - m) / 256) * b);
        }
    }
}
