namespace BinaryTableDB
{
    /// <summary>
    /// Custom procedure to encode/decode a BTable row.
    /// </summary>
    public interface ICustomSerializable
    {
        /// <summary>
        /// Converts the current object state into a byte array.
        /// </summary>
        /// <returns>The byte array representing the current object state.</returns>
        byte[] Serialize();

        /// <summary>
        /// Sets the object state to the encoded one from the byte array.
        /// </summary>
        /// <param name="serial">The byte array holding the object state.</param>
        void Deserialize(byte[] serial);
    }
}