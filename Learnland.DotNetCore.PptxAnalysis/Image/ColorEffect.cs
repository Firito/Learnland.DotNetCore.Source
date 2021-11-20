using System;
using System.Drawing;
using Learnland.DotNetCore.Drawing;

namespace Learnland.DotNetCore.PptxAnalysis.Image
{
    /// <summary>
    ///     PPTX颜色效果
    /// </summary>
    public class ColorEffect
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
                var isSimilar = color.IsSimilarColors(colorA);
                return isSimilar ? colorB : color;
            });
        }

        /// <summary>
        ///     设置黑白图效果
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="threshold">像素灰度大于该阈值设为白色，否则为黑色。范围 0-1</param>
        public void SetBlackWhiteEffect(Bitmap bitmap, float threshold)
        {
            //这里是遍历图片中的每一个像素
            bitmap.PerPixelProcess(color =>
            {
                //如果当前的颜色灰度大于等于该阈值设为白色，否则为黑色
                var rgb = color.GetGrayScale() >= threshold ? Color.White : Color.Black;
                //此处需要注意不能改变原始像素的Alpha值
                return Color.FromArgb(color.A, rgb);
            });
        }
    }
}
