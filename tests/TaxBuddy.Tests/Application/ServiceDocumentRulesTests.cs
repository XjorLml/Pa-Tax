using System.Text;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Application.Work;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Application;

/// <summary>F03 stage 2 (R23; D26): which files are kept, judged by content, and the names they are shown and downloaded with.</summary>
public sealed class ServiceDocumentRulesTests
{
    [Fact]
    public void A_complete_pdf_and_readable_images_are_kept_by_their_content()
    {
        Assert.Equal(DocumentRules.Pdf, ServiceDocuments.ContentTypeOf(Encoding.ASCII.GetBytes("%PDF-1.7\nbody\n%%EOF\n")));
        Assert.Equal(DocumentRules.Jpeg, ServiceDocuments.ContentTypeOf(TestImages.Jpeg()));
        Assert.Equal(DocumentRules.Png, ServiceDocuments.ContentTypeOf(TestImages.Png(20, 10)));
    }

    [Theory]
    [InlineData("%PDF-1.7 with no end")]
    [InlineData("MZ an executable")]
    [InlineData("<html><script>alert(1)</script></html>")]
    [InlineData("GIF89a")]
    [InlineData("")]
    public void Anything_else_is_refused(string content)
    {
        Assert.Null(ServiceDocuments.ContentTypeOf(Encoding.ASCII.GetBytes(content)));
    }

    [Fact]
    public void Image_signatures_without_an_image_are_refused()
    {
        Assert.Null(ServiceDocuments.ContentTypeOf([0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3]));
        Assert.Null(ServiceDocuments.ContentTypeOf([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0]));
        // A real PNG claiming to be a JPEG by its first bytes is still judged by what it is.
        Assert.Equal(DocumentRules.Png, DocumentRules.TypeBySignature(TestImages.Png(4, 4)));
    }

    [Theory]
    [InlineData("March receipts.pdf", DocumentRules.Pdf, "March receipts.pdf")]
    [InlineData("SCAN.JPG", DocumentRules.Jpeg, "SCAN.JPG")]
    [InlineData("photo.jpeg", DocumentRules.Jpeg, "photo.jpeg")]
    [InlineData("statement", DocumentRules.Pdf, "statement.pdf")]
    [InlineData("invoice.exe", DocumentRules.Pdf, "invoice.exe.pdf")]
    [InlineData("C:\\Users\\Ana\\Desktop\\payslip.png", DocumentRules.Png, "payslip.png")]
    [InlineData("../../etc/passwd", DocumentRules.Pdf, "passwd.pdf")]
    [InlineData("a<b>c:\"d|e?f*.pdf", DocumentRules.Pdf, "abcdef.pdf")]
    [InlineData("tab\there\u202Egnp.pdf", DocumentRules.Pdf, "tabheregnp.pdf")]
    [InlineData("   ...   ", DocumentRules.Png, "document.png")]
    [InlineData(null, DocumentRules.Jpeg, "document.jpg")]
    public void Names_are_made_safe_and_end_in_the_contents_extension(string? uploaded, string type, string expected)
    {
        Assert.Equal(expected, DocumentRules.SafeName(uploaded, type));
    }

    [Fact]
    public void Long_names_are_cut_keeping_the_extension()
    {
        var name = DocumentRules.SafeName(new string('a', 300) + ".pdf", DocumentRules.Pdf);

        Assert.Equal(DocumentRules.DisplayName, name.Length);
        Assert.EndsWith("a.pdf", name);
    }

    [Theory]
    [InlineData(1, "1 KB")]
    [InlineData(1024, "1 KB")]
    [InlineData(250_000, "245 KB")]
    [InlineData(3_565_158, "3.4 MB")]
    [InlineData(DocumentRules.MaxBytes, "10 MB")]
    public void Sizes_read_as_KB_or_MB(long length, string expected)
    {
        Assert.Equal(expected, DocumentRules.SizeText(length));
    }
}
