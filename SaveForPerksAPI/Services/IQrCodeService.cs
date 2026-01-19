namespace SaveForPerksAPI.Services;

public interface IQrCodeService
{
    string GenerateQrCodeValue();
    Task<bool> IsQrCodeUniqueAsync(string qrCodeValue);
}