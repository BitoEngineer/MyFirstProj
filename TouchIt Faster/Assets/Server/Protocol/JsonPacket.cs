﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Server.Protocol
{
    public enum ReplyStatus : byte { None = 0, Request, OK, Created, NotAuthorized, NoContent, NotFound, BadContent, Forbidden, Conflict, ContentTypeNotFound, InternalServerError };

    public class JsonPacket
    {
        public const byte INITIALIZER = 0x01;
        public const byte FINALIZER = 0x04;

        public string ClientID { get; set; }
        public int PacketID { get; private set; }
        public ReplyStatus ReplyStatus { get; set; }
        public string ContentType { get; set; }
        public string ContentJson { get; set; }


        public JsonPacket(string clientId, string contentType, string contentJson)
        {
            ClientID = clientId;
            ContentType = contentType;
            ContentJson = contentJson;
            PacketID = (int)(DateTime.UtcNow - DateTime.UtcNow.Date).TotalMilliseconds;
        }

        protected JsonPacket(string clientId, string contentType, string contentJson, ReplyStatus contentResult, int packetId)
        {
            ClientID = clientId;
            ContentType = contentType;
            ContentJson = contentJson;
            PacketID = packetId;
            ReplyStatus = contentResult;
        }

        public T DeserializeContent<T>()
        {
            //return JsonMapper.ToObject<T>(ContentJson);
            //return JsonUtility.FromJson<T>(ContentJson);
            return JsonConvert.DeserializeObject<T>(ContentJson);
        }

        public byte[] GetPacket()
        {
            BinaryContentCreator payload = new BinaryContentCreator();

            payload.AddString(ClientID ?? "");
            payload.AddInt(PacketID, 4);
            payload.AddInt((int)ReplyStatus, 1);
            payload.AddString(ContentType ?? "");
            payload.AddString(ContentJson);

            BinaryContentCreator packet = new BinaryContentCreator();
            byte[] payloadArray = payload.GetContent();

            packet.AddInt(INITIALIZER, 1); // 1 byte
            packet.AddInt(payloadArray.Length); // 4 bytes
            packet.AddInt(0, 1); // 1 byte reserved to use in the future to indicate if payload is zipped
            packet.AddRawBytes(payloadArray); // x bytes
            packet.AddBytesCRC(payloadArray); // 1 byte
            packet.AddInt(FINALIZER, 1); // 1 byte

            return packet.GetContent();
        }

        public static bool DeserializePacket(List<byte> buffer, out JsonPacket p)
        {
            int initializerIdx = 0;
            p = null;

            if (buffer.Count <= 6)
                return false;

            // Search for initializer

            while ((initializerIdx < buffer.Count) && (buffer[initializerIdx] != INITIALIZER))
                initializerIdx++;

            if (initializerIdx > 0)
            {
                //Debug.LogFormat("{0} bytes of trush removed", initializerIdx);
                buffer.RemoveRange(0, initializerIdx);
                initializerIdx = 0;
            }

            if (initializerIdx >= buffer.Count) // Only trush in buffer
                return false;

            // Check size

            BinaryContentReader reader = new BinaryContentReader(buffer.ToArray());

            reader.ReadInt(1); // INITIALIZER
            int payloadsize = reader.ReadInt(4);
            reader.ReadInt(1); // 1 byte reserved to use in the future to indicate if payload is zipped

            if ((payloadsize + 7) >= buffer.Count) // Not enought, wait for more data
                return false;

            // Check CRC and finalizer

            byte[] payload = reader.ReadRawBytes(payloadsize);
            reader.ReadAndVerifyBytesCRC(payload);

            if (reader.ReadInt(1) != FINALIZER)
            {
                //Debug.LogErrorFormat("UNEXPECTED ERROR! {0} bytes removed, finalizer wasn't in the correct place", reader.CurrentIndex);
                buffer.RemoveRange(0, reader.CurrentIndex);
                return false;
            }

            buffer.RemoveRange(0, reader.CurrentIndex);

            // Decode payload

            reader = new BinaryContentReader(payload);

            string clientId = reader.ReadString();
            int packetId = reader.ReadInt(4);
            ReplyStatus result = (ReplyStatus)reader.ReadInt(1);
            string contentType = reader.ReadString();
            string json = reader.ReadString();

            p = new JsonPacket(clientId, contentType, json, result, packetId);

            return true;
        }
    }
}