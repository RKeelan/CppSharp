using CppSharp.AST;
using System;
using System.Collections.Generic;
using System.Text;

namespace CppSharp.Generators.Java
{
    public class JavaGenerator : Generator
    {
        private readonly JavaTypePrinter typePrinter;

        public JavaGenerator(BindingContext context) : base(context)
        {
            typePrinter = new JavaTypePrinter(context);
        }

        public override List<CodeGenerator> Generate(IEnumerable<TranslationUnit> units)
        {
            var outputs = new List<CodeGenerator>();

            var gen = new JavaSources(Context, units) { TypePrinter = typePrinter };
            outputs.Add(gen);

            return outputs;
        }

        public override bool SetupPasses()
        {
            return true;
        }

        protected override string TypePrinterDelegate(CppSharp.AST.Type type)
        {
            return type.Visit(typePrinter);
        }
    }
}
