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
        /// Gets the first row in the stream.
        /// </summary>
        /// <returns>The first row in the stream.</returns>
        public T GetFirstRow()
        {
            var rowCount = GetRowCount();

            if (rowCount == 0)
                throw new InvalidOperationException("The are no rows in the stream!");
            
            return ReadRow(0);
        }

        /// <summary>
        /// Gets the last row in the stream.
        /// </summary>
        /// <returns>The last row in the stream.</returns>
        public T GetLastRow()
        {
            var rowCount = GetRowCount();

            if (rowCount == 0)
                throw new InvalidOperationException("The are no rows in the stream!");

            return ReadRow(rowCount - 1);
        }   

        /// <summary>
        /// Gets the amount of rows stored in the BTable.
        /// </summary>
        /// <returns>Te amount of rows in BTable.</returns>
        public long GetRowCount()
        {
            if (!rowWidthInitialized) return 0;

            var mod = (_stream.Length - HeaderSize) % _rowWidth;

            if (mod != 0) throw
                new InvalidDataException("The stream is corrupted!");

            return (_stream.Length - HeaderSize) / _rowWidth;
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

        /// <summary>
        /// Writes a row to the specified stream position.
        /// Also, initializes the stream with a row width, if the stream has not been init yet.
        /// </summary>
        /// <param name="streamPosition">The stream position to seek.</param>
        /// <param name="data">The data to write.</param>
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
        public void WriteRow(long rowId, T data)
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
            var streamPosition = !rowWidthInitialized ? 8 : _stream.Length;

            Write(streamPosition, data);
        }

        /// <summary>
        /// Reads a row from the stream.
        /// </summary>
        /// <param name="rowId">The row Id to read from.</param>
        /// <returns>An instantiation from the given row.</returns>
        public T ReadRow(long rowId)
        {
            if (!rowWidthInitialized) throw 
                new InvalidOperationException("The stream is not initialized!");

            _stream.Position = GetRowIdStreamPosition(rowId);

            var instance = new T();

            var serial = _reader.ReadBytes(_rowWidth); 

            instance.Deserialize(serial);

            return instance;
        }
    }
}