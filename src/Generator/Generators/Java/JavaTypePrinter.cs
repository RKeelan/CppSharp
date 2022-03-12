using System;
using System.Collections.Generic;
using System.Text;

namespace CppSharp.Generators.Java
{
    public  class JavaTypePrinter : TypePrinter
    {

        public JavaTypePrinter()
        {
        }

        public JavaTypePrinter(BindingContext context)
        {
            Context = context;
        }
    }
}
