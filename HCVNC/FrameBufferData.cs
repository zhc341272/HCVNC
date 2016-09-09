using System;
namespace HCVNC
{
    /// <summary>
    /// framebuffer数据类
    /// </summary>
    class FrameBufferData
    {
        public IntPtr data;
        public int length;
        public int w;
        public int h;
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
