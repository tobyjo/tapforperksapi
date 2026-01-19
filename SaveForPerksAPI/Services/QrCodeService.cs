using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SaveForPerksAPI.DbContexts;

namespace SaveForPerksAPI.Services;

public class QrCodeService : IQrCodeService
{
    private readonly TapForPerksContext _context;

    public QrCodeService(TapForPerksContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public string GenerateQrCodeValue()
    {
        // Generate 12 random bytes (96 bits of entropy)
        byte[] randomBytes = new byte[12];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert to uppercase alphanumeric (Base32-like)
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var result = new StringBuilder(16);

        foreach (byte b in randomBytes)
        {
            result.Append(chars[b % chars.Length]);
        }

        return result.ToString(); // e.g., "K7M2NPQR8VXZ3Y4W"
    }

    public async Task<bool> IsQrCodeUniqueAsync(string qrCodeValue)
    {
        return !await _context.Users
            .AnyAsync(u => u.QrCodeValue == qrCodeValue);
    }
}