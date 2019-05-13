using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Helper
{
    class GridWidthHelper
    {
        /// <summary>
        /// 获取自适应列表项宽度。
        /// </summary>
        /// <param name="WindowWidth">可用窗口宽度</param>
        /// <param name="min">项目最小宽度</param>
        /// <param name="spacing">项目间空隙</param>
        public static double GetWidth(double WindowWidth, int min, int spacing = 5)
        {
            double width = 1;
            int column = 1;
            int maxcolumn = (int)WindowWidth / min;
            double j = WindowWidth / min;
            column = (int)j == 0 ? 1 : (int)j;
            width = WindowWidth / column;
            width -= spacing;
            return width;
        }
    }
}
