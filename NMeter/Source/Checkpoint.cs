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
        struct CheckpointMetric
        {
            public CheckpointMetric(string name, IList items) {
                this.Name = name;
                this.Items = items;
            }

            public readonly string Name;
            public readonly IList Items;
        }

        List<CheckpointMetric> metrics = new List<CheckpointMetric>();
        private Action<ICheckpointWriter> writeMetrics = x => { };

        public static Checkpoint For(Assembly assembly) {
            var methodMetrics = new MethodMetricsExtractor();
            var result = new Checkpoint();
            var types = assembly.GetTypes();
            Action<ClassMetrics> addClass = result.AddMetric<ClassMetrics>("Classes").Add;
            types.ForEach(x => addClass(new ClassMetrics { Name = x.FullName }));

            Action<MethodMetrics> addMethod = result.AddMetric<MethodMetrics>("Methods").Add;
            types.SelectMany(x => DefinedMethods(x)).ForEach(x => addMethod(methodMetrics.ComputeMetrics(x)));
            return result; 
        }

        public IList<T> AddMetric<T>(string name) {
            var list = new List<T>();
            metrics.Add(new CheckpointMetric(name, list));
            writeMetrics += x => x.WriteMetric(name, list);
            return list;
        }

        public void EachMetric(Action<string, IEnumerable> withMetric) {
            foreach(var item in metrics)
                withMetric(item.Name, item.Items);
        }

        public IList GetMetric(string name) {
            foreach(var item in metrics)
                if(item.Name == name)
                    return item.Items;
            throw new ArgumentException();
        }

        public IList<T> GetMetric<T>(string name) {
            return (IList<T>)GetMetric(name);
        }

        public void SaveTo(ICheckpointWriter writer)
        {
            writer.Write(this);
            writeMetrics(writer);

        }

        public Guid Id = Guid.NewGuid();
        public int MetricsCount { get { return metrics.Count; } }

        static IEnumerable<MethodInfo> DefinedMethods(Type type){
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(method => method.DeclaringType.Equals(type));
        }
    }
}
