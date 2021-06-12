using System;

namespace YA.UserWorker.Application.Enums
{
    [Flags]
    public enum MembershipAccessType
    {
        Unknown = 0,
        ReadOnly = 1,
        ReadWrite = 2,
        Admin = 4,
        Owner = 8
    }
}
