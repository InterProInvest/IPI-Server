namespace HES.Core.Enums
{
    public enum VaultStatusReason
    {
        None,
        InvalidActivationCode,
        LockedByInvalidPin,
        Withdrawal,
        Lost,
        Stolen,
        Broken
    }
}