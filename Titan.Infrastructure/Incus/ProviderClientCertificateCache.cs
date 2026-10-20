using System.Security.Cryptography.X509Certificates;

namespace Titan.Infrastructure.Incus;

public sealed record CachedClientCertificate(string Fingerprint, X509Certificate2 Certificate);

// Singleton: Titan's client certificate is decrypted and loaded once per process and shared by every Incus call
// instead of being rebuilt (and leaked) per request. Keyed by fingerprint so a replaced identity row is noticed.
public sealed class ProviderClientCertificateCache
{
    private readonly object _lock = new();
    private CachedClientCertificate? _cached;

    public X509Certificate2? For(string fingerprint)
    {
        lock (_lock)
        {
            bool isTheSameIdentity = _cached != null && _cached.Fingerprint == fingerprint;
            if (!isTheSameIdentity)
            {
                return null;
            }
            return _cached!.Certificate;
        }
    }

    /// Keeps the candidate unless an equal one is already cached, in which case that one is returned. A certificate
    /// of a replaced identity is dropped, not disposed: a connection may still be using it.
    public X509Certificate2 Keep(string fingerprint, X509Certificate2 candidate)
    {
        lock (_lock)
        {
            bool alreadyCached = _cached != null && _cached.Fingerprint == fingerprint;
            if (alreadyCached)
            {
                return _cached!.Certificate;
            }
            _cached = new CachedClientCertificate(fingerprint, candidate);
            return candidate;
        }
    }
}
