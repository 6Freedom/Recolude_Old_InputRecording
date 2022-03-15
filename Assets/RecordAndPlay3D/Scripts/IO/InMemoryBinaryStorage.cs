using System.IO;
using System.Collections.Generic;

namespace RecordAndPlay.IO
{

    public class InMemoryBinaryStorage : IBinaryStorage
    {

        private static byte[] ReadAll(Stream sourceStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                sourceStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static Stream ToStream(byte[] data)
        {
            return new MemoryStream(data);
        }

        Dictionary<string, byte[]> storage;

        public InMemoryBinaryStorage()
        {
            storage = new Dictionary<string, byte[]>();
        }

        public void Delete(string location)
        {
            storage.Remove(location);
        }

        public bool Exists(string location)
        {
            return storage.ContainsKey(location);
        }

        public Stream Read(string location)
        {
            return ToStream(storage[location]);
        }

        public void Write(Stream data, string location)
        {
            storage[location] = ReadAll(data);
        }

    }

}