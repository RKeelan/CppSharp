using CppSharp.AST;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var javaSoure = new JavaSources(Context, units) { TypePrinter = typePrinter };
            outputs.Add(javaSoure);

            var jniSource = new JniSources(Context, units);
            outputs.Add(jniSource);

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
