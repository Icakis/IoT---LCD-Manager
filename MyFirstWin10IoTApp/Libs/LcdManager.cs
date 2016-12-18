namespace MyFirstWin10IoTApp.Libs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using I2cLCD;

    public class LcdManager
    {
        private CancellationTokenSource tokenSource;
        private object lockObj = new object();
        private Action loopAction;

        public LcdManager()
        {
            this.LCD = new I2cLCD.PCF8574A();
            this.LoopAction = ShowDateTime;
            this.ActionQueue = new ConcurrentQueue<Action>();
            this.MessageQueue = new ConcurrentQueue<LcdMessage>();
            this.tokenSource = new CancellationTokenSource();
        }

        protected PCF8574A LCD { get; private set; }

        public static byte LCD_FIRST_LINE
        {
            get
            {
                return PCF8574A.LCD_LINE_1;
            }
        }

        public static byte LCD_SECOND_LINE
        {
            get
            {
                return PCF8574A.LCD_LINE_2;
            }
        }

        public Action LoopAction
        {
            get
            {
                return this.loopAction;
            }
            set
            {
                lock (this.lockObj)
                {
                    this.loopAction = value;
                }
            }
        }

        public bool IsLoopActionRunning { get; private set; }

        public ConcurrentQueue<Action> ActionQueue { get; set; }

        public ConcurrentQueue<LcdMessage> MessageQueue { get; set; }

        public async void StartLoopAction()
        {
            if (!this.tokenSource.IsCancellationRequested)
            {
                this.tokenSource.Cancel();
            }

            this.tokenSource = new CancellationTokenSource();
            await this.Loop(tokenSource.Token).ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.Canceled:
                        this.LCD.LCDMessage("Cancel Complete", PCF8574A.LCD_LINE_1);
                        this.LCD.LCDMessage("Cancel Complete", PCF8574A.LCD_LINE_2);
                        Task.Delay(5000).Wait();
                        this.IsLoopActionRunning = false;
                        break;
                    case TaskStatus.Created:
                        break;
                    case TaskStatus.Faulted:
                        this.LCD.LCDMessage("Faulted", PCF8574A.LCD_LINE_1);
                        this.LCD.LCDMessage("Faulted", PCF8574A.LCD_LINE_2);
                        Task.Delay(5000).Wait();
                        this.IsLoopActionRunning = false;
                        break;
                    case TaskStatus.RanToCompletion:
                        this.LCD.LCDMessage("Complete", PCF8574A.LCD_LINE_1);
                        this.LCD.LCDMessage("Complete", PCF8574A.LCD_LINE_2);
                        this.IsLoopActionRunning = false;
                        Task.Delay(5000).Wait();
                        break;
                    case TaskStatus.Running:
                        break;
                    case TaskStatus.WaitingForActivation:
                        break;
                    case TaskStatus.WaitingForChildrenToComplete:
                        break;
                    case TaskStatus.WaitingToRun:
                        break;
                    default:
                        break;

                }
            });
        }

        private Task Loop(CancellationToken cancelToken)
        {
            Task loop2 = Task.Run(() =>
            {
                this.IsLoopActionRunning = true;
                while (true)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    if (this.MessageQueue.Count() != 0)
                    {
                        LcdMessage nextMessga;
                        if (this.MessageQueue.TryDequeue(out nextMessga))
                        {
                            try
                            {
                                if (nextMessga.DelayBefore != 0)
                                {
                                    Task.Delay(nextMessga.DelayBefore).Wait();
                                }

                                this.LCD.LCDMessage(nextMessga.Message, nextMessga.Line);

                                if (nextMessga.DelayAfter != 0)
                                {
                                    Task.Delay(nextMessga.DelayAfter).Wait();
                                }
                            }
                            catch (Exception)
                            {
                                this.MessageQueue.Enqueue(nextMessga);
                            }
                        }
                    }
                    else if (this.ActionQueue.Count() != 0)
                    {
                        Action nextAction;
                        if (this.ActionQueue.TryDequeue(out nextAction))
                        {
                            try
                            {
                                nextAction();
                            }
                            catch (Exception)
                            {
                                this.ActionQueue.Enqueue(nextAction);
                            }
                        }
                    }
                    else
                    {
                        lock (this.lockObj)
                        {
                            this.LoopAction();
                        }
                    }
                }
            }, cancelToken);

            return loop2;
        }

        public void ShowDateTime()
        {
            var dateTimeNow = DateTime.Now;
            this.LCD.LCDMessage(dateTimeNow.ToString("dd/MM/yyyy"), PCF8574A.LCD_LINE_1);
            this.LCD.LCDMessage(dateTimeNow.ToString("HH:mm:ss zzz"), PCF8574A.LCD_LINE_2);
        }

        public void ShowAllCustomMessages()
        {
            this.LCD.LCDMessage("I DID IT!!!", PCF8574A.LCD_LINE_1);
            this.LCD.LCDMessage("I DID IT AGAIN", PCF8574A.LCD_LINE_2);
            Task.Delay(1000).Wait();
            this.LCD.LCDMessage("Dexter!FOOD!", PCF8574A.LCD_LINE_1);
            this.LCD.LCDMessage("ХРАНА!", PCF8574A.LCD_LINE_2);
            Task.Delay(1000).Wait();
            this.LCD.TurnOFFBacklight();
            Task.Delay(200).Wait();
            this.LCD.TurnONBacklight();
            Task.Delay(200).Wait();
            this.LCD.LCDMessage("Test turn ON.", PCF8574A.LCD_LINE_2);
            Task.Delay(1000).Wait();
        }

        public void StopLoop()
        {
            this.tokenSource.Cancel();
            Task.Delay(1000).Wait();
            this.LCD.LCDMessage("STOP LCD Loop", PCF8574A.LCD_LINE_1);
            this.LCD.LCDMessage("STOP LCD", PCF8574A.LCD_LINE_2);
            //Task.Delay(1000).Wait();
        }

        public void ShowTest()
        {
            Task.Delay(1000).Wait();
            this.LCD.LCDMessage("ShowTest", PCF8574A.LCD_LINE_1);
            this.LCD.LCDMessage("ShowTest2", PCF8574A.LCD_LINE_2);
        }
    }
}
