namespace BinaryTableDB
{
    /// <summary>
    /// Provides info related to a column in BTables.
    /// </summary>
    public class BTableColumn
    {
        public string Name { get; }
        public int Size { get; }

        /// <summary>
        /// Instantiates a BTableColumn by providing a name and a size.
        /// The size is the byte-width for all cells of the column.
        /// </summary>
        /// <param name="name">The name to set to the column.</param>
        /// <param name="size">The size for all cells of the column.</param>
        public BTableColumn(string name, int size)
        {
            Name = name;
            Size = size;
        }
    }
}