using System;
using System.Collections.Generic;
using System.IO;

namespace BinaryTableDB
{
    public class BTableBuilder
    {
        private readonly Stream _stream;
        private readonly BinaryWriter _writer;
        private readonly List<BTableColumn> _columns = new List<BTableColumn>();

        /// <summary>
        /// Instantiates the BTableBuilder by providing a stream to write.
        /// This stream must be empty in order to create a BTable.
        /// </summary>
        /// <param name="stream"></param>
        public BTableBuilder(Stream stream)
        {
            _stream = stream;
            _writer = new BinaryWriter(stream);
        }
        
        /// <summary>
        /// Adds a column to the list of column definitions.
        /// All columns are added sequentially.
        /// </summary>
        /// <param name="column">The column to add.</param>
        public void AddColumn(BTableColumn column)
        {
            _columns.Add(column);
        }

        /// <summary>
        /// Creates the BTable with the specified columns.
        /// </summary>
        public void CreateBTable()
        {
            // TODO: Throw if stream is not empty.

            _writer.Write((char) _columns.Count); // TODO: Throw if overflow.

            foreach(var column in _columns)
            {
                _writer.Write((char) column.Size);
                _writer.Write(column.Name);
            }
        }
    }
}
