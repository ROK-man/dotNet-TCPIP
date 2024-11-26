using System.Text;
using System.Text.Json;

namespace ChattingServer
{
    internal class Message
    {
        // 프로토콜 종류
        public static readonly int ERROR = -1;
        public static readonly int TEXT = 0;
        public static readonly int GETFILELIST = 50;
        public static readonly int GETFILE = 100;

        public MessageHeader Header;
        public MessagePayload Payload;

        public const int HEADERLENGTH = 20;

        public Message(int protocol, int textLength, int userID, string text)
        {
            this.Payload = new MessagePayload(text);
            this.Header = new MessageHeader(protocol, textLength + 20, GetCheckSum(Payload.Text), userID);
        }

        public bool CheckHeader()
        {
            return this.Header.IsHeader();
        }

        public bool CompareCheckSum()
        {
            return this.Header.CheckSum == GetCheckSum(this.Payload.Text);
        }

        public static byte[] Serialize(object m)
        {
            var jsonData = JsonSerializer.SerializeToUtf8Bytes(m);
            var lengthBytes = BitConverter.GetBytes(jsonData.Length);
            return lengthBytes.Concat(jsonData).ToArray(); // 헤더(4바이트) + JSON 데이터
        }
        public static Message Deserialize(byte[] data)
        {
            return JsonSerializer.Deserialize<Message>(data);
        }
        public static MessageHeader DeserializeHeader(byte[] data)
        {
            return JsonSerializer.Deserialize<MessageHeader>(data);
        }

        private static int GetCheckSum(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return 0;
            }

            byte[] data = Encoding.UTF8.GetBytes(text);

            int checksum = 0;
            foreach (byte b in data)
            {
                checksum += b;
            }
            return checksum & 0x0FFFFFFF; // 4-byte checksum
        }
    }

    // 크기가 20바이트 고정인 구조체
    public struct MessageHeader
    {
        public int Protocol;
        public int TotalLength;
        public int CheckSum;
        public int UserID;
        private const int Key = 0x12345678;

        public MessageHeader(int protocol, int length, int sum, int userID)
        {
            Protocol = protocol;
            TotalLength = length;
            CheckSum = sum;
            UserID = userID;
        }

        public bool IsHeader()
        {
            return Key == 0x12345678;
        }
    }


    class MessagePayload(string text)
    {
        public string Text { get; set; } = text;
    }


}