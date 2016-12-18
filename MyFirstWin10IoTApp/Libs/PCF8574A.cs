namespace I2cLCD
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Devices.I2c;

    public class PCF8574A
    {
        // raspberry Pi3 "I2C1"
        private string I2C_CONTROLLER_NAME;

        // I2C device address : default for PCF8574A is 0x3F, for PCF8574 is 0x27
        private byte I2C_ADDR;
        private const byte LCD_WIDTH = 16;   // Maximum characters per line

        //# Define some device constants
        private const byte LCD_CHR = 1; // Mode - Sending data
        private const byte LCD_CMD = 0; // Mode - Sending command

        public const byte LCD_LINE_1 = 0x80; // LCD RAM address for the 1st line
        public const byte LCD_LINE_2 = 0xC0; // LCD RAM address for the 2nd line
        public const byte LCD_LINE_3 = 0x94; // LCD RAM address for the 3rd line
        public const byte LCD_LINE_4 = 0xD4; // LCD RAM address for the 4th line

        //LCD_BACKLIGHT = 0x00  # Off
        private byte LCD_BACKLIGHT;  // On
        private const byte LCD_BACKLIGHT_ON = 0x08;
        private const byte LCD_BACKLIGHT_OFF = 0x00;

        private const byte ENABLE = 4; // 0x00000100; // Enable bit

        // Timing constants
        private const int E_PULSE = 1; //0.0005;
        private const int E_DELAY = 1; //0.0005;

        //Open I2C interface
        //bus = smbus.SMBus(0)  # Rev 1 Pi uses 0
        //bus = smbus.SMBus(1) # Rev 2 Pi uses 1

        private I2cDevice lcd;

        private bool isInitialized;

        private object lockWriteObj = new object();

        public PCF8574A(byte address = 0x3F, string controllerName = "I2C1", bool initNow = true)
        {
            this.I2C_ADDR = address;
            this.I2C_CONTROLLER_NAME = controllerName;
            this.LCD_BACKLIGHT = LCD_BACKLIGHT_ON;
            if (initNow)
            {
                this.Init().Wait();
            }
        }

        public async Task<bool> Init()
        {
            var settings = new I2cConnectionSettings(this.I2C_ADDR);
            //settings.SharingMode = I2cSharingMode.Shared;
            //settings.BusSpeed = I2cBusSpeed.FastMode;
            // Create an I2cDevice with the specified I2C settings
            var controller = await I2cController.GetDefaultAsync();
            this.lcd = controller.GetDevice(settings);
            this.isInitialized = true;

            ClearDisplay();

            return isInitialized;
        }

        public void ClearDisplay()
        {
            // Clear display
            lcdByteWithMode(0x33, LCD_CMD); // 110011 Initialise
            lcdByteWithMode(0x32, LCD_CMD); // 110010 Initialise
            lcdByteWithMode(0x06, LCD_CMD); // 000110 Cursor move direction
            lcdByteWithMode(0x0C, LCD_CMD); // 001100 Display On,Cursor Off, Blink Off 
            lcdByteWithMode(0x28, LCD_CMD); // 101000 Data length, number of lines, font size
            lcdByteWithMode(0x01, LCD_CMD); // 000001 Clear display
        }

        private void lcdByteWithMode(byte bits, byte mode)
        {
            // Send byte to data pins
            // bits = the data
            // mode = 1 for data
            //        0 for command
            byte bits_high = Convert.ToByte(mode | (bits & 0xF0) | LCD_BACKLIGHT);
            byte bits_low = Convert.ToByte(mode | ((bits << 4) & 0xF0) | LCD_BACKLIGHT);

            // High bits
            lcd.Write(new byte[] { bits_high });
            toggleLCDOnOFF(bits_high);

            // Low bits
            lcd.Write(new byte[] { bits_low });
            toggleLCDOnOFF(bits_low);
        }

        private void toggleLCDOnOFF(byte mode)
        {
            // Toggle enable
            //Task.Delay(E_DELAY).Wait();
            lcd.Write(new byte[] { Convert.ToByte(mode | ENABLE) });
            //Task.Delay(E_PULSE).Wait();
            lcd.Write(new byte[] { Convert.ToByte(mode & ~ENABLE) });
            //Task.Delay(E_DELAY).Wait();
        }

        //public async void LCDMessageAsync(string message, byte line)
        //{
        //    var task = Task.Run(() =>
        //     {
        //         LCDMessage(message, line);
        //     });

        //    await task;
        //}

        public void LCDMessage(string message, byte line)
        {
            if (!isInitialized)
            {
                this.Init().Wait();
            }

            // Send string to display
            message = message.PadRight(LCD_WIDTH, ' ');
            lock (lockWriteObj)
            {
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                lcdByteWithMode(line, LCD_CMD);
                for (int i = 0; i < LCD_WIDTH; i++)
                {
                    lcdByteWithMode(messageBytes[i], LCD_CHR);
                }
            }
        }

        public void TurnOFFBacklight()
        {
            this.LCD_BACKLIGHT = LCD_BACKLIGHT_OFF;
            lcdByteWithMode(1, LCD_CMD);
        }

        public void TurnONBacklight()
        {
            this.LCD_BACKLIGHT = LCD_BACKLIGHT_ON;
            lcdByteWithMode(1, LCD_CMD);
        }
    }
}
