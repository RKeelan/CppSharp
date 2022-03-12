using CppSharp.AST;
using CppSharp.Generators;
using System;
using System.Collections.Generic;
using System.Text;

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

        public override void Process()
        {
            // TODO RK: Implement
        }
    }
}
