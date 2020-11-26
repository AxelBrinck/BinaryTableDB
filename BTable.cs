using System.Collections.Generic;
using System.IO;

namespace BinaryTableDB
{
    /// <summary>
    /// Manages an already created BTable.
    /// </summary>
    public class BTable
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
    }
}