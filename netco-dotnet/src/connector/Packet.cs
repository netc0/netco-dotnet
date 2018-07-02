using System;
namespace netco {
    public enum PacketType {
        PacketType_NAN = 0,     // 未知
        PacketType_SYN,         // 同步包
        PacketType_ACK,         // 确认连接
        PacketType_HEARTBEAT,   // 心跳包
        PacketType_DATA,        // 请求数据包, 需要包含requestID
        PacketType_PUSH,        // 推送数据
        PacketType_KICK,        // 踢下线
    }
    public class Packet {
        public PacketType Type {get; private set;}
        public byte[] Data {get; private set;}

        public Packet() {
            Type = 0;
            Data = null;
        }

        // 构造一个空包
        public static byte[] ToEmptyPacketBinary(PacketType type) {
            byte[] header = new byte[4];
            header[0] = (byte)type;
            return header;
        }

        // 构造一个特定类型的包
        public static byte[] ToRawPacketBinary(PacketType type, byte[] data) {
            if (data == null) data = new byte[0];
            int len = data.Length;
            byte[] header = new byte[4];
            header[0] = (byte)type;
            header[1] = (byte)(len >> 16);
            header[2] = (byte)(len >> 8);
            header[3] = (byte)(len >> 0);

            byte[] buffer = new byte[header.Length + data.Length];
            // header
            Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
            // body
            Buffer.BlockCopy(data, 0, buffer, header.Length, data.Length);

            return buffer;
        }

        // 构造一个 Data 类型的包
        public static byte[] ToDataPacketBinary(PacketType type, byte[] data) {
            if (data == null) data = new byte[0];
            int len = data.Length; // requestID + route + body
            byte[] header = new byte[4];
            header[0] = (byte)type;
            header[1] = (byte)(len >> 16);
            header[2] = (byte)(len >> 8);
            header[3] = (byte)(len >> 0);

            byte[] buffer = new byte[header.Length + data.Length];
            // header
            Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
            // body
            Buffer.BlockCopy(data, 0, buffer, header.Length, data.Length);

            return buffer;
        }

        public static Packet ToPacket(byte[] data) {
            Packet pkg = new Packet();
            int len = 0;

            pkg.Type = (PacketType)data[0];
            len = (byte)(len << 16) |
                  (byte)(len << 8)  |
                  (byte)(len << 0);
            if (len > 0) {
                pkg.Data = new byte[len];
                Buffer.BlockCopy(pkg.Data, 0, data, 5, len);
            }
            return pkg;
        }
        public static Packet NewPacket(PacketType type, byte[] body) {
            Packet pkg = new Packet();
            pkg.Type = type;
            pkg.Data = body;
            return pkg;
        }
    }
}
