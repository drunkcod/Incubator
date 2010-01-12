using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Xlnt.Stuff;

namespace NMeter
{
    public class Checkpoint
    {
        int classCount;
        int methodCount;

        public static Checkpoint For(Assembly assembly) {
            var result = new Checkpoint();
            var types = assembly.GetTypes();
            result.classCount = types.Length;
            result.methodCount = types.SelectMany(x => DefinedMethods(x)).Count();
            return result; 
        }
        
        public int ClassCount { get { return classCount; } }
        public int MethodCount { get { return methodCount; } }

        static IEnumerable<MethodInfo> DefinedMethods(Type type){
            return type.GetMethods()
                .Where(method => method.DeclaringType.Equals(type));
        }

    }
}
