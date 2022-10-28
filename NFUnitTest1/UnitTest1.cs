using nanoFramework.TestFramework;
using System;
using System.Collections;
using System.Text;
using nanoFramework.Hardware.Esp32.Rmt;

namespace NFUnitTest1
{
    [TestClass]
    public class Test1
    {
        [TestMethod]
        public void ShouldDecodeNEC()
        {
            RmtCommand[] response = new RmtCommand[]
            {
                new RmtCommand(9200,false,4100, true),
            };
            var data = Decode(response);
            Assert.Equal("NEC", "NEC");
        }

        public SignalData Decode(RmtCommand[] response)
        {
            SignalData signalData = new SignalData();
            var pulses = new ArrayList();
            StringBuilder sb = new StringBuilder();
            var firstPulse = response[0];
            var lastPulse = response[response.Length - 1];
            bool isNecHeaderMark = firstPulse.Duration0 > 9000 && !firstPulse.Level0 && firstPulse.Duration1 > 4000 && firstPulse.Level1;
            bool isEndOfTransmission = lastPulse.Duration0 < 650 && !lastPulse.Level0 && lastPulse.Duration1 == 0 && lastPulse.Level1;
            foreach (var rmtCommand in response)
            {
                if (rmtCommand.Duration0 > 9000 && rmtCommand.Duration1 > 4000)
                {
                    //do nothing, header
                }
                else if (rmtCommand.Duration0 + rmtCommand.Duration1 > 2000)
                {
                    pulses.Add(1);
                    sb.Append("1");
                }
                else if (rmtCommand.Duration0 + rmtCommand.Duration1 > 1000)
                {
                    pulses.Add(0);
                    sb.Append("0");
                }

            }

            var message = sb.ToString();
            Console.WriteLine($"Binary:{message}");
            //byte.TryParse(sb.ToString(), out byte address);
            //Console.WriteLine($"Address:{address}");

            //NEC protocol
            var c = response.Length * 2;
            if (isNecHeaderMark && c == 68 && isEndOfTransmission)
            {
                Console.WriteLine("Protocol: NEC");
                signalData.Protocol = "NEC";
                signalData.Address = message.Substring(0, 8);
                signalData.Command = message.Substring(16, 8);
            }

            return signalData;
        }
    }

    public class SignalData
    {
        public string Address { set; get; }
        public string Command { set; get; }
        public string Protocol { get; set; }
    }
}
