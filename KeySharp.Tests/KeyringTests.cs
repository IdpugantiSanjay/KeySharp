using Xunit;

namespace KeySharp.Tests;

public class KeyringTests : IDisposable
{
    private const string Package = "com.keysharp.tests";
    private const string Service = "TestService";
    private const string Username = "testuser";

    public void Dispose()
    {
        // Remove only the specific test credential created by these tests
        try { Keyring.DeletePassword(Package, Service, Username); }
        catch { /* ignore if already deleted or never set */ }
    }

    [Fact]
    public void SetPassword_ThenGetPassword_ReturnsStoredPassword()
    {
        Keyring.SetPassword(Package, Service, Username, "secret123");

        var result = Keyring.GetPassword(Package, Service, Username);

        Assert.Equal("secret123", result);
    }

    [Fact]
    public void SetPassword_Overwrite_ReturnsNewPassword()
    {
        Keyring.SetPassword(Package, Service, Username, "first");
        Keyring.SetPassword(Package, Service, Username, "second");

        var result = Keyring.GetPassword(Package, Service, Username);

        Assert.Equal("second", result);
    }

    [Fact]
    public void DeletePassword_ThenGetPassword_ThrowsNotFound()
    {
        Keyring.SetPassword(Package, Service, Username, "to-delete");
        Keyring.DeletePassword(Package, Service, Username);

        var ex = Assert.Throws<KeyringException>(() =>
            Keyring.GetPassword(Package, Service, Username));

        Assert.Equal(ErrorType.NotFound, ex.Type);
    }

    [Fact]
    public void GetPassword_WhenNotSet_ThrowsKeyringException()
    {
        var ex = Assert.Throws<KeyringException>(() =>
            Keyring.GetPassword(Package, Service, "no-such-user"));

        Assert.Equal(ErrorType.NotFound, ex.Type);
        Assert.NotNull(ex.BackendMessage);
    }

    [Fact]
    public void DeletePassword_WhenNotSet_ThrowsKeyringException()
    {
        Assert.Throws<KeyringException>(() =>
            Keyring.DeletePassword(Package, Service, "no-such-user"));
    }

    [Fact]
    public void ListPasswords_ReturnsEntriesForPackage()
    {
        Keyring.SetPassword(Package, Service, Username, "pw");

        var entries = Keyring.ListPasswords(Package);

        Assert.Contains(entries, e => e.Service == Service && e.User == Username);
    }

    [Fact]
    public void ListPasswords_AllEntries_HaveNonEmptyServiceAndUser()
    {
        Keyring.SetPassword(Package, Service, Username, "pw");

        var entries = Keyring.ListPasswords("org.freedesktop.Secret.Generic");

        Assert.All(entries, e =>
        {
            Assert.False(string.IsNullOrEmpty(e.Service), "Entry Service must not be empty");
            Assert.False(string.IsNullOrEmpty(e.User), "Entry User must not be empty");
        });
    }

    [Fact]
    public void ListPasswords_AfterDelete_DoesNotContainEntry()
    {
        Keyring.SetPassword(Package, Service, Username, "pw");
        Keyring.DeletePassword(Package, Service, Username);

        var entries = Keyring.ListPasswords(Package);

        Assert.DoesNotContain(entries, e => e.Service == Service && e.User == Username);
    }

    [Fact]
    public void SetPassword_WithUnicodePassword_RoundTrips()
    {
        const string unicode = "p@$$wørd™£€";
        Keyring.SetPassword(Package, Service, Username, unicode);

        var result = Keyring.GetPassword(Package, Service, Username);

        Assert.Equal(unicode, result);
    }

    [Fact]
    public void SetPassword_WithEmptyPassword_RoundTrips()
    {
        Keyring.SetPassword(Package, Service, Username, "");

        var result = Keyring.GetPassword(Package, Service, Username);

        Assert.Equal("", result);
    }
}
