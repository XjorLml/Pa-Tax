using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Application.Pictures;

namespace TaxBuddy.Tests.Application;

/// <summary>F06 PP02 and PP03: what the processor accepts and what it stores.</summary>
public sealed class PictureProcessorTests
{
    [Fact]
    public void A_wide_png_becomes_a_400_pixel_square_jpeg_cropped_to_the_centre()
    {
        var result = Assert.IsType<PictureProcessResult.Ok>(PictureProcessor.Process(TestImages.Png(600, 300)));

        Assert.Equal((400, 400), TestImages.SizeOf(result.Jpeg));
        Assert.Equal(new byte[] { 0xFF, 0xD8 }, result.Jpeg[..2]);
        // The centre square of a red|blue image keeps red on the left and blue on the right.
        Assert.True(TestImages.IsRed(TestImages.PixelAt(result.Jpeg, 50, 200)));
        Assert.True(TestImages.IsBlue(TestImages.PixelAt(result.Jpeg, 350, 200)));
    }

    [Fact]
    public void A_phone_photo_is_turned_upright_and_loses_all_its_metadata()
    {
        var upload = TestImages.JpegWithExifOrientation6();
        Assert.Contains(TestImages.SecretMarker, Encoding.ASCII.GetString(upload)); // the fixture really carries it

        var result = Assert.IsType<PictureProcessResult.Ok>(PictureProcessor.Process(upload));
        var stored = Encoding.ASCII.GetString(result.Jpeg);

        Assert.DoesNotContain(TestImages.SecretMarker, stored);
        Assert.DoesNotContain("Exif", stored);
        // Orientation 6 turns the red|blue image clockwise: red ends up on top, blue at the bottom.
        Assert.True(TestImages.IsRed(TestImages.PixelAt(result.Jpeg, 200, 50)));
        Assert.True(TestImages.IsBlue(TestImages.PixelAt(result.Jpeg, 200, 350)));
    }

    [Fact]
    public void Empty_oversized_unreadable_and_huge_images_are_refused()
    {
        Assert.Equal(PictureProblem.NoFile, Problem(Array.Empty<byte>()));
        Assert.Equal(PictureProblem.TooLarge, Problem(new byte[PictureLimits.MaxUploadBytes + 1]));
        Assert.Equal(PictureProblem.NotAnImage, Problem(TestImages.NotAnImage()));
        Assert.Equal(PictureProblem.TooManyPixels, Problem(TestImages.PngClaiming(10_000, 5_000)));
    }

    private static PictureProblem Problem(byte[] upload) =>
        Assert.IsType<PictureProcessResult.Rejected>(PictureProcessor.Process(upload)).Problem;
}

/// <summary>F06 PP01, PP04, PP05, PP07: storing, replacing, removing and reading pictures, with fakes.</summary>
public sealed class ProfilePictureServiceTests
{
    private static readonly Guid Accountant = Guid.Parse("0193a6c0-0000-7000-8000-0000000f0601");
    private static readonly Guid Client = Guid.Parse("0193a6c0-0000-7000-8000-0000000f0602");

    [Fact]
    public async Task Uploading_stores_a_new_object_points_the_account_at_it_and_deletes_the_old_one()
    {
        var (service, storage, accounts) = Create();

        Assert.Equal(PictureUploadOutcome.Saved, (await service.UploadAsync(Accountant, TestImages.Png())).Outcome);
        var first = accounts.Pictures[Accountant];
        Assert.Equal(PictureUploadOutcome.Saved, (await service.UploadAsync(Accountant, TestImages.Png())).Outcome);
        var second = accounts.Pictures[Accountant];

        Assert.NotEqual(first, second);
        Assert.StartsWith($"users/{Accountant:N}/", second);
        Assert.Equal(new[] { second }, storage.Objects.Keys);
        Assert.Equal($"/pictures/{Accountant:D}?v={Path.GetFileNameWithoutExtension(second)}", ProfilePictureService.AddressFor(Accountant, second));
    }

    [Fact]
    public async Task A_storage_failure_keeps_the_current_picture()
    {
        var (service, storage, accounts) = Create();
        await service.UploadAsync(Accountant, TestImages.Png());
        var current = accounts.Pictures[Accountant];

        storage.FailWrites = true;
        var result = await service.UploadAsync(Accountant, TestImages.Png());

        Assert.Equal(PictureUploadOutcome.Failed, result.Outcome);
        Assert.Equal(current, accounts.Pictures[Accountant]);
        Assert.True(storage.Objects.ContainsKey(current!));
    }

    [Fact]
    public async Task A_rejected_upload_stores_nothing()
    {
        var (service, storage, accounts) = Create();

        var result = await service.UploadAsync(Accountant, TestImages.NotAnImage());

        Assert.Equal((PictureUploadOutcome.Rejected, PictureProblem.NotAnImage), (result.Outcome, result.Problem));
        Assert.Empty(storage.Objects);
        Assert.Null(accounts.Pictures[Accountant]);
    }

    [Fact]
    public async Task Removing_clears_the_account_and_deletes_the_object()
    {
        var (service, storage, accounts) = Create();
        await service.UploadAsync(Client, TestImages.Png());

        Assert.True(await service.RemoveAsync(Client));

        Assert.Null(accounts.Pictures[Client]);
        Assert.Empty(storage.Objects);
    }

    [Fact]
    public async Task An_accountants_picture_is_public_and_a_clients_is_for_that_client_only()
    {
        var (service, _, _) = Create();
        await service.UploadAsync(Accountant, TestImages.Png());
        await service.UploadAsync(Client, TestImages.Png());

        Assert.True((await service.ReadAsync(Accountant, viewerId: null))!.IsPublic);
        Assert.NotNull(await service.ReadAsync(Accountant, viewerId: Client));
        Assert.Null(await service.ReadAsync(Client, viewerId: null));
        Assert.Null(await service.ReadAsync(Client, viewerId: Accountant));
        Assert.False((await service.ReadAsync(Client, viewerId: Client))!.IsPublic);
        Assert.Null(await service.ReadAsync(Guid.NewGuid(), viewerId: null));
    }

    [Fact]
    public async Task Without_storage_uploads_are_switched_off_and_reads_find_nothing()
    {
        var accounts = new FakeAccounts();
        var service = new ProfilePictureService(new NoStorage(), accounts, NullLogger<ProfilePictureService>.Instance);

        Assert.False(service.IsAvailable);
        Assert.Equal(PictureUploadOutcome.Unavailable, (await service.UploadAsync(Accountant, TestImages.Png())).Outcome);
        Assert.Null(await service.ReadAsync(Accountant, null));
    }

    private static (ProfilePictureService Service, FakeStorage Storage, FakeAccounts Accounts) Create()
    {
        var storage = new FakeStorage();
        var accounts = new FakeAccounts();
        return (new ProfilePictureService(storage, accounts, NullLogger<ProfilePictureService>.Instance), storage, accounts);
    }

    private sealed class FakeStorage : IPictureStorage
    {
        public Dictionary<string, byte[]> Objects { get; } = [];

        public bool FailWrites { get; set; }

        public bool IsAvailable => true;

        public Task PutAsync(string objectName, byte[] jpeg, CancellationToken cancellationToken)
        {
            if (FailWrites)
            {
                throw new PictureStorageException("down");
            }

            Objects[objectName] = jpeg;
            return Task.CompletedTask;
        }

        public Task<byte[]?> GetAsync(string objectName, CancellationToken cancellationToken) =>
            Task.FromResult(Objects.TryGetValue(objectName, out var b) ? b : null);

        public Task DeleteAsync(string objectName, CancellationToken cancellationToken)
        {
            Objects.Remove(objectName);
            return Task.CompletedTask;
        }
    }

    private sealed class NoStorage : IPictureStorage
    {
        public bool IsAvailable => false;

        public Task PutAsync(string objectName, byte[] jpeg, CancellationToken cancellationToken) => throw new PictureStorageException("off");

        public Task<byte[]?> GetAsync(string objectName, CancellationToken cancellationToken) => Task.FromResult<byte[]?>(null);

        public Task DeleteAsync(string objectName, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeAccounts : IAccountPictureStore
    {
        public Dictionary<Guid, string?> Pictures { get; } = new() { [Accountant] = null, [Client] = null };

        public Task<AccountPicture?> FindAsync(Guid accountId, CancellationToken cancellationToken) =>
            Task.FromResult(Pictures.TryGetValue(accountId, out var p) ? new AccountPicture(accountId == Accountant, p) : null);

        public Task<string?> SetAsync(Guid accountId, string? objectName, CancellationToken cancellationToken)
        {
            if (!Pictures.ContainsKey(accountId))
            {
                throw new DataUnavailableException(new InvalidOperationException("no account"));
            }

            var previous = Pictures[accountId];
            Pictures[accountId] = objectName;
            return Task.FromResult(previous);
        }
    }
}
