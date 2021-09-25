namespace ProtocolLibrary
{
    public static class FileTransferProtocol
    {

        public const int FileNameSize = 4;
        public const int FileSize = 8;
        public const int MaxPacketSize = 8192;

        public static long CalculateParts(long size)
        {
            long parts = size / MaxPacketSize;
            return parts * MaxPacketSize == size ? parts : parts + 1;
        }

    }
}
