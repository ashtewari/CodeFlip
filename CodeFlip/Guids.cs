// Guids.cs
// MUST match guids.h
using System;

namespace AshTewari.CodeFlip
{
    static class GuidList
    {
        public const string guidCodeFlipPkgString = "5cdd74b0-6ee8-48c7-844c-b96b601bf98a";
        public const string guidCodeFlipCmdSetString = "5a021485-4877-4843-a0a1-a545527e4248";

        public static readonly Guid guidCodeFlipCmdSet = new Guid(guidCodeFlipCmdSetString);
    };
}