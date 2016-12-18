// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyFirstWin10IoTApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using I2cLCD;
    using Libs;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();


            LcdManager lcdManager = new LcdManager();
            //lcdManager.LoopAction = lcdManager.ShowDateTime;
            lcdManager.StartLoopAction();
            //Task.Delay(3000).Wait();
            lcdManager.ActionQueue.Enqueue(lcdManager.ShowAllCustomMessages);
            //Task.Delay(10000).Wait();
            lcdManager.LoopAction = lcdManager.ShowTest;
            //Task.Delay(2000).Wait();
            lcdManager.MessageQueue.Enqueue(new LcdMessage("Message Queue", PCF8574A.LCD_LINE_1) { DelayAfter = 2000 });
            lcdManager.MessageQueue.Enqueue(new LcdMessage("Message Queue@", LcdManager.LCD_SECOND_LINE) { DelayAfter = 3000 });
            //Task.Delay(2000).Wait();
            lcdManager.LoopAction = lcdManager.ShowDateTime;
            //Task.Delay(2000).Wait();
            lcdManager.StopLoop();
            //Task.Delay(3000).Wait();
            //lcdManager.LCD.LCDMessage("Broken1", PCF8574A.LCD_LINE_1);
            //lcdManager.LCD.LCDMessage("Broken2", PCF8574A.LCD_LINE_2);
            //Task.Delay(25000).Wait();
            //lcdManager.ActionQueue.Enqueue(lcdManager.StopLoop);
            lcdManager.StartLoopAction();
            //Task.Delay(1000).Wait();
            lcdManager.ActionQueue.Enqueue(lcdManager.ShowAllCustomMessages);
            //Task.Delay(2000).Wait();
        }
    }
}
