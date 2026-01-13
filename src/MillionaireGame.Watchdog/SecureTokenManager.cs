using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MillionaireGame.Watchdog;

/// <summary>
/// Manages secure storage of OAuth tokens using Windows Credential Manager
/// Tokens are stored encrypted and tied to the current user account
/// </summary>
public class SecureTokenManager
{
    private const string CredentialTarget = "TheMillionaireGame_GitHub_OAuth";
    private const int ERROR_NOT_FOUND = 1168;
    
    /// <summary>
    /// Stores a GitHub OAuth token securely in Windows Credential Manager
    /// </summary>
    /// <param name="token">The OAuth token to store</param>
    /// <returns>True if stored successfully</returns>
    public static bool StoreToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;
        
        try
        {
            byte[] credentialBlob = Encoding.UTF8.GetBytes(token);
            IntPtr credentialBlobPtr = Marshal.AllocHGlobal(credentialBlob.Length);
            Marshal.Copy(credentialBlob, 0, credentialBlobPtr, credentialBlob.Length);
            
            try
            {
                var credential = new CREDENTIAL
                {
                    Type = CRED_TYPE.GENERIC,
                    TargetName = CredentialTarget,
                    CredentialBlob = credentialBlobPtr,
                    CredentialBlobSize = credentialBlob.Length,
                    Persist = CRED_PERSIST.LOCAL_MACHINE,
                    AttributeCount = 0,
                    UserName = "GitHubOAuthToken"
                };
                
                bool result = CredWrite(ref credential, 0);
                
                if (!result)
                {
                    int error = Marshal.GetLastWin32Error();
                    WatchdogConsole.Error($"[SecureTokenManager] Failed to store token. Error: {error}");
                }
                
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(credentialBlobPtr);
            }
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"[SecureTokenManager] Exception storing token: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Retrieves the stored GitHub OAuth token from Windows Credential Manager
    /// </summary>
    /// <returns>The stored token, or null if not found</returns>
    public static string? RetrieveToken()
    {
        try
        {
            IntPtr credPtr;
            bool result = CredRead(CredentialTarget, CRED_TYPE.GENERIC, 0, out credPtr);
            
            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                if (error != ERROR_NOT_FOUND)
                {
                    WatchdogConsole.Warn($"[SecureTokenManager] Failed to retrieve token. Error: {error}");
                }
                return null;
            }
            
            try
            {
                var credential = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                byte[] credentialBlob = new byte[credential.CredentialBlobSize];
                Marshal.Copy(credential.CredentialBlob, credentialBlob, 0, credential.CredentialBlobSize);
                
                return Encoding.UTF8.GetString(credentialBlob);
            }
            finally
            {
                CredFree(credPtr);
            }
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"[SecureTokenManager] Exception retrieving token: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Deletes the stored GitHub OAuth token from Windows Credential Manager
    /// </summary>
    /// <returns>True if deleted successfully or not found</returns>
    public static bool DeleteToken()
    {
        try
        {
            bool result = CredDelete(CredentialTarget, CRED_TYPE.GENERIC, 0);
            
            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                // Not found is considered success for deletion
                if (error == ERROR_NOT_FOUND)
                    return true;
                
                WatchdogConsole.Error($"[SecureTokenManager] Failed to delete token. Error: {error}");
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"[SecureTokenManager] Exception deleting token: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Checks if a token is currently stored
    /// </summary>
    public static bool HasToken()
    {
        return !string.IsNullOrEmpty(RetrieveToken());
    }
    
    #region Windows Credential Manager P/Invoke
    
    [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr credentialPtr);
    
    [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);
    
    [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredDelete(string target, CRED_TYPE type, int reservedFlag);
    
    [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
    private static extern void CredFree([In] IntPtr cred);
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public uint Flags;
        public CRED_TYPE Type;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string TargetName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public CRED_PERSIST Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string TargetAlias;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string UserName;
    }
    
    private enum CRED_TYPE : uint
    {
        GENERIC = 1,
        DOMAIN_PASSWORD = 2,
        DOMAIN_CERTIFICATE = 3,
        DOMAIN_VISIBLE_PASSWORD = 4,
        GENERIC_CERTIFICATE = 5,
        DOMAIN_EXTENDED = 6,
        MAXIMUM = 7,
        MAXIMUM_EX = 1007
    }
    
    private enum CRED_PERSIST : uint
    {
        SESSION = 1,
        LOCAL_MACHINE = 2,
        ENTERPRISE = 3
    }
    
    #endregion
}
