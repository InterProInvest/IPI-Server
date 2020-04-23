namespace HES.Core.Enums
{
    public enum VaultStatusReason
    {
        None,
        InvalidActivationCode,
        LockedByInvalidPin,
        Lost,
        Stolen,
        Broken,
        Suspended,
        Other
    }
}