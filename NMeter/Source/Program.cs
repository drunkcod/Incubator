using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Xlnt.Data;
using Xlnt.Stuff;
using System.Reflection;
using System.Diagnostics;

namespace NMeter
{
    class BulkCopyCheckpointWriter
    {
        static readonly MethodInfo WriteMetricMethod = typeof(BulkCopyCheckpointWriter).GetMethod("WriteMetric", BindingFlags.Instance | BindingFlags.NonPublic);
        SqlBulkCopy bulkCopy;

        public BulkCopyCheckpointWriter(string connnectionString) {
            bulkCopy = new SqlBulkCopy(connnectionString);
        }

        public void Write(Checkpoint checkpoint) {
            checkpoint.EachMetric((name, items) => BindWriteMetric(name, items));
        }

        void BindWriteMetric(params object[] arguments) {
            WriteMetricMethod.MakeGenericMethod(arguments[1].GetType().GetGenericArguments()).Invoke(this, arguments);
        }

        void WriteMetric<T>(string name, IList<T> items) {
            bulkCopy.DestinationTableName = name;
            var data = items.AsDataReader();
            data.MapAll();
            bulkCopy.ColumnMappings.Clear();
            data.ColumnMappings.ForEach(x => bulkCopy.ColumnMappings.Add(x.Name, x.Name));
            bulkCopy.WriteToServer(data);
        }
    }

    class Program
    {
        static void Main(string[] args) {
            var time = Stopwatch.StartNew();
            var checkpoint = Checkpoint.For(typeof(Program).Assembly);
            Console.WriteLine("Checkpoint generation took {0}", time.Elapsed);

            var writer = new BulkCopyCheckpointWriter("Server=.;Initial Catalog=MethodFingerprints;Integrated Security=SSPI");
            writer.Write(checkpoint);
            Console.WriteLine("Saved after {0}", time.Elapsed);

        }
    }
}
