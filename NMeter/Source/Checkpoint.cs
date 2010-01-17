using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Xlnt.Stuff;
using System.Collections.ObjectModel;
using System.Collections;

namespace NMeter
{
    public class ClassMetrics {
        public string Name { get; set; }
    }

    public class Checkpoint
    {
        List<KeyValuePair<string, object>> metrics = new List<KeyValuePair<string, object>>();
        List<ClassMetrics> classes = new List<ClassMetrics>();
        List<MethodMetrics> methods = new List<MethodMetrics>();

        public static Checkpoint For(Assembly assembly) {
            var methodMetrics = new MethodMetricsExtractor();
            var result = new Checkpoint();
            var types = assembly.GetTypes();
            types.ForEach(x => result.classes.Add(new ClassMetrics { Name = x.FullName }));
            types.SelectMany(x => DefinedMethods(x)).ForEach(x => result.methods.Add(methodMetrics.ComputeMetrics(x)));
            return result; 
        }

        public void AddMetric<T>(string name) {
            metrics.Add(new KeyValuePair<string,object>(name, new List<T>()));
        }
        public IEnumerable GetMetric(string name) { return new object[0]; }

        public Guid Id = Guid.NewGuid();
        public int MetricsCount { get { return 0; } }
        public List<ClassMetrics> Classes { get { return classes; } }
        public List<MethodMetrics> Methods { get { return methods; } }

        static IEnumerable<MethodInfo> DefinedMethods(Type type){
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(method => method.DeclaringType.Equals(type));
        }
    }
}
