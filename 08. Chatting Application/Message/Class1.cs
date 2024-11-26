using System.Text;

namespace DLL
{
    // Message 클래스 정의
    public class Message
    {
        public static readonly int ERROR = -1;
        public static readonly int TEXT = 0;
        public static readonly int GETFILELIST = 50;
        public static readonly int GETFILE = 100;

        public MessageHeader Header;
        public MessagePayload Payload;

        public const int HEADERLENGTH = 20;

        public Message(MessageHeader header, MessagePayload payload)
        {
            this.Header = header;
            this.Payload = payload;
        }

        public Message(int protocol, int textLength, int userID, string text)
        {
            this.Payload = new MessagePayload(text);
            this.Header = new MessageHeader(protocol, textLength + HEADERLENGTH, GetCheckSum(Payload.Text), userID);
        }

        public byte[] ToBytes()
        {
            byte[] headerBytes = Header.ToBytes();
            byte[] payloadBytes = Payload.ToBytes();

            byte[] data = new byte[headerBytes.Length + payloadBytes.Length];
            Buffer.BlockCopy(headerBytes, 0, data, 0, headerBytes.Length);
            Buffer.BlockCopy(payloadBytes, 0, data, headerBytes.Length, payloadBytes.Length);

            return data;
        }

        public static Message ParseByte(byte[] data)
        {
            MessageHeader header = MessageHeader.ParseByte(data);
            MessagePayload payload = MessagePayload.ParseByte(data, 20, header.TotalLength - 20);

            return new Message(header, payload);
        }

        public bool CheckHeader()
        {
            return this.Header.IsHeader();
        }

        public bool CompareCheckSum()
        {
            return this.Header.CheckSum == GetCheckSum(this.Payload.Text);
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

    // MessageHeader 구조체
    public struct MessageHeader
    {
        public int Protocol;
        public int TotalLength;
        public int CheckSum;
        public int UserID;
        public int Key;

        public MessageHeader(int protocol, int length, int sum, int userID)
        {
            Protocol = protocol;
            TotalLength = length;
            CheckSum = sum;
            UserID = userID;
            Key = 0x12345678;
        }

        public bool IsHeader()
        {
            return Key == 0x12345678;
        }

        public byte[] ToBytes()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(Protocol));
            data.AddRange(BitConverter.GetBytes(TotalLength));
            data.AddRange(BitConverter.GetBytes(CheckSum));
            data.AddRange(BitConverter.GetBytes(UserID));
            data.AddRange(BitConverter.GetBytes(Key));

            return data.ToArray();
        }

        public static MessageHeader ParseByte(byte[] data)
        {
            return new MessageHeader
            {
                Protocol = BitConverter.ToInt32(data, 0),        // 0~3: Protocol
                TotalLength = BitConverter.ToInt32(data, 4),     // 4~7: TotalLength
                CheckSum = BitConverter.ToInt32(data, 8),        // 8~11: CheckSum
                UserID = BitConverter.ToInt32(data, 12),         // 12~15: UserID
                Key = BitConverter.ToInt32(data, 16)             // 16~19: Key
            };
        }
    }

    // MessagePayload 클래스
    public class MessagePayload
    {
        public string Text { get; set; }

        public MessagePayload(string text)
        {
            this.Text = text;
        }

        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(this.Text);
        }

        public static MessagePayload ParseByte(byte[] data, int index, int count)
        {
            return new MessagePayload(Encoding.UTF8.GetString(data, index, count));
        }
    }
}
