using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Server.Protocol
{
    public enum ReplyStatus : byte 
    {
        /// <summary>
        /// <see cref="None"/> is used by the server when no reply should be sent.
        /// Or when the packet sent is not a reply.
        /// </summary>
        None = 0,

        /// <summary>
        /// Returned when the Packet Client ID doesn't exists in Users database.
        /// <para>URI "CreateUser" ignores this validation, so use it to create users.</para>
        /// </summary>
        NotAuthorized,

        /// <summary>
        /// Returned when everything is OK.
        /// </summary>
        OK,

        /// <summary>
        /// Returned when the received URI does not exist.
        /// </summary>
        BadRequest,

        /// <summary>
        /// Returned when the call content is invalid.
        /// </summary>
        BadContent,

        /// <summary>
        /// Returned when content is expected and there's none.
        /// </summary>
        NoContent,

        /// <summary>
        /// <see cref="Forbidden"/> may be used to indicate that the given user is not able to perform such call.
        /// </summary>
        Forbidden,

        /// <summary>
        /// <see cref="Created"/> may be used to indicate that a given call created something successfully.
        /// </summary>
        Created,

        /// <summary>
        /// <see cref="NotFound"/> may be used to indicate that a given call haven't found something.
        /// </summary>
        NotFound,

        /// <summary>
        /// <see cref="Conflict"/> may be used to indicate that the given call data conflicts with something. 
        /// </summary>
        Conflict,

        /// <summary>
        /// Returned whenever an unexpected error occurs in the call execution, for instance an unhandled expection.
        /// </summary>
        InternalServerError
    };

    public class JsonPacket
    {
        public const byte INITIALIZER = 0x01;
        public const byte FINALIZER = 0x04;

        public string ClientID { get; set; }
        public int PacketID { get; private set; }
        public ReplyStatus ReplyStatus { get; set; }
        public string ContentType { get; set; }
        public string ContentJson { get; set; }

        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings();

        static JsonPacket()
        {
            jsonSettings.Converters.Add(new StringEnumConverter() { CamelCaseText = true });
        }

        public JsonPacket(string clientId, string contentType, object contentJson)
        {
            ClientID = clientId;
            ContentType = contentType;
            ContentJson = JsonConvert.SerializeObject(contentJson, jsonSettings);
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
            return JsonConvert.DeserializeObject<T>(ContentJson, jsonSettings);
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
