using System;
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
            
            // We firstly read the number of columns.
            // It is a number encoded in the first byte.
            int columnCount = _reader.ReadByte();

            // Now we get to read the column names.
            for (var i = 0; i < columnCount; i++)
            {
                // This 8-bit number contains the character length.
                var columnName = _reader.ReadString();
                var columnSize = _reader.ReadByte();

                _columns.Add(new BTableColumn(columnName, columnSize));
            }
        }

        /// <summary>
        /// Converts binary data from the row to the object <see cref="T"/>.
        /// </summary>
        /// <param name="row">The target row to read from.</param>
        /// <returns></returns>
        public abstract T read(long row);

        /// <summary>
        /// The row to convert to.
        /// </summary>
        /// <param name="row">The row to write to.</param>
        /// <param name="data">The data to use as source.</param>
        public abstract void write(long row, T data);
        
    }
}