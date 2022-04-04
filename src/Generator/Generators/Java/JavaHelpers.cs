using CppSharp.AST;
using CppSharp.AST.Extensions;
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
        public const string CreateInstanceIdentifier ="createInstance";
        public const string DestroyInstanceIdentifier = "destroyInstance";
        public const string CreateInstanceJniIdentifier = CreateInstanceIdentifier + "Jni";
        public const string DestroyInstanceJniIdentifier = DestroyInstanceIdentifier + "Jni";
        public const string SetNativeInstanceIdentifier = "setNativeInstance";
        public const string GetNativeInstanceIdentifier = "getNativeInstance";
        public const string SetOwnsNativeInstanceIdentifier = "setOwnsNativeInstance";
        public const string AccessorParameterName = "value";

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

        public static bool IsVoid(Function function)
        {
            return function.OriginalReturnType.Type.Desugar().IsPrimitiveType(PrimitiveType.Void);
        }
    }
}
