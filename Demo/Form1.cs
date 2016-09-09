using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            screenBox1.PrepareToStart();
            screenBox1.ScreenState += RemoteScreen_ScreenState;
        }

        /// <summary>
        /// 接收到推屏状态反馈事件之后进行的函数
        /// </summary>
        /// <param name="code"></param>
        private void RemoteScreen_ScreenState(string code)
        {
            switch (code)
            {
                case "success"://connect
                    break;
                case "close":
                    break;
                case "error":
                    break;
                default:
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (screenBox1.InvokeRequired)
            {
                screenBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    screenBox1.StartPushScreen(textBox1.Text);
                }));
            }
            else
            {
                screenBox1.StartPushScreen(textBox1.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (screenBox1.InvokeRequired)
            {
                screenBox1.BeginInvoke(new MethodInvoker(delegate
                {
                    screenBox1.CloseScreen();
                }));
            }
            else
            {
                screenBox1.CloseScreen();
            }
        }
    }
}
