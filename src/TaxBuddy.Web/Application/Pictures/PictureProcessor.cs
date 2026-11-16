using SkiaSharp;

namespace TaxBuddy.Web.Application.Pictures;

/// <summary>Limits for profile pictures (F06; ADR 011).</summary>
public static class PictureLimits
{
    public const long MaxUploadBytes = 5 * 1024 * 1024;

    /// <summary>Decoded size cap, checked before decoding, so a small file can't expand into a huge bitmap.</summary>
    public const long MaxPixels = 40_000_000;

    public const int OutputSize = 400;

    public const int JpegQuality = 85;
}

public enum PictureProblem
{
    NoFile,
    TooLarge,
    NotAnImage,
    TooManyPixels,
}

public abstract record PictureProcessResult
{
    public sealed record Ok(byte[] Jpeg) : PictureProcessResult;

    public sealed record Rejected(PictureProblem Problem) : PictureProcessResult;
}

/// <summary>
/// Turns an uploaded photo into the stored picture (F06 PP02, PP03): a real JPEG, PNG or WebP within the limits,
/// turned upright by its orientation tag, cropped to a centred square, resized to 400 × 400 and saved as a new JPEG.
/// Re-encoding leaves every piece of metadata behind, including location (R11).
/// </summary>
public static class PictureProcessor
{
    private static readonly SKEncodedImageFormat[] Accepted =
        [SKEncodedImageFormat.Jpeg, SKEncodedImageFormat.Png, SKEncodedImageFormat.Webp];

    public static PictureProcessResult Process(byte[] upload)
    {
        if (upload.Length == 0)
        {
            return new PictureProcessResult.Rejected(PictureProblem.NoFile);
        }

        if (upload.Length > PictureLimits.MaxUploadBytes)
        {
            return new PictureProcessResult.Rejected(PictureProblem.TooLarge);
        }

        using var data = SKData.CreateCopy(upload);
        using var codec = SKCodec.Create(data);
        if (codec is null || !Accepted.Contains(codec.EncodedFormat))
        {
            return new PictureProcessResult.Rejected(PictureProblem.NotAnImage);
        }

        var info = codec.Info;
        if ((long)info.Width * info.Height > PictureLimits.MaxPixels)
        {
            return new PictureProcessResult.Rejected(PictureProblem.TooManyPixels);
        }

        using var decoded = new SKBitmap(new SKImageInfo(info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        var status = codec.GetPixels(decoded.Info, decoded.GetPixels());
        if (status is not (SKCodecResult.Success or SKCodecResult.IncompleteInput))
        {
            return new PictureProcessResult.Rejected(PictureProblem.NotAnImage);
        }

        using var upright = Upright(decoded, codec.EncodedOrigin);
        var side = Math.Min(upright.Width, upright.Height);
        var source = SKRect.Create((upright.Width - side) / 2f, (upright.Height - side) / 2f, side, side);

        using var surface = SKSurface.Create(new SKImageInfo(PictureLimits.OutputSize, PictureLimits.OutputSize, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White); // JPEG has no transparency: transparent areas of a PNG become white.
        using (var image = SKImage.FromBitmap(upright))
        {
            canvas.DrawImage(image, source, SKRect.Create(PictureLimits.OutputSize, PictureLimits.OutputSize),
                new SKSamplingOptions(SKCubicResampler.Mitchell));
        }

        using var snapshot = surface.Snapshot();
        using var jpeg = snapshot.Encode(SKEncodedImageFormat.Jpeg, PictureLimits.JpegQuality);
        return new PictureProcessResult.Ok(jpeg.ToArray());
    }

    /// <summary>Applies the EXIF orientation, so a phone photo isn't saved sideways once its metadata is gone.</summary>
    private static SKBitmap Upright(SKBitmap source, SKEncodedOrigin origin)
    {
        float w = source.Width, h = source.Height;
        var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        // x' = ScaleX·x + SkewX·y + TransX; y' = SkewY·x + ScaleY·y + TransY.
        var matrix = origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1),
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),
            _ => SKMatrix.Identity,
        };

        var result = new SKBitmap(swap ? source.Height : source.Width, swap ? source.Width : source.Height, source.ColorType, source.AlphaType);
        using var canvas = new SKCanvas(result);
        canvas.SetMatrix(matrix);
        using var image = SKImage.FromBitmap(source);
        canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest)); // a pure rotation or flip: no resampling
        return result;
    }
}
