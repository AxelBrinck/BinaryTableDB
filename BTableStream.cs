using System;
using System.IO;

namespace BinaryTableDB
{
    /// <summary>
    /// Reads and writes to a BTable stream.
    /// </summary>
    public class BTableStream<T> where T : ICustomSerializable, new()
    {
        private readonly Stream _stream;
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;


        private int _rowWidth;

        private bool initialized;

        private static readonly int HeaderSize = 8; // Bytes

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

            if (_stream.Length > 0)
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

                initialized = true;

                return;
            }

            _writer.Write("BT");
            _writer.Write((byte) 1);
        }

        /// <summary>
        /// Given a row Id, seeks to its begin position in the stream.
        /// </summary>
        /// <param name="rowId">The row Id to seek.</param>
        private void SeekToRowId(long rowId)
        {
            _stream.Position = HeaderSize + rowId * _rowWidth;
        }

        /// <summary>
        /// Writes a new row to the stream.
        /// </summary>
        /// <param name="rowId">The row Id to write.</param>
        /// <param name="data">The data to write.</param>
        public void WriteRow(int rowId, T data)
        {
            SeekToRowId(rowId);

            var serial = data.Serialize();

            if (!initialized)
            {
                _rowWidth = serial.Length;
                _writer.Write((byte) _rowWidth);
            }

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

            var instantiation = new T();

            var serial = _reader.ReadBytes(_rowWidth); 

            instantiation.Deserialize(serial);

            return instantiation;
        }
    }
}