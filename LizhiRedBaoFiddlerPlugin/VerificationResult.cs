using System;

namespace LizhiRedBaoFiddlerPlugin
{
    [Flags]
    public enum VerificationResult
    {
        NO_INFORMATION,
        EXPIRED,
        INFORMATION_ERROR,
        NORMAL
    }
}