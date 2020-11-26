using System.Collections.Generic;
using System.IO;

namespace BinaryTableDB
{
    /// <summary>
    /// Manages and converts data from an already created BTable.
    /// </summary>
    public abstract class BTable<T>
    {
        private readonly Stream _stream;
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;

        private readonly long _headerSize;
        private readonly int _rowLength;

        private readonly List<BTableColumn> _columns = new List<BTableColumn>();

        /// <summary>
        /// Instantiates a BTable by providing a stream.
        /// </summary>
        /// <param name="stream">The BTable stream.</param>
        public BTable(Stream stream)
        {
            _stream = stream;
            _reader = new BinaryReader(stream);
            _writer = new BinaryWriter(stream);

            _stream.Position = 0;
            
            // We firstly read the number of columns.
            // It is a number encoded in the first byte.
            int columnCount = _reader.ReadByte();
            int rowLength = 0;

            // Now we get to read the column names.
            for (var i = 0; i < columnCount; i++)
            {
                var columnName = _reader.ReadString();
                var columnSize = _reader.ReadByte();

                rowLength += columnSize;

                _columns.Add(new BTableColumn(columnName, columnSize));
            }

            _rowLength = rowLength;
            _headerSize = _stream.Position;
        }

        /// <summary>
        /// Gets the total number of rows from the stream.
        /// </summary>
        /// <returns>The total number of rows in the stream.</returns>
        public long GetTotalRows()
        {
            return (_stream.Length -_headerSize) / _rowLength;
        }

        /// <summary>
        /// Seeks to the specified row.
        /// </summary>
        /// <param name="row">The row to seek to.</param>
        private void SeekRow(long row)
        {
            long rowBegin = _headerSize + row * _rowLength;

            _stream.Position = rowBegin;
        }

        /// <summary>
        /// Reads a row from the stream.
        /// </summary>
        /// <param name="row"></param>
        /// <returns>A T instance read from the stream.</returns>
        public T ReadRow(long row)
        {
            SeekRow(row);
            T data = ReadRowProcedure(_reader);
            return data;
        }

        /// <summary>
        /// Writes data to an specified row.
        /// </summary>
        /// <param name="row">The target row to write to.</param>
        /// <param name="data">The source data to be written.</param>
        public void WriteRow(long row, T data)
        {
            SeekRow(row);
            WriteRowProcedure(_writer, data);
            _writer.Flush();
        }

        /// <summary>
        /// Appends a new row to the stream.
        /// </summary>
        /// <param name="data">The source data.</param>
        public void Append(T data)
        {
            _stream.Position = _stream.Length;
            WriteRowProcedure(_writer, data);
            _writer.Flush();
        }

        /// <summary>
        /// Decodes an instance of type T from the stream.
        /// </summary>
        /// <returns>A T instance read from the stream.</returns>
        protected abstract T ReadRowProcedure(BinaryReader reader);

        /// <summary>
        /// Encondes an instance of type T into the stream.
        /// </summary>
        /// <param name="data">An instance of type T used as source.</param>
        protected abstract void WriteRowProcedure(BinaryWriter writer, T data);
        
    }
}