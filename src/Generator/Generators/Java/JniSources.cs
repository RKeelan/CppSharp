using CppSharp.AST;
using CppSharp.Generators.C;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CppSharp.Generators.Java
{
    public class JniSources : CCodeGenerator
    {
        public const string JNI_EXPORT = "extern \"C\" JNIEXPORT";
        public const string JNI_CALL = "JNICALL";
        public const string JNI_COMMON_PARAMETERS_DECL = "JNIEnv *env, jobject obj";
        public const string JNI_COMMON_PARAMETERS = "env, obj";
        public const string CPP_SHARP_JNI_RUNTIME_NAMESPACE = "CppSharp::Runtime::";

        public JniSources(BindingContext context, IEnumerable<TranslationUnit> units)
            : base(context, units)
        {
            typePrinter = new JniTypePrinter(context);
        }

        public override string FileExtension { get { return "cpp"; } }

        public override void Process()
        {
            GenerateFilePreamble(CommentKind.BCPL);
            NewLine();

            PushBlock(BlockKind.Includes);
            WriteLine("#include \"CppSharpJniRuntime.h\"");
            WriteLine($"#include \"{TranslationUnit.FileNameWithoutExtension}.h\"");
            NewLine();
            PopBlock();

            GenerateMain();

            PushBlock(BlockKind.Footer);
            PopBlock();
        }
        
        public override bool VisitClassDecl(Class @class)
        {
            if (!@class.IsGenerated || @class.IsIncomplete)
                return false;

            VisitDeclContext(@class);
            GenerateClassConstructors(@class);
            GenerateClassMethods(@class);

            return true;
        }

        #region Constructors
        public virtual void GenerateClassConstructors(Class @class)
        {
            if (@class.IsStatic)
                return;

            foreach (var ctor in @class.Constructors.Where(c => !c.IsImplicit))
            {
                if (ASTUtils.CheckIgnoreMethod(ctor))
                    continue;

                GenerateMethod(ctor, @class);
            }
            GenerateDestroyMethod(@class);
        }

        private void GenerateDestroyMethod(Class @class)
        {
            PushBlock(BlockKind.Method);
            WriteLine(JNI_EXPORT);
            WriteLine($"void {JNI_CALL} {GetJniFunctionPrefix(@class)}{JavaHelpers.DestroyInstanceJniIdentifier}" +
                $"({JNI_COMMON_PARAMETERS_DECL})");

            WriteOpenBraceAndIndent();

            GenerateGetNativeInstance(@class);

            Write($"if ({JavaHelpers.NativeInstanceIdentifier} != nullptr) ");
            WriteOpenBraceAndIndent();
            WriteLine($"delete {JavaHelpers.NativeInstanceIdentifier};");
            WriteLine($"{JavaHelpers.NativeInstanceIdentifier} = nullptr;");
            UnindentAndWriteCloseBrace();
            NewLine();
            Write($"{CPP_SHARP_JNI_RUNTIME_NAMESPACE}{JavaHelpers.SetNativeInstanceIdentifier}");
            WriteLine($"({JNI_COMMON_PARAMETERS}, {JavaHelpers.NativeInstanceIdentifier});");

            Write($"{CPP_SHARP_JNI_RUNTIME_NAMESPACE}{JavaHelpers.SetOwnsNativeInstanceIdentifier}");
            WriteLine($"({JNI_COMMON_PARAMETERS}, JNI_FALSE);");

            UnindentAndWriteCloseBrace();
            PopBlock(NewLineKind.BeforeNextBlock);
        }
        #endregion

        #region Methods / Functions
        public void GenerateClassMethods(Class @class)
        {
            foreach (var method in @class.Methods)
            {
                if (ASTUtils.CheckIgnoreMethod(method))
                    continue;

                if (method.IsConstructor)
                    continue;

                // Do not generate property getter/setter methods as they will be generated
                // as part of properties generation.
                var field = (method?.AssociatedDeclaration as Property)?.Field;
                if (field != null)
                    continue;

                GenerateMethod(method, @class);
            }
        }

        public void GenerateMethod(Method method, Class @class)
        {
            PushBlock(BlockKind.Method, method);
            GenerateDeclarationCommon(method);

            WriteLine(JNI_EXPORT);
            GenerateMethodSpecifier(method);
            NewLine();
            WriteOpenBraceAndIndent();

            if (method.Kind == CXXMethodKind.Constructor)
            {
                Write($"{@class.Name}* {JavaHelpers.NativeInstanceIdentifier} = ");
                WriteLine($"new {@class.Name}({GetCallSiteParameters(method)});");

                Write($"{CPP_SHARP_JNI_RUNTIME_NAMESPACE}{JavaHelpers.SetNativeInstanceIdentifier}");
                WriteLine($"({JNI_COMMON_PARAMETERS}, {JavaHelpers.NativeInstanceIdentifier});");

                Write($"{CPP_SHARP_JNI_RUNTIME_NAMESPACE}{JavaHelpers.SetOwnsNativeInstanceIdentifier}");
                WriteLine($"({JNI_COMMON_PARAMETERS}, JNI_TRUE);");
            }
            else
            {
                GenerateGetNativeInstance(@class);
                if (JavaHelpers.IsVoid(method))
                    WriteLine($"{JavaHelpers.NativeInstanceIdentifier}->{method.OriginalName}({GetCallSiteParameters(method)});");
                else
                    WriteLine($"return {JavaHelpers.NativeInstanceIdentifier}->{method.OriginalName}({GetCallSiteParameters(method)});");
            }

            UnindentAndWriteCloseBrace();
            PopBlock(NewLineKind.BeforeNextBlock);
        }

        private void GenerateGetNativeInstance(Class @class)
        {
            Write($"{@class.Name}* {JavaHelpers.NativeInstanceIdentifier} = ");
            Write($"{CPP_SHARP_JNI_RUNTIME_NAMESPACE}{JavaHelpers.GetNativeInstanceIdentifier}");
            WriteLine($"<{@class.Name}>({JNI_COMMON_PARAMETERS});");
        }

        public override string GetMethodIdentifier(Function function, TypePrinterContextKind context = TypePrinterContextKind.Managed)
        {
            return base.GetMethodIdentifier(function, context) + "Jni";
        }

        public override void GenerateMethodSpecifier(Method method, MethodSpecifierKind? kind = null)
        {
            Class @class = method.Namespace as Class;
            var methodId = method.IsConstructor ?
                GetJniFunctionPrefix(@class) + JavaHelpers.CreateInstanceJniIdentifier :
                GetJniFunctionPrefix(@class) + GetMethodIdentifier(method);
            var returnType = method.ReturnType.Visit(CTypePrinter);
            Write($"{returnType} {JNI_CALL} {methodId}(");
            GenerateMethodParameters(method);
            Write(")");
        }

        public override void GenerateMethodParameters(Function function)
        {
            Write(JNI_COMMON_PARAMETERS_DECL);
            if(function.Parameters.Count > 0)
            {
                Write(", ");
            }
            base.GenerateMethodParameters(function);
        }

        public string GetJniFunctionPrefix(Class @class)
        {
            if (@class is null)
                return string.Empty;

            //  TODO RK 02-Apr-2022: Deal with namespaces
            return $"Java_{@class}_";
        }

        public string GetFunctionIdentifier(string jniPrefix, Function function)
        {
            if (function.IsOperator)
                throw new NotImplementedException("Operator overload");

            return jniPrefix + function.Name + "Jni";
        }

        private string GetCallSiteParameters(Function function)
        {
            return string.Join(", ", function.Parameters.Select(p => p.Name));
        }
        #endregion

        public virtual void GenerateMain()
        {
            VisitNamespace(TranslationUnit);
        }

        public override bool VisitProperty(Property property)
        {
            return true;
        }

        public override bool VisitFieldDecl(Field field)
        {
            return true;
        }

        public override bool VisitFunctionDecl(Function function)
        {
            return true;
        }

        public override bool VisitMethodDecl(Method method)
        {
            return true;
        }

        public override bool VisitParameterDecl(Parameter parameter)
        {
            return true;
        }

        public override bool VisitEnumDecl(Enumeration @enum)
        {
            return true;
        }

        public override bool VisitVariableDecl(Variable variable)
        {
            return true;
        }
    }
}
