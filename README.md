# HCVNC
A vnc client dll for c#, written in c/c++ and c#, depends on TightVnc 2.7.10, so it can use Tight to push screen faster than vncsharp use less wifi.<br>
It contains a core dll:`HCVncCore.dll`, a common dll:`HCVNC.dll`.<br>
One thing you need to know that 1.0.0 can only to show screen, can not control or send any file.<br>
Email:390088762@qq.com
# How To Use
Those dlls depend on .Net 4.0, so you can use them on XP.<br><br>
You can download dlls at:<br>
2016-09-09 [1.0.0 Click to download](https://yunpan.cn/ckgJwmStYAiMg)  secret:8467<br>
## Use HCVncCore.dll and HCVNC.dll
1.You can import HCVNC.dll to your project and push HCVncCore.dll to your EXE folder(debug or release).<br>
2.You can find ScreenBox(a UserControl) in your project, you can use it!<br>
3.Use these codes to push screen:<br>
```c#
        public Form1()
        {
            InitializeComponent();

            screenBox1.PrepareToStart();
            screenBox1.ScreenState += RemoteScreen_ScreenState;
        }
        
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
```

You can find these in DEMO project.
