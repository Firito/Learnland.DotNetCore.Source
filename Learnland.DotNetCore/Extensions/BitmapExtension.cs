using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Learnland.DotNetCore.Extensions
{
    /// <summary>
    ///     提供<see cref="Bitmap"/>使用的拓展方法
    /// </summary>
    public static class BitmapExtension
    {
        /// <summary>
        ///     对图像进行逐像素处理
        /// </summary>
        /// <param name="bitmap">源图像</param>
        /// <param name="func">处理回调函数</param>
        public static void PerPixelProcess(this Bitmap bitmap, Func<Color, Color> func)
        {
            var pixelFormat = bitmap.PixelFormat;

            if (pixelFormat != PixelFormat.Format32bppArgb && pixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new NotSupportedException($"Unsupported image pixel format {nameof(pixelFormat)} is used.");
            }

            var cols = bitmap.Width;
            var rows = bitmap.Height;
            var channels = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
            var total = cols * rows * channels;

            //锁定图片并拷贝图片像素
            var rect = new Rectangle(0, 0, cols, rows);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var iPtr = bitmapData.Scan0;
            var data = new byte[total];
            Marshal.Copy(iPtr, data, 0, total);

            //逐像素处理
            Parallel.For(0, rows, row =>
            {
                for (var col = 0; col < cols; col++)
                {
                    var indexOffset = (row * cols + col) * channels;
                    if (!TryCreateColorFromData(data, indexOffset, channels, out var color)) continue;

                    var targetColor = func?.Invoke(color);
                    if (targetColor != null)
                    {
                        SaveToData(targetColor.Value, data, indexOffset, channels);
                    }
                }
            });

            Marshal.Copy(data, 0, iPtr, total);
            bitmap.UnlockBits(bitmapData);
        }

        /// <summary>
        ///     尝试获取颜色
        /// </summary>
        /// <param name="colorData"></param>
        /// <param name="offset"></param>
        /// <param name="channels"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static bool TryCreateColorFromData(byte[] colorData, int offset, int channels, out Color color)
        {
            //需要考虑大小端
            var isLittleEndian = BitConverter.IsLittleEndian;
            color = Color.Black;

            try
            {
                if (channels == 3)
                {
                    var r = isLittleEndian ? colorData[offset + 2] : colorData[offset];
                    var g = colorData[offset + 1];
                    var b = isLittleEndian ? colorData[offset] : colorData[offset + 2];
                    color = Color.FromArgb(byte.MaxValue, r, g, b);
                    return true;
                }

                if (channels == 4)
                {
                    var a = isLittleEndian ? colorData[offset + 3] : colorData[offset + 0];
                    var r = isLittleEndian ? colorData[offset + 2] : colorData[offset + 1];
                    var g = isLittleEndian ? colorData[offset + 1] : colorData[offset + 2];
                    var b = isLittleEndian ? colorData[offset + 0] : colorData[offset + 3];
                    color = Color.FromArgb(a, r, g, b);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        ///     保存颜色到数组
        /// </summary>
        /// <param name="color"></param>
        /// <param name="colorData"></param>
        /// <param name="offset"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        private static void SaveToData(Color color, byte[] colorData, int offset, int channels)
        {
            //需要考虑大小端
            var isLittleEndian = BitConverter.IsLittleEndian;

            if (channels == 3)
            {
                colorData[offset] = isLittleEndian ? color.B : color.R;
                colorData[offset + 1] = color.G;
                colorData[offset + 2] = isLittleEndian ? color.R : color.B;
            }

            if (channels == 4)
            {
                colorData[offset + 0] = isLittleEndian ? color.B : color.A;
                colorData[offset + 1] = isLittleEndian ? color.G : color.R;
                colorData[offset + 2] = isLittleEndian ? color.R : color.G;
                colorData[offset + 3] = isLittleEndian ? color.A : color.B;
            }
        }
    }
}
