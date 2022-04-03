using CppSharp.AST;
using CppSharp.Extensions;
using CppSharp.Generators.C;
using CppSharp.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace CppSharp.Generators.Java
{
    public class JniTypePrinter : CppTypePrinter
    {
        public JniTypePrinter(BindingContext context) : base(context) { }

        public override TypePrinterResult VisitPrimitiveType(PrimitiveType primitive)
        {
            switch (primitive)
            {
                case PrimitiveType.Bool: return "jboolean";
                case PrimitiveType.Void: return "void";

                case PrimitiveType.Char16:
                case PrimitiveType.Char32:
                case PrimitiveType.WideChar:
                    return GetCharString(primitive, Context.TargetInfo);

                case PrimitiveType.Char:
                    return Options.MarshalCharAsManagedChar &&
                        ContextKind != TypePrinterContextKind.Native
                        ? GetCharString(primitive, Context.TargetInfo)
                        : "jbyte"; 

                case PrimitiveType.SChar:
                case PrimitiveType.UChar:
                case PrimitiveType.Short:
                case PrimitiveType.UShort:
                case PrimitiveType.Int:
                case PrimitiveType.UInt:
                case PrimitiveType.Long:
                case PrimitiveType.ULong:
                case PrimitiveType.LongLong:
                case PrimitiveType.ULongLong:
                case PrimitiveType.Int128:
                case PrimitiveType.UInt128:
                    return GetIntString(primitive, Context.TargetInfo);

                case PrimitiveType.Float: return "jfloat";
                case PrimitiveType.Double: return "jdouble";
            }

            return base.VisitPrimitiveType(primitive);
        }

        string GetCharString(PrimitiveType primitive, ParserTargetInfo targetInfo)
        {
            uint witdh = primitive.GetInfo(targetInfo, out _).Width;
            if (witdh == 16)
                return "jchar";
            else
                return base.VisitPrimitiveType(primitive);
        }

        static string GetIntString(PrimitiveType primitive, ParserTargetInfo targetInfo)
        {
            uint width = primitive.GetInfo(targetInfo, out bool signed).Width;

            if (!signed)
                throw new NotImplementedException();

            switch (width)
            {
                case 8:
                    return "jbyte";
                case 16:
                    return "jshort";
                case 32:
                    return "jint";
                case 64:
                    return "jlong";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
