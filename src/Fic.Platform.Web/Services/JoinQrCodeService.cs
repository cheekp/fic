using QRCoder;

namespace Fic.Platform.Web.Services;

public sealed class JoinQrCodeService
{
    public string RenderSvg(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var svg = new SvgQRCode(data).GetGraphic(8);
        return svg;
    }
}
