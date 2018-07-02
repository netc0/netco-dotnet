using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Timers;

namespace netco {
    public class Protocol {
        Transporter transporter = null;
        UInt32 _requestId = 1;
        Client client;
        public Protocol(Client client, Transporter transporter) {
            this.client = client;
            this.transporter = transporter;
            readType = ReadType.Head;
            startHeartBeat();
        }

        Dictionary<UInt32, Action<byte[]>> RequestMap = new Dictionary<UInt32, Action<byte[]>>();

        UInt32 GetRouteCRC32(string route) {
            return Crc32.GetCRC32Str(route);
        }

        byte[] BuildData(UInt32 requestId, UInt32 routeId, byte[] data) {
            byte[] header = new byte[8];
            header[0] = (byte)(requestId >> 24);
            header[1] = (byte)(requestId >> 16);
            header[2] = (byte)(requestId >> 8);
            header[3] = (byte)(requestId >> 0);

            header[4] = (byte)(routeId >> 24);
            header[5] = (byte)(routeId >> 16);
            header[6] = (byte)(routeId >> 8);
            header[7] = (byte)(routeId >> 0);

            byte[] buffer = new byte[header.Length + data.Length];
            // header
            Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
            // body
            Buffer.BlockCopy(data, 0, buffer, header.Length, data.Length);

            return buffer;
        }

        #region Request 发送请求
        public void Request(string route, byte[] data, Action<byte[]> callback) {
            _requestId++;
            RequestMap.Add(_requestId, callback);
            // 1. 构造应用层数据包
            // appcation level
            // [requestId] [routeId] [data]
            var routeId = GetRouteCRC32(route);
            var appData = BuildData(_requestId, routeId, data);
            // 2. 构造传输层数据包
            // transport level
            var rs = Packet.ToDataPacketBinary(PacketType.PacketType_DATA, appData);
            transporter.Send(rs);
        }

        #endregion

        #region buffer数据处理
        enum ReadType {
            Head,
            Body
        }
        ReadType readType;
        byte[] readBuffer = new byte[4096];
        int readBufferCount = 0;
        PacketType headType = 0;
        int headLength = 0;
        void AppendToReadBuffer(byte[] data) {
            var leftSpace = readBuffer.Length - readBufferCount;
            if (leftSpace > data.Length) {
                Buffer.BlockCopy(data, 0, readBuffer, readBufferCount, data.Length);
                readBufferCount += data.Length;
                return;
            }
            // realloc buffer
            int newLength = readBuffer.Length + data.Length * 2;
            if (newLength < 4096) { newLength = 4096; }

            var tmp = new byte[newLength]; // 预留空间
            // old data
            Buffer.BlockCopy(tmp, 0, readBuffer, 0, readBufferCount);
            readBuffer = tmp;
            // new data
            Buffer.BlockCopy(data, 0, readBuffer, readBufferCount, data.Length);
            readBufferCount += data.Length;
        }
        void TrimBuffer(int frontByte) {
            Buffer.BlockCopy(readBuffer, frontByte, readBuffer, 0, readBufferCount - frontByte);
            readBufferCount -= frontByte;
        }
        #endregion

        #region OnReadBytes 读bytes
        public void OnReadBytes(byte[] data) {
            try {
                Packet pkg = handleBytes(data);
                do {
                    if (pkg == null) break;
                    processPacket(pkg);
                    pkg = handleBytes(null);
                } while (true);
            } catch (Exception e) {
                Debug.Log(e);
            }
        }
        #endregion

        #region handleBytes 尝试解析byte构造packet
        Packet handleBytes(byte[] data) {
            //Console.WriteLine(data.ToStringx());
            if (data != null) {
                AppendToReadBuffer(data);
            }
            if (readBufferCount == 0) return null;
            if (readType == ReadType.Head) {
                if (readBufferCount < 4) return null;
                // parse header
                headType = (PacketType)readBuffer[0];
                headLength =
                    ((int)readBuffer[1]) << 16 |
                    ((int)readBuffer[2]) << 8 |
                    ((int)readBuffer[3]);
                TrimBuffer(4);
                readType = ReadType.Body;
                return handleBytes(null);
            } else if (readType == ReadType.Body) {
                if (headLength == 0) { // 空包
                    var Body = new byte[0];

                    readType = ReadType.Head;
                    headLength = 0;

                    // emit msg
                    Packet pkg = Packet.NewPacket(headType, Body);
                    return pkg;
                } else {
                    if (readBufferCount < headLength || readBufferCount == 0) return null;

                    var Body = new byte[headLength];
                    Buffer.BlockCopy(readBuffer, 0, Body, 0, headLength);
                    TrimBuffer(headLength);

                    readType = ReadType.Head;
                    headLength = 0;

                    // emit msg
                    Packet pkg = Packet.NewPacket(headType, Body);

                    return pkg;
                }
            }
            return null;
        }
        #endregion

        void processPacket(Packet pkg) {
            //Console.WriteLine("processPacket:" + pkg.Type);
            switch (pkg.Type) {
                case PacketType.PacketType_DATA: {
                        // 解析request ID
                        var data = pkg.Data;
                        UInt32 requestID =
                            ((UInt32)data[0]) << 24 |
                            ((UInt32)data[1]) << 16 |
                            ((UInt32)data[2]) << 8 |
                            ((UInt32)data[3]);
                        var Body = new byte[data.Length - 4];
                        Buffer.BlockCopy(data, 4, Body, 0, data.Length - 4);
                        if (RequestMap.ContainsKey(requestID)) {
                            var cb = RequestMap[requestID];
                            RequestMap.Remove(requestID);
                            try { cb(Body); } catch { }
                        }
                    }
                    break;
                case PacketType.PacketType_KICK: {
                        Console.WriteLine("踢下线了");
                    }break;
                case PacketType.PacketType_PUSH : {
                        // 解析routeId
                        var data = pkg.Data;
                        UInt32 routeId =
                            ((UInt32)data[0]) << 24 |
                            ((UInt32)data[1]) << 16 |
                            ((UInt32)data[2]) << 8 |
                            ((UInt32)data[3]);
                        var Body = new byte[data.Length - 4];
                        Buffer.BlockCopy(data, 4, Body, 0, data.Length - 4);
                        client.OnPushMessage(routeId, Body);
                    }
                    break;
            }
        }

        #region 心跳处理
        Timer heartBeatTimer;
        void sendHeartBeat(object source, ElapsedEventArgs e) {
            var rs = Packet.ToRawPacketBinary(PacketType.PacketType_HEARTBEAT, null);
            transporter.Send(rs);
        }
        void startHeartBeat() {
            heartBeatTimer = new Timer();
            heartBeatTimer.Interval = client.HeartBeatInterval * 1000;
            heartBeatTimer.Elapsed += new ElapsedEventHandler(sendHeartBeat);
            heartBeatTimer.Enabled = true;
        }

        void stopHeartBeat() {
            heartBeatTimer.Enabled = false;
            heartBeatTimer.Dispose();
        }
        #endregion

        
    }
}


public static class Utility {
    public static string ToStringx(this byte[] data) {
        if (data == null) return "[]";
        var s = "[";
        foreach(var d in data) {
            s += d + " ";
        }
        return s + "]";
    }

    public static byte[] StringToBytes(this string data) {
        if (data == null) return null;
        return System.Text.Encoding.Default.GetBytes(data);
    }
    public static string BytesToString(this byte[] data) {
        if (data == null) return null;
        return System.Text.Encoding.Default.GetString(data);
    }

    public static byte[] StringToBytesUTF8(this string data) {
        if (data == null) return null;
        return System.Text.Encoding.UTF8.GetBytes(data);
    }
    public static string BytesToStringUTF8(this byte[] data) {
        if (data == null) return null;
        return System.Text.Encoding.UTF8.GetString(data);
    }
}