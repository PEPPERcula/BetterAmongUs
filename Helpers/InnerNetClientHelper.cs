using Hazel;
using InnerNet;
using System.Globalization;

namespace BetterAmongUs.Helpers;

static class InnerNetClientHelper
{
    /// <summary>
    /// Writes an array of boolean values to a MessageWriter in a packed format to save space.
    /// Each byte stores up to 8 boolean values as bits.
    /// </summary>
    internal static void WriteBooleans(this MessageWriter writer, IEnumerable<bool> boolsEnumerable)
    {
        bool[] bools = boolsEnumerable.ToArray();

        writer.Write(bools.Length);

        byte currentByte = 0;
        int bitIndex = 0;

        foreach (bool b in bools)
        {
            if (b) currentByte |= (byte)(1 << bitIndex);

            bitIndex++;

            if (bitIndex == 8)
            {
                writer.Write(currentByte);
                currentByte = 0;
                bitIndex = 0;
            }
        }

        if (bitIndex > 0) writer.Write(currentByte);
    }

    /// <summary>
    /// Reads an array of boolean values from a MessageReader that were previously packed using WriteBooleans.
    /// </summary>
    internal static bool[] ReadBooleans(this MessageReader reader)
    {
        int length = reader.ReadInt32();
        bool[] bools = new bool[length];

        int bitIndex = 0;
        byte currentByte = 0;

        for (int i = 0; i < length; i++)
        {

            if (bitIndex == 0) currentByte = reader.ReadByte();

            bools[i] = (currentByte & 1 << bitIndex) != 0;

            bitIndex++;

            if (bitIndex == 8) bitIndex = 0;
        }

        return bools;
    }

    /// <summary>
    /// Writes an array of bytes to a MessageWriter in a packed format, combining two bytes into one to save space.
    /// </summary>
    internal static void WriteBytes(this MessageWriter writer, IEnumerable<byte> bytesEnumerable)
    {
        byte[] bytes = bytesEnumerable.ToArray();

        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    /// <summary>
    /// Reads an array of bytes from a MessageReader that were previously packed using WritePackedBytes.
    /// </summary>
    internal static byte[] ReadBytes(this MessageReader reader)
    {
        int count = reader.ReadInt32();
        var bytes = reader.ReadBytes(count);
        return [.. bytes];
    }

    /// <summary>
    /// Writes an array of bytes to a MessageWriter in a packed format, combining two bytes into one to save space.
    /// </summary>
    internal static void WritePackedBytes(this MessageWriter writer, IEnumerable<byte> bytesEnumerable)
    {
        byte[] bytes = bytesEnumerable.ToArray();

        writer.Write((byte)bytes.Length);

        for (int i = 0; i < bytes.Length; i += 2)
        {
            byte packedBytes;
            if (i + 1 < bytes.Length) packedBytes = (byte)((bytes[i] & 0x0F) << 4 | bytes[i + 1] & 0x0F);
            else packedBytes = (byte)((bytes[i] & 0x0F) << 4);
            writer.Write(packedBytes);
        }
    }

    /// <summary>
    /// Reads an array of bytes from a MessageReader that were previously packed using WritePackedBytes.
    /// </summary>
    internal static byte[] ReadPackedBytes(this MessageReader reader)
    {
        int count = reader.ReadByte();
        List<byte> bytes = [];

        for (int i = 0; i < (count + 1) / 2; i++)
        {
            byte packedBytes = reader.ReadByte();
            bytes.Add((byte)(packedBytes >> 4 & 0x0F));

            if (bytes.Count < count) bytes.Add((byte)(packedBytes & 0x0F));
        }

        return bytes.ToArray();
    }

    /// <summary>
    /// Writes a floating-point value to a MessageWriter in a packed format to save space.
    /// Handles negative values, integers, and decimal numbers efficiently.
    /// </summary>
    internal static void WritePacked(this MessageWriter writer, float value)
    {
        bool isNegative = value < 0;
        float absValue = Math.Abs(value);

        // Check if it's a solid number (integer)
        bool isSolidNumber = absValue == (uint)absValue && absValue <= uint.MaxValue;

        // Track the number of decimal places dynamically
        int decimalPlaces = 0;
        int scaleFactor = 1;
        bool needsFallback = false; // Flag to determine if we need to send the value as a regular float

        // Check if the value is not a solid number
        if (!isSolidNumber)
        {
            // Convert to string to count decimals (avoiding floating point precision issues)
            string absValueStr = absValue.ToString("G", CultureInfo.InvariantCulture);
            if (absValueStr.Contains('.'))
            {
                decimalPlaces = absValueStr.Split('.')[1].Length; // Count decimals after the period
                scaleFactor = (int)Math.Pow(10, decimalPlaces); // Set scale factor dynamically
            }
        }

        // If the value can't be packed efficiently, we need a fallback
        if (!isSolidNumber || absValue * scaleFactor > uint.MaxValue)
        {
            needsFallback = true; // Flag for fallback as a regular float
        }

        // Pack the flags into a list of booleans
        var flags = new List<bool>
        {
            isNegative,
            isSolidNumber,
            decimalPlaces > 0,
            needsFallback
        };

        // Write the flags using WriteBooleans
        writer.WriteBooleans(flags);

        // Write the decimal places as a byte (0-3 for simplicity)
        writer.Write((byte)decimalPlaces);

        // If it's a solid number, pack as uint
        if (isSolidNumber && !needsFallback)
        {
            writer.Write((uint)absValue);
        }

        // If it's a decimal number, scale and pack it as uint (but check if it can fit)
        else if (decimalPlaces > 0 && !needsFallback)
        {
            // Check if the scaled value fits into a uint
            if (absValue * scaleFactor <= uint.MaxValue)
            {
                uint scaledValue = (uint)(absValue * scaleFactor); // Dynamically scale the value
                writer.Write(scaledValue);
            }
            else
            {
                // If it can't fit into a uint, send as regular float
                writer.Write(value);
            }
        }
        else
        {
            // Send as regular float if it requires fallback
            writer.Write(value);
        }
    }

    /// <summary>
    /// Reads a floating-point value from a MessageReader that was previously packed using WritePacked.
    /// Handles negative values, integers, and decimal numbers efficiently.
    /// </summary>
    internal static float ReadPackedSingle(this MessageReader reader)
    {
        var flags = reader.ReadBooleans();
        bool isNegative = flags[0];
        bool isSolidNumber = flags[1];
        bool hasDecimalPart = flags[2];
        bool needsFallback = flags[3];

        byte decimalPlaces = reader.ReadByte(); // Read the decimal places count

        // If the value was packed as a regular float, just read it (don't reapply the sign)
        if (needsFallback)
        {
            return reader.ReadSingle(); // The sign is already handled in the packed value
        }

        // Handle solid number (uint) case
        if (isSolidNumber)
        {
            uint packedValue = reader.ReadUInt32();
            return isNegative ? -(float)packedValue : packedValue;
        }
        // Handle decimal number (scaled uint)
        else if (hasDecimalPart)
        {
            uint scaledValue = reader.ReadUInt32();
            float scaleFactor = (float)Math.Pow(10, decimalPlaces); // Use the stored decimal places count
            return isNegative ? -(scaledValue / scaleFactor) : scaledValue / scaleFactor;
        }
        else
        {
            // For other cases where it's a normal float
            float value = reader.ReadSingle();
            return isNegative ? -value : value;
        }
    }

    internal static void WritePlayerId(this MessageWriter writer, PlayerControl player) => writer.Write(player?.PlayerId ?? 255);

    internal static PlayerControl? ReadPlayerId(this MessageReader reader) => Utils.PlayerFromPlayerId(reader.ReadByte());

    internal static void WritePlayerDataId(this MessageWriter writer, NetworkedPlayerInfo data) => writer.Write(data?.PlayerId ?? 255);

    internal static NetworkedPlayerInfo? ReadPlayerDataId(this MessageReader reader) => Utils.PlayerDataFromPlayerId(reader.ReadByte());

    internal static void WriteDeadBodyId(this MessageWriter writer, DeadBody body) => writer.Write(body?.ParentId ?? 255);

    internal static DeadBody? ReadDeadBodyId(this MessageReader reader) => Main.AllDeadBodys.FirstOrDefault(deadbody => deadbody.ParentId == reader.ReadByte());

    internal static void WriteVentId(this MessageWriter writer, Vent vent) => writer.Write(vent?.Id ?? -1);

    internal static Vent? ReadVentId(this MessageReader reader) => Main.AllVents.FirstOrDefault(vent => vent.Id == reader.ReadInt32());

    /// <summary>
    /// Converts a MessageWriter to a MessageReader.
    /// </summary>
    internal static MessageReader ToReader(this MessageWriter writer) => MessageReader.Get(writer.ToByteArray(false));

    internal static MessageReader[] ToReaders(this MessageWriter writer)
    {
        var reader = writer.ToReader();
        List<MessageReader> readers = [];

        while (reader.Position < reader.Length)
        {
            readers.Add(reader.ReadMessage());
        }

        return [.. readers];
    }

    internal static MessageReader[] ToReaders(this MessageReader reader)
    {
        List<MessageReader> readers = [];

        while (reader.Position < reader.Length)
        {
            readers.Add(reader.ReadMessage());
        }

        return [.. readers];
    }

    internal static MessageReader[] ToReadersNewBuffer(this MessageReader reader)
    {
        List<MessageReader> readers = [];

        while (reader.Position < reader.Length)
        {
            readers.Add(reader.ReadMessageAsNewBuffer());
        }

        return [.. readers];
    }

    internal static MessageWriter WriteReaders(this MessageWriter writer, IEnumerable<MessageReader> readers, bool clear = false)
    {
        if (clear) writer.Clear(writer.SendOption);

        foreach (MessageReader reader in readers)
        {
            writer.StartMessage(reader.Tag);
            writer.Write(reader.ReadBytes(reader.Length));
            writer.EndMessage();
        }

        return writer;
    }

    internal static MessageWriter Copy(this MessageWriter writer)
    {
        var newWriter = MessageWriter.Get(writer.SendOption);
        newWriter.Write(writer.ToByteArray(false));
        return newWriter;
    }

    /// <summary>
    /// Starts the RPC desynchronization process for the given player, call ID, and send option.
    /// </summary>
    /// <param name="client">The InnerNetClient instance.</param>
    /// <param name="playerNetId">The network ID of the player.</param>
    /// <param name="callId">The RPC call ID.</param>
    /// <param name="option">The send option for the RPC.</param>
    /// <param name="ignoreClientId">The client ID to ignore. Default is -1, which means no client is ignored.</param>
    /// <returns>A list of MessageWriter instances for the RPC calls.</returns>
    /// <example>
    /// <code>
    /// List&lt;MessageWriter&gt; messageWriter = AmongUsClient.Instance.StartRpcDesync(PlayerNetId, (byte)RpcCalls, SendOption, ClientId);
    /// messageWriter.ForEach(mW => mW.Write("RPC TEST"));
    /// AmongUsClient.Instance.FinishRpcDesync(messageWriter);
    /// </code>
    /// </example>
    /*
            List<MessageWriter> messageWriter = AmongUsClient.Instance.StartRpcDesync(PlayerNetId, (byte)RpcCalls, SendOption, ClientId);
            messageWriter.ForEach(mW => mW.Write("RPC TEST"));
            AmongUsClient.Instance.FinishRpcDesync(messageWriter);
    */
    internal static List<MessageWriter> StartRpcDesync(this InnerNetClient client, uint playerNetId, byte callId, SendOption option, int ignoreClientId = -1, Func<ClientData, bool> clientCheck = null)
    {
        List<MessageWriter> messageWriters = [];

        if (ignoreClientId < 0)
        {
            messageWriters.Add(client.StartRpcImmediately(playerNetId, callId, option, -1));
        }
        else
        {
            foreach (var allClients in AmongUsClient.Instance.allClients.ToArray().Where(c => c.Id != ignoreClientId))
            {
                if (clientCheck == null || clientCheck.Invoke(allClients))
                {
                    messageWriters.Add(client.StartRpcImmediately(playerNetId, callId, option, allClients.Id));
                }
            }
        }

        return messageWriters;
    }



    internal static void FinishRpcDesync(this InnerNetClient client, List<MessageWriter> messageWriters)
    {
        foreach (var msg in messageWriters)
        {
            msg.EndMessage();
            msg.EndMessage();
            client.SendOrDisconnect(msg);
            msg.Recycle();
        }
    }
}
