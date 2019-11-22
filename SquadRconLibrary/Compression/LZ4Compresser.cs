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
        /// <param name="input_buffer">The input byte array to compress.</param>
        /// <returns>The compressed version of the input byte array.</returns>
        public static byte[] Compress(byte[] input_buffer)
        {
            var inputBufferLength = input_buffer.Length;
            var inputBufferMaxLength = LZ4Codec.MaximumOutputSize(inputBufferLength);
            var outputBuffer = new byte[inputBufferMaxLength];

            var outputLength = LZ4Codec.Encode(
                input_buffer, 0, input_buffer.Length,
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
        /// <param name="input_buffer">The input byte array to decompress.</param>
        /// <returns>The decompressed version of the input byte array.</returns>
        public static byte[] Decompress(byte[] input_buffer, int expected_length = -1)
        {
            var inputBufferLength = input_buffer.Length;
            var inputBufferMaxLength = (expected_length != -1) ? expected_length : inputBufferLength * 255;
            var outputBuffer = new byte[inputBufferMaxLength];

            var outputLength = LZ4Codec.Decode(
                input_buffer, 0, input_buffer.Length,
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