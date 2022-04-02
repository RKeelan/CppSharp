using CppSharp.AST;
using System;
using System.Collections.Generic;
using System.Text;

namespace CppSharp.Generators.Java
{
    public static class JavaHelpers
    {
        public const string NativeInstanceIdentifier = "nativeInstance";
        public const string NativeInstanceAddrIdentifier = "nativeInstanceAddr";
        public const string OwnsNativeInstanceIdentifier = "ownsNativeInstance";
        public const string CreateInstanceIdentifier ="jniCreateInstance";
        public const string DestroyInstanceIdentifier = "jniDestroyInstance";
        public const string SetNativeInstanceIdentifier = "setNativeInstance";
        public const string GetNativeInstanceIdentifier = "getNativeInstance";
        public const string SetOwnsNativeInstanceIdentifier = "setOwnsNativeInstance";

        public static string GetAccess(AccessSpecifier accessSpecifier)
        {
            switch (accessSpecifier)
            {
                case AccessSpecifier.Private:
                case AccessSpecifier.Internal:
                    return "private ";
                case AccessSpecifier.Protected:
                    return "protected ";
                default:
                    return "public ";
            }
        }
    }
}
