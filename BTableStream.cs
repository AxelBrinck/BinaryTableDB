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

        private bool rowWidthInitialized;

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

                rowWidthInitialized = true;

                return;
            }

            _writer.Write("BT");
            _writer.Write((byte) Version);
        }

        /// <summary>
        /// Given a row Id, seeks to its begin position in the stream.
        /// </summary>
        /// <param name="rowId">The row Id to seek.</param>
        /// <returns>The target row stream position.</returns>
        private long GetRowIdStreamPosition(long rowId)
        {
            return HeaderSize + rowId * _rowWidth;
        }
        
        /// <summary>
        /// Indicates a stream the maximum number of bytes every row must have.
        /// </summary>
        /// <param name="width"></param>
        private void InitializeRowWidth(int width)
        {
                long previousStreamPosition = _stream.Position;

                _rowWidth = width;
                _stream.Position = 4;
                _writer.Write(_rowWidth);

                _stream.Position = previousStreamPosition;
                
                rowWidthInitialized = true;
        }

        /// <summary>
        /// Throws an exception if the inbound data is bigger than the maximum row's width.
        /// </summary>
        /// <param name="inboundWidth">The inbound width to check.</param>
        private void CheckOverflow(int inboundWidth)
        {
            if (inboundWidth > _rowWidth) throw
                new InvalidOperationException("Inbound data is bigger than row width.");
        }

        private void Write(long streamPosition, T data)
        {
            var serial = data.Serialize();

            if (!rowWidthInitialized)
            {
                InitializeRowWidth(serial.Length);
            }

            CheckOverflow(serial.Length);

            _stream.Position = streamPosition;

            _writer.Write(serial);

            _stream.Flush();
        }

        /// <summary>
        /// Writes a new row to the stream.
        /// </summary>
        /// <param name="rowId">The row Id to write.</param>
        /// <param name="data">The data to write.</param>
        public void WriteRow(int rowId, T data)
        {
            var streamPosition = GetRowIdStreamPosition(rowId);
            
            Write(streamPosition, data);
        }

        /// <summary>
        /// Appends a row to the end of the stream.
        /// </summary>
        /// <param name="data">The data to append to the stream.</param>
        public void AppendRow(T data)
        {
            var streamPosition = _stream.Length;

            Write(streamPosition, data);
        }

        /// <summary>
        /// Reads a row from the stream.
        /// </summary>
        /// <param name="rowId">The row Id to read from.</param>
        /// <returns>An instantiation from the given row.</returns>
        public T ReadRow(int rowId)
        {
            if (!rowWidthInitialized) throw 
                new InvalidOperationException("The stream is not initialized!");

            GetRowIdStreamPosition(rowId);

            var instance = new T();

            var serial = _reader.ReadBytes(_rowWidth); 

            instance.Deserialize(serial);

            return instance;
        }
    }
}