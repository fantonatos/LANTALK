using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SharedObjects
{
    [Serializable()]
    public class ChatMessage : ISerializable
    {
        public string Text { get; set; }

        public string Destination { get; set; }

        public string Source { get; set; }

        #region Constuctors

        public ChatMessage() { }

        public ChatMessage(string text) => Text = text;

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        protected ChatMessage(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            Text = info.GetString("Text");
            Destination = info.GetString("Destination");
        }

        #endregion

        public byte[] ToByteArray()
        {
            using(var stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                return stream.ToArray();
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Text", Text);
            info.AddValue("Destination", Destination);
        }

        public override string ToString()
        {
            return Text;
        }

        public static ChatMessage FromByteArray(byte[] serializedObject)
        {
            using (var stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                stream.Write(serializedObject, 0, serializedObject.Length);
                stream.Seek(0, SeekOrigin.Begin);

                return (ChatMessage)formatter.Deserialize(stream);
            }
        }
    }
}
