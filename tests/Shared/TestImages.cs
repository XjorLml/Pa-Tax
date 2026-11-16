using System.Buffers.Binary;
using System.Text;
using SkiaSharp;

namespace TaxBuddy.TestSupport;

/// <summary>Synthetic images for the profile-picture tests (F06). Nothing here is a real photo.</summary>
public static class TestImages
{
    /// <summary>A marker hidden in a photo's metadata; a processed picture must never contain it.</summary>
    public const string SecretMarker = "SECRET-LOCATION-14.5995N-120.9842E";

    /// <summary>A two-colour image: the left half red, the right half blue.</summary>
    public static byte[] Png(int width = 600, int height = 300) => Encode(width, height, SKEncodedImageFormat.Png);

    public static byte[] Jpeg(int width = 200, int height = 100) => Encode(width, height, SKEncodedImageFormat.Jpeg);

    /// <summary>
    /// A JPEG carrying an EXIF block with orientation 6 (turn 90° clockwise to display) and a comment segment holding
    /// <see cref="SecretMarker"/>, as a phone photo carries location data.
    /// </summary>
    public static byte[] JpegWithExifOrientation6(int width = 200, int height = 100)
    {
        var jpeg = Jpeg(width, height);

        // TIFF header (big-endian) with one IFD entry: Orientation (0x0112), SHORT, count 1, value 6.
        var tiff = new byte[] { (byte)'M', (byte)'M', 0, 42, 0, 0, 0, 8, 0, 1, 0x01, 0x12, 0, 3, 0, 0, 0, 1, 0, 6, 0, 0, 0, 0, 0, 0 };
        var exif = Encoding.ASCII.GetBytes("Exif\0\0").Concat(tiff).ToArray();
        var app1 = Segment(0xE1, exif);
        var comment = Segment(0xFE, Encoding.ASCII.GetBytes(SecretMarker));

        // Insert straight after the start-of-image marker (FF D8).
        return jpeg[..2].Concat(app1).Concat(comment).Concat(jpeg[2..]).ToArray();
    }

    /// <summary>A PNG whose header claims a size (10,000 × 5,000 = 50 MP in the test) with almost no data: a decompression bomb's shape.</summary>
    public static byte[] PngClaiming(int width, int height)
    {
        var ihdr = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(0), width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4), height);
        ihdr[8] = 8; // bit depth
        ihdr[9] = 2; // colour type RGB
        return new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A }
            .Concat(Chunk("IHDR", ihdr))
            .Concat(Chunk("IDAT", [0x78, 0x9C, 0x03, 0x00, 0x00, 0x00, 0x00, 0x01])) // a tiny zlib stream, so a decoder opens it
            .Concat(Chunk("IEND", [])).ToArray();
    }

    public static byte[] NotAnImage() => Encoding.UTF8.GetBytes("<html><body>not a picture</body></html>");

    public static SKColor PixelAt(byte[] encoded, int x, int y)
    {
        using var bitmap = SKBitmap.Decode(encoded);
        return bitmap.GetPixel(x, y);
    }

    public static (int Width, int Height) SizeOf(byte[] encoded)
    {
        using var codec = SKCodec.Create(SKData.CreateCopy(encoded));
        return (codec.Info.Width, codec.Info.Height);
    }

    public static bool IsRed(SKColor c) => c.Red > 180 && c.Blue < 90 && c.Green < 90;

    public static bool IsBlue(SKColor c) => c.Blue > 180 && c.Red < 90 && c.Green < 90;

    private static byte[] Encode(int width, int height, SKEncodedImageFormat format)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        using var red = new SKPaint { Color = SKColors.Red };
        using var blue = new SKPaint { Color = SKColors.Blue };
        canvas.DrawRect(0, 0, width / 2f, height, red);
        canvas.DrawRect(width / 2f, 0, width / 2f, height, blue);
        using var image = surface.Snapshot();
        using var data = image.Encode(format, 95);
        return data.ToArray();
    }

    private static byte[] Segment(byte marker, byte[] payload)
    {
        var length = (ushort)(payload.Length + 2);
        return new byte[] { 0xFF, marker, (byte)(length >> 8), (byte)length }.Concat(payload).ToArray();
    }

    private static byte[] Chunk(string type, byte[] data)
    {
        var typeBytes = Encoding.ASCII.GetBytes(type);
        var result = new byte[12 + data.Length];
        BinaryPrimitives.WriteInt32BigEndian(result, data.Length);
        typeBytes.CopyTo(result, 4);
        data.CopyTo(result, 8);
        BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(8 + data.Length), Crc32(typeBytes.Concat(data).ToArray()));
        return result;
    }

    private static uint Crc32(byte[] bytes)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in bytes)
        {
            crc ^= b;
            for (var k = 0; k < 8; k++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }

        return ~crc;
    }
}
