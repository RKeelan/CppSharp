using CppSharp.AST;
using System;
using System.Collections.Generic;
using System.Text;

namespace CppSharp.Generators.Java
{
    public static class JavaHelpers
    {
        public static readonly string NativeInstanceAddrIdentifier = "nativeInstanceAddr";
        public static readonly string OwnsNativeInstanceIdentifier = "ownsNativeInstance";
        public static readonly string CreateInstanceIdentifier ="jniCreateInstance";
        public static readonly string DestroyInstanceIdentifier = "jniDestroyInstance";

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
