namespace MyFirstWin10IoTApp.Libs
{
    public class LcdMessage
    {
        public LcdMessage(string message, byte line)
        {
            this.Message = message;
            this.Line = line;
        }

        public string Message { get; set; }

        public byte Line { get; set; }

        public int DelayBefore { get; set; }

        public int DelayAfter { get; set; }
    }
}
