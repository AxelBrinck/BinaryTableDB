using System;
using System.IO;

namespace BinaryTableDB
{
    /// <summary>
    /// Reads and writes to a BTable stream.
    /// </summary>
    public class BTableStream<T> where T : IBTableRowSerializable, new()
    {
        private static readonly int HeaderSize = 8; // Bytes
        private static readonly int Version = 1;
        private static readonly int EmptySize = 0;

        private readonly Stream _stream;
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;

        private int _rowWidth;

        private bool initialized;

        /// <summary>
        /// Instatiates the BTable by providing a stream.
        /// </summary>
        /// <param name="stream">The provided stream to read/write.</param>
        public BTableStream(Stream stream)
        {
            if (!stream.CanRead || !stream.CanWrite) throw
                new InvalidDataException("Required read and write access.");

            if (!stream.CanSeek) throw
                new InvalidDataException("Unseekable stream!");

            _stream = stream;
            _reader = new BinaryReader(stream);
            _writer = new BinaryWriter(stream);

            if (_stream.Length > EmptySize)
            {
                if (_stream.Length < HeaderSize) throw
                    new InvalidDataException("The file is corrupted.");

                var signature = _reader.ReadString();
                if (signature != "BT") throw
                    new InvalidDataException("Invalid format signature.");

                var version = _reader.ReadByte();
                if (signature != "BT") throw
                    new InvalidDataException("Unrecognized version.");

                _rowWidth = _reader.ReadInt32();
                
                Console.WriteLine($"Header read. Signature: { signature }. Version: { version }. Row width: { _rowWidth }");

                initialized = true;

                return;
            }

            _writer.Write("BT");
            _writer.Write((byte) Version);

            Console.WriteLine("Header written.");
        }

        /// <summary>
        /// Given a row Id, seeks to its begin position in the stream.
        /// </summary>
        /// <param name="rowId">The row Id to seek.</param>
        private void SeekToRowId(long rowId)
        {
            long targetPosition = HeaderSize + rowId * _rowWidth;
            Console.WriteLine($"Requested row: { rowId }. Row width is: { _rowWidth }");
            Console.WriteLine($"Seeking to position: { targetPosition }");

            _stream.Position = targetPosition;
        }

        /// <summary>
        /// Writes a new row to the stream.
        /// </summary>
        /// <param name="rowId">The row Id to write.</param>
        /// <param name="data">The data to write.</param>
        public void WriteRow(int rowId, T data)
        {

            Console.WriteLine("Performing write...");


            var serial = data.Serialize();

            if (!initialized)
            {
                _rowWidth = serial.Length;
                _writer.Write(_rowWidth);
                
                initialized = true;
                Console.WriteLine($"Stream initialized with row width: { _rowWidth }. Serial Length: { serial.Length }. Row width byte position: { _stream.Position - 4 }");
            }

            if (serial.Length > _rowWidth) throw
                new InvalidOperationException("Inbound data is bigger than row width.");

            SeekToRowId(rowId);

            _writer.Write(serial);

            _stream.Flush();
        }

        /// <summary>
        /// Reads a row from the stream.
        /// </summary>
        /// <param name="rowId">The row Id to read from.</param>
        /// <returns>An instantiation from the given row.</returns>
        public T ReadRow(int rowId)
        {
            if (!initialized) throw 
                new InvalidOperationException("The stream is not initialized!");

            SeekToRowId(rowId);
            
            Console.WriteLine("Performing read...");

            var instance = new T();

            var serial = _reader.ReadBytes(_rowWidth); 

            instance.Deserialize(serial);

            return instance;
        }
    }
}