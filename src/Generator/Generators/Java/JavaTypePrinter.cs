using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Extensions;
using CppSharp.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace CppSharp.Generators.Java
{
    public class JavaTypePrinter : TypePrinter
    {
        public string IntPtrType => "long ";

        public JavaTypePrinter()
        {
        }

        public JavaTypePrinter(BindingContext context)
        {
            Context = context;
        }

        public override TypePrinterResult VisitArrayType(ArrayType array, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitCILType(CILType type, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitClassTemplateDecl(ClassTemplate template)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitDeclaration(Declaration decl)
        {
            // RK 18-Mar-2022: Start with the simplest possible thing
            return decl.Name;
        }

        public override TypePrinterResult VisitDelegate(FunctionType function)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitDependentNameType(
            DependentNameType dependent, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitDependentTemplateSpecializationType(
            DependentTemplateSpecializationType template, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitEvent(Event @event)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitFieldDecl(Field field)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitFriend(Friend friend)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitFunctionDecl(Function function)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitFunctionTemplateDecl(
            FunctionTemplate template)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitFunctionTemplateSpecializationDecl(
            FunctionTemplateSpecialization specialization)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitFunctionType(FunctionType function,
            TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitInjectedClassNameType(
            InjectedClassNameType injected, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitMacroDefinition(MacroDefinition macro)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitMemberPointerType(
            MemberPointerType member, TypeQualifiers quals)
        {
            return member.QualifiedPointee.Visit(this);
        }

        public override TypePrinterResult VisitMethodDecl(Method method)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitNamespace(Namespace @namespace)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitNonTypeTemplateParameterDecl(
            NonTypeTemplateParameter nonTypeTemplateParameter)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitPackExpansionType(
            PackExpansionType packExpansionType, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitPointerType(PointerType pointer,
            TypeQualifiers quals)
        {
            if (MarshalKind == MarshalKind.NativeField && !pointer.Pointee.IsEnumType())
                return IntPtrType;

            if (pointer.Pointee is FunctionType)
                return pointer.Pointee.Visit(this, quals);

            var isManagedContext = ContextKind == TypePrinterContextKind.Managed;

            var pointee = pointer.Pointee.Desugar();

            if (isManagedContext &&
                new QualifiedType(pointer, quals).IsConstRefToPrimitive())
                return pointee.Visit(this);

            if ((pointee.IsDependent || pointee.IsClass())
                && ContextKind == TypePrinterContextKind.Native)
            {
                return IntPtrType;
            }

            return pointer.QualifiedPointee.Visit(this);
        }

        public override TypePrinterResult VisitPrimitiveType(PrimitiveType type,
            TypeQualifiers quals)
        {
            switch (type)
            {
                case PrimitiveType.Null:
                    return "null";
                case PrimitiveType.Void:
                    return "void";
                case PrimitiveType.Bool:
                    return "boolean";
                //case PrimitiveType.WideChar:
                //case PrimitiveType.Char:
                //case PrimitiveType.Char16:
                //case PrimitiveType.Char32:
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
                    return GetIntString(type, Context.TargetInfo);
                //case PrimitiveType.Int128:
                //case PrimitiveType.UInt128:
                // TODO RK 14-Mar-2022: Can I use BigInteger for these?
                case PrimitiveType.Float:
                    return "float";
                case PrimitiveType.Double:
                    return "double";
                    //case PrimitiveType.LongDouble:
                    //case PrimitiveType.Float128:
                    // TODO RK 14-Mar-2022: Can I use BigDecimal for these?
                    //case PrimitiveType.IntPtr:
                    //case PrimitiveType.UIntPtr:
                    //case PrimitiveType.String:
                    //    return "string";
            }

            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitProperty(Property property)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitTemplateParameterDecl(
            TypeTemplateParameter templateParameter)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitTemplateParameterSubstitutionType(
            TemplateParameterSubstitutionType param, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitTemplateParameterType(
            TemplateParameterType param, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitTemplateSpecializationType(
            TemplateSpecializationType template, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitTemplateTemplateParameterDecl(
            TemplateTemplateParameter templateTemplateParameter)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitTranslationUnit(TranslationUnit unit)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitTypeAliasTemplateDecl(
            TypeAliasTemplate typeAliasTemplate)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitTypedefDecl(TypedefDecl typedef)
        {
            return VisitDeclaration(typedef);
        }

        public override TypePrinterResult VisitTypedefNameDecl(TypedefNameDecl typedef)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitTypedefType(TypedefType typedef, TypeQualifiers quals)
        {
            if (typedef.Declaration.QualifiedOriginalName == "std::nullptr_t")
                return VisitPrimitiveType(PrimitiveType.Null, quals);

            if (typedef.Declaration.Type.IsPrimitiveType())
                return typedef.Declaration.Type.Visit(this);

            return base.VisitTypedefType(typedef, quals);
        }

        public override TypePrinterResult VisitUnaryTransformType(
            UnaryTransformType unaryTransformType, TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitUnsupportedType(UnsupportedType type,
            TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitVarTemplateDecl(VarTemplate template)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitVarTemplateSpecializationDecl(
            VarTemplateSpecialization template)
        {
            throw new NotImplementedException();
        }

        public override TypePrinterResult VisitVectorType(VectorType vectorType,
            TypeQualifiers quals)
        {
            throw new NotImplementedException();
        }

        static string GetIntString(PrimitiveType primitive, ParserTargetInfo targetInfo)
        {
            // There are three possible approaches to handling unsigned types:
            //  1. Ignore it, and let the consuming Java code handle it
            //  2. Widen the type (e.g., replace byte with short) so that the Java code can represent the
            // full range of potential values in a signed type
            //  3. Box the value in a custom class (e.g., SignedByte)
            //
            // All three approachs have advantages and disadvantages, but given that the Java language
            // designers seem to have elected for option 1:
            //  https://docs.oracle.com/javase/8/docs/api/java/lang/Integer.html#compareUnsigned-int-int-
            // I think it makes the best default.
            // It would be a good idea to add passes or driver options to support options 2 and 3, though.
            uint width = primitive.GetInfo(targetInfo, out _).Width;

            switch (width)
            {
                case 8:
                    return "byte";
                case 16:
                    return "short";
                case 32:
                    return "int";
                case 64:
                    return "long";
                default:
                    throw new NotImplementedException(primitive.ToString());
            }
        }
    }
}
