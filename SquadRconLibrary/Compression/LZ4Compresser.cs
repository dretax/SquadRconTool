using K4os.Compression.LZ4;

namespace SquadRconLibrary.Compression
{
    /// <summary>
    /// LZ4Compressor used to compress and decompress data, such as byte arrays, using LZ4 compression algorithms.
    /// </summary>
    public static class LZ4Compresser
    {
        /// <summary>
        /// Compress byte array using LZ4 algorithm (LZ4.LZ4Codec.Encode).
        /// </summary>
        /// <param name="inputBuffer">The input byte array to compress.</param>
        /// <returns>The compressed version of the input byte array.</returns>
        public static byte[] Compress(byte[] inputBuffer)
        {
            var inputBufferLength = inputBuffer.Length;
            var inputBufferMaxLength = LZ4Codec.MaximumOutputSize(inputBufferLength);
            var outputBuffer = new byte[inputBufferMaxLength];

            var outputLength = LZ4Codec.Encode(
                inputBuffer, 0, inputBuffer.Length,
                outputBuffer, 0, outputBuffer.Length
            );

            var compressedBuffer = new byte[outputLength];
            for (int i = 0; i < outputLength; i++)
            {
                compressedBuffer[i] = outputBuffer[i];
            }

            return compressedBuffer;
        }

        /// <summary>
        /// Decompress byte array that was compressed with Compress method. Uses LZ4.LZ4Codec.Decode.
        /// </summary>
        /// <param name="inputBuffer">The input byte array to decompress.</param>
        /// <returns>The decompressed version of the input byte array.</returns>
        public static byte[] Decompress(byte[] inputBuffer, int expectedLength = -1)
        {
            var inputBufferLength = inputBuffer.Length;
            var inputBufferMaxLength = (expectedLength != -1) ? expectedLength : inputBufferLength * 255;
            var outputBuffer = new byte[inputBufferMaxLength];

            var outputLength = LZ4Codec.Decode(
                inputBuffer, 0, inputBuffer.Length,
                outputBuffer, 0, inputBufferMaxLength
            );

            var compressedBuffer = new byte[outputLength];
            for (int i = 0; i < outputLength; i++)
            {
                compressedBuffer[i] = outputBuffer[i];
            }

            return compressedBuffer;
        }
    }
}