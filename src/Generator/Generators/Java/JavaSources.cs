using CppSharp.AST;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CppSharp.Generators.Java
{
    public class JavaSources : CodeGenerator
    {
        public JavaTypePrinter TypePrinter { get; set; }

        public override string FileExtension => "java";

        public JavaSources(BindingContext context, IEnumerable<TranslationUnit> units)
            : base(context, units)
        {
            TypePrinter = new JavaTypePrinter(Context);
        }

        #region Identifiers
        // Extracted from:
        // https://docs.oracle.com/javase/specs/jls/se11/html/jls-3.html#jls-3.9
        static readonly string[] ReservedKeywords =
        {
            "_", "abstract", "assert", "boolean", "break", "byte", "case", "catch", "char",
            "class", "const", "continue", "default", "do", "double", "else", "enum", "extends",
            "false", "final", "finally", "float", "for", "goto", "if", "implements", "import",
            "instanceof", "int", "interface", "long", "native", "new", "null", "package",
            "private", "protected", "public", "return", "short", "static", "strictfp", "super",
            "switch", "synchronized", "this", "throw", "throws", "transient", "true", "try", "var",
            "void", "volatile", "while",
        };

        public static string SafeIdentifier(string id)
        {
            if (id.All(char.IsLetterOrDigit))
                return ReservedKeywords.Contains(id) ? "_" + id : id;

            return new string((from c in id
                               where c != '$'
                               select char.IsLetterOrDigit(c) ? c : '_').ToArray());
        }
        #endregion

        public Module Module => TranslationUnits.Count == 0 ?
            Context.Options.SystemModule : TranslationUnit.Module;

        public override void Process()
        {
            GenerateFilePreamble(CommentKind.JavaDoc);
            GenerateMain();
        }

         public virtual void GenerateMain()
        {
            VisitNamespace(TranslationUnit);
        }

        public override void GenerateClassSpecifier(Class @class)
        {
            Write("public ");

            if (@class.IsAbstract)
                Write("abstract ");

            Write(@class.IsInterface ? "interface" : "class");
            Write($" {@class.Name} ");
        }

        public override bool VisitClassDecl(Class @class)
        {
            if (!@class.IsGenerated || @class.IsIncomplete)
                return false;

            PushBlock(BlockKind.Class, @class);
            NewLine();
            GenerateClassSpecifier(@class);
            WriteOpenBraceAndIndent();

            VisitDeclContext(@class);

            GenerateClassConstructors(@class);
            GenerateClassMethods(@class);

            UnindentAndWriteCloseBrace();
            PopBlock(NewLineKind.BeforeNextBlock);

            return true;
        }

        #region Constructors
        public virtual void GenerateClassConstructors(Class @class)
        {
            if (@class.IsStatic)
                return;

            PushBlock(BlockKind.Field);
            WriteLine($"private long {JavaHelpers.NativeInstanceAddrIdentifier};");
            WriteLine($"private boolean {JavaHelpers.OwnsNativeInstanceIdentifier};");
            PopBlock(NewLineKind.BeforeNextBlock);

            foreach (var ctor in @class.Constructors.Where(c => !c.IsImplicit))
            {
                if (ASTUtils.CheckIgnoreMethod(ctor))
                    continue;

                GenerateMethod(ctor, @class);
            }
            GenerateCloseMethod();
        }

        private void GenerateCloseMethod()
        {
            PushBlock(BlockKind.Field);
            WriteLine($"private native void {JavaHelpers.DestroyInstanceJniIdentifier}();");
            PopBlock(NewLineKind.BeforeNextBlock);

            PushBlock(BlockKind.Method);
            WriteLine($"public void close()");
            WriteOpenBraceAndIndent();

            WriteLine($"if (!{JavaHelpers.OwnsNativeInstanceIdentifier})");
            WriteOpenBraceAndIndent();
            WriteLine($"return;");
            UnindentAndWriteCloseBrace();
            NewLine();

            WriteLine($"if ({JavaHelpers.NativeInstanceAddrIdentifier} == 0)");
            WriteOpenBraceAndIndent();
            WriteLine($"return;");
            UnindentAndWriteCloseBrace();
            NewLine();

            WriteLine($"{JavaHelpers.DestroyInstanceJniIdentifier}();");
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

            GenerateJniMethoodSpecifier(method, out string jniMethod);
            Write(JavaHelpers.GetAccess(method.Access));
            GenerateMethodSpecifier(method);
            NewLine();
            WriteOpenBraceAndIndent();

            if (JavaHelpers.IsVoid(method))
                Write($"{jniMethod}");
            else
                Write($"return {jniMethod}");

            WriteLine($"({GetCallSiteParameters(method)});");

            UnindentAndWriteCloseBrace();
            PopBlock(NewLineKind.BeforeNextBlock);
        }

        public void GenerateJniMethoodSpecifier(Method method, out string jniMethod)
        {
            string printedType = method.OriginalReturnType.Visit(TypePrinter);
            string functionName = method.IsConstructor ?
                JavaHelpers.CreateInstanceIdentifier :
                GetMethodIdentifier(method);
            jniMethod = $"{functionName}Jni";

            Write($"private native {printedType} {jniMethod}(");
            Write(FormatMethodParameters(method.Parameters));
            WriteLine(");");
        }

        public override void GenerateMethodSpecifier(Method method, MethodSpecifierKind? kind = null)
        {
            bool isTemplateMethod = method.Parameters.Any(
                p => p.Kind == ParameterKind.Extension);

            if (method.IsGeneratedOverride())
                WriteLine("@Override");

            if (method.IsStatic)
                Write("static ");

            if (method.IsPure)
                Write("abstract ");

            var functionName = GetMethodIdentifier(method);

            var printedType = method.OriginalReturnType.Visit(TypePrinter);

            if (method.IsConstructor || method.IsDestructor)
                Write($"{functionName}(");
            else
                Write("{0} {1}(", printedType, functionName);

            Write(FormatMethodParameters(method.Parameters));

            Write(")");
        }

        public static string GetMethodIdentifier(Method method)
        {
            if (method.IsConstructor)
                return method.Namespace.Name;

            return GetFunctionIdentifier(method);
        }

        public static string GetFunctionIdentifier(Function function)
        {
            if (function.IsOperator)
                throw new NotImplementedException("Operator overload");

            return function.Name;
        }

        private string FormatMethodParameters(IEnumerable<Parameter> @params)
        {
            return TypePrinter.VisitParameters(@params, true).Type;
        }

        private string GetCallSiteParameters(Function function)
        {
            return string.Join(", ", function.Parameters.Select(p => p.Name));
        }
        #endregion

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
