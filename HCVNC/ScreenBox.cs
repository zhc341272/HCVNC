using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Timers;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace HCVNC
{
    public partial class ScreenBox : UserControl
    {
        #region 链接DLL代码段
        [DllImport("/HCVncCore.dll", EntryPoint = "SetIP", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIP([MarshalAs(UnmanagedType.LPWStr)]string ip);//配置需要推屏的IP

        [DllImport("/HCVncCore.dll", EntryPoint = "StartVNCClient", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartVNCClient();//启动VNC客户端

        [DllImport("/HCVncCore.dll", EntryPoint = "CloseVNCClient", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseVNCClient();//关闭VNC客户端

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CSCallback_DEBUG(string tick);//调用委托
        static CSCallback_DEBUG callback_DEBUG;//委托实例
        [DllImport("/HCVncCore.dll", EntryPoint = "SetCallback_DEBUG", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCallback_DEBUG(CSCallback_DEBUG callback);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CSCallback(string tick);//调用委托
        static CSCallback callback;//委托实例
        [DllImport("/HCVncCore.dll", EntryPoint = "SetCallback", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCallback(CSCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CSCallUpdateback(IntPtr date, int length, int w, int h, int left, int top, int right, int bottom);//屏幕数据刷新的代理
        static CSCallUpdateback callupdateback;//委托实例
        [DllImport("/HCVncCore.dll", EntryPoint = "SetUpdateCallback", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetUpdateCallback(CSCallUpdateback callback);//设置屏幕刷新数据的回调函数
        #endregion

        private bool timerlock = false;//timer锁

        Graphics m_maing;
        Bitmap m_mainimg;
        public string m_IP = "";//用于保存推屏IP的变量
        private FrameBufferData m_framebufferdata;//缓冲数据
        private System.Timers.Timer m_updatetimer;//刷新屏幕使用的timer事件
        private Rectangle m_truedrawrect = new Rectangle(0, 0, 0, 0);//真实的矩形绘制区域

        public delegate void ScreenStateHandle(string code);
        public event ScreenStateHandle ScreenState;

        public ScreenBox()
        {
            InitializeComponent();   
        }

        /// <summary>
        /// 外部调用，准备启动推屏
        /// </summary>
        public void PrepareToStart()
        {
            SetCallBack();
            SetGraphics();
        }

        /// <summary>
        /// 设置绘制对象
        /// </summary>
        private void SetGraphics()
        {
            m_maing = OSPictureBox.CreateGraphics();
            m_maing.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            m_maing.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            m_maing.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
        }

        /// <summary>
        /// 批量设置dll的回调函数
        /// </summary>
        private void SetCallBack()
        {
            //委托指向
            callback_DEBUG = CSCallbackFunction_DEBUG;
            callback = CSCallbackFunction;
            callupdateback = GetUpdateDate;

            SetCallback_DEBUG(callback_DEBUG);
            SetCallback(callback);
            SetUpdateCallback(callupdateback);   
        }

        /// <summary>
        /// 用于c++回调的函数(DEBUG)
        /// </summary>
        /// <param name="tick"></param>
        private void CSCallbackFunction_DEBUG(string tick)
        {
            Trace.WriteLine("接收到c++的回调指令:" + tick);
        }

        /// <summary>
        /// 用于c++回调的函数，并将状态信息传递出去
        /// </summary>
        /// <param name="tick"></param>
        private void CSCallbackFunction(string tick)
        {
            ScreenState?.Invoke(tick);
        }

        /// <summary>
        /// 回调的屏幕刷新函数，对关键数据进行赋值
        /// </summary>
        /// <param name="date"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        private void GetUpdateDate(IntPtr date, int length, int w, int h, int left, int top, int right, int bottom)
        {
            if (m_framebufferdata == null)
            {
                m_framebufferdata = new FrameBufferData();
            }

            m_framebufferdata.data = date;
            m_framebufferdata.length = length;
            m_framebufferdata.w = w;
            m_framebufferdata.h = h;
            m_framebufferdata.left = left;
            m_framebufferdata.top = top;
            m_framebufferdata.right = right;
            m_framebufferdata.bottom = bottom;
        }

        /// <summary>
        /// 外部调用，开始进行推屏
        /// </summary>
        /// <param name="IP"></param>
        public void StartPushScreen(string IP)
        {
            if (m_updatetimer != null)
            {
                m_updatetimer.Stop();
                m_updatetimer.Dispose();
                m_updatetimer = null;
            }

            timerlock = false;
            m_mainimg = null;
            m_framebufferdata = null;
            m_truedrawrect = new Rectangle(0, 0, 0, 0);
            m_IP = IP;
            //将需要推屏的IP传递进入DLL
            SetIP(m_IP);

            StartVNCClient();

            //启动timer事件，准备刷新屏幕数据
            if (m_updatetimer == null)
            {
                m_updatetimer = new System.Timers.Timer(300);
                m_updatetimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateImage);
                m_updatetimer.AutoReset = true;
                m_updatetimer.Enabled = true;
            }
        }

        /// <summary>
        /// timer事件决定的屏幕刷新事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateImage(object sender, ElapsedEventArgs e)
        {
            if (timerlock)
            {
                return;
            }
            timerlock = true;
            if (m_framebufferdata != null)
            {
                Update(m_framebufferdata);
            }
            timerlock = false;
        }

        /// <summary>
        /// 刷新屏幕
        /// </summary>
        /// <param name="fb"></param>
        [HandleProcessCorruptedStateExceptions]
        private void Update(FrameBufferData fb)
        {
            IntPtr date = fb.data;
            int length = fb.length;
            int w = fb.w;
            int h = fb.h;
            int left = fb.left;
            int top = fb.top;
            int right = fb.right;
            int bottom = fb.bottom;

            //lock (locker)
                try
                {
                    if (m_mainimg == null)
                    {
                        m_mainimg = new Bitmap(w, h, PixelFormat.Format32bppRgb);
                    }

                    if (m_truedrawrect.Width == 0)
                    {
                        if ((w * OSPictureBox.Height) >= (OSPictureBox.Width * h))
                        {
                            m_truedrawrect.Width = OSPictureBox.Width;
                            m_truedrawrect.Height = (OSPictureBox.Width * h) / w;
                            m_truedrawrect.X = 0;
                            m_truedrawrect.Y = (OSPictureBox.Height - m_truedrawrect.Height) / 2;
                        }
                        else
                        {
                            m_truedrawrect.Height = OSPictureBox.Height;
                            m_truedrawrect.Width = (OSPictureBox.Height * w) / h;
                            m_truedrawrect.X = (OSPictureBox.Width - m_truedrawrect.Width) / 2;
                            m_truedrawrect.Y = 0;
                        }

                        m_maing = OSPictureBox.CreateGraphics();
                        m_maing.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        m_maing.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                        m_maing.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                    }

                    byte[] m_imgData = new byte[length];

                    try
                    {
                        Marshal.Copy(date, m_imgData, 0, length);
                        BitmapData bitdata = m_mainimg.LockBits(
                            new Rectangle(0, 0, m_mainimg.Width, m_mainimg.Height),
                            ImageLockMode.WriteOnly,
                            PixelFormat.Format32bppRgb);
                        Marshal.Copy(m_imgData, 0, bitdata.Scan0, m_imgData.Length);
                        m_mainimg.UnlockBits(bitdata);
                        m_maing.DrawImage(m_mainimg,
                        m_truedrawrect,
                        new Rectangle(0, 0, w, h),
                        GraphicsUnit.Pixel);
                    }
                    catch (AccessViolationException)
                    {
                        Trace.WriteLine("内存托管错误");
                    }
                }
                catch (Exception)
                {
                    
                }

        }

        /// <summary>
        /// 外部调用，关闭推屏
        /// </summary>
        [HandleProcessCorruptedStateExceptions]
        public void CloseScreen()
        {
            try
            {
                if (m_updatetimer != null)
                {
                    m_updatetimer.Stop();
                    m_updatetimer.Dispose();
                    m_updatetimer = null;
                    CloseVNCClient();
                }
            }
            catch (AccessViolationException)
            {
                Trace.WriteLine("内存托管错误");
            }     
        }

        /// <summary>
        /// 组件改变大小时使用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResizeSelf(object sender, EventArgs e)
        {
            if (OSPictureBox != null)
            {
                if (m_framebufferdata != null)
                {
                    if ((m_framebufferdata.w * OSPictureBox.Height) >= (OSPictureBox.Width * m_framebufferdata.h))
                    {
                        m_truedrawrect.Width = OSPictureBox.Width;
                        m_truedrawrect.Height = (OSPictureBox.Width * m_framebufferdata.h) / m_framebufferdata.w;
                        m_truedrawrect.X = 0;
                        m_truedrawrect.Y = (OSPictureBox.Height - m_truedrawrect.Height) / 2;
                    }
                    else
                    {
                        m_truedrawrect.Height = OSPictureBox.Height;
                        m_truedrawrect.Width = (OSPictureBox.Height * m_framebufferdata.w) / m_framebufferdata.h;
                        m_truedrawrect.X = (OSPictureBox.Width - m_truedrawrect.Width) / 2;
                        m_truedrawrect.Y = 0;
                    }
                }

                m_maing = OSPictureBox.CreateGraphics();
                m_maing.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                m_maing.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                m_maing.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            }
        }

    }
}
