﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Xlnt.Data;
using Xlnt.Stuff;
using System.Reflection;
using System.Diagnostics;
using NMeter.Migraine;

namespace NMeter
{
    class BulkCopyCheckpointWriter
    {
        static readonly MethodInfo WriteMetric = typeof(BulkCopyCheckpointWriter).GetMethod("WriteMetricCore", BindingFlags.Instance | BindingFlags.NonPublic);

        readonly string connectionString;
        SqlBulkCopy bulkCopy;
        int checkpointId;

        public BulkCopyCheckpointWriter(string connectionString) {
            this.connectionString = connectionString;
            bulkCopy = new SqlBulkCopy(connectionString);
        }

        public void Write(Checkpoint checkpoint) {
            WriteCheckpointCore(checkpoint);
            checkpoint.EachMetric((name, items) => BindWriteMetric(name, items));
        }

        void BindWriteMetric(params object[] arguments) {
            WriteMetric.MakeGenericMethod(arguments[1].GetType().GetGenericArguments()).Invoke(this, arguments);
        }

        void WriteCheckpointCore(Checkpoint checkpoint) {
            using(var db = new SqlConnection(connectionString))
            using(var command = db.CreateCommand()) {
                command.CommandText = "insert Checkpoints(Id, Project, Name, Created) output Inserted.LocalId values(@Id, @Project, @Name, @Created)";
                command.Parameters.AddWithValue("@Id", checkpoint.Id);
                command.Parameters.AddWithValue("@Project", "NMeter");
                command.Parameters.AddWithValue("@Name", "");
                command.Parameters.AddWithValue("@Created", DateTime.Now);
                db.Open();
                checkpointId = (int)command.ExecuteScalar();
            }
        }

        void WriteMetricCore<T>(string name, IList<T> items) {
            bulkCopy.DestinationTableName = name;
            var data = items.AsDataReader();
            data.MapAll();
            data.ColumnMappings.Add("Checkpoint", x => checkpointId);
            bulkCopy.ColumnMappings.Clear();
            data.ColumnMappings.ForEach(x => bulkCopy.ColumnMappings.Add(x.Name, x.Name));
            bulkCopy.WriteToServer(data);
        }
    }

    class Program
    {
        const string ConnectionString = "Server=.;Initial Catalog=MethodFingerprints;Integrated Security=SSPI";

        static void Main(string[] args) {
            Migrations.ApplyMissing(ConnectionString);
            var time = Stopwatch.StartNew();
            var checkpoint = Checkpoint.For(typeof(Program).Assembly);
            Console.WriteLine("Checkpoint generation took {0}", time.Elapsed);

            var writer = new BulkCopyCheckpointWriter(ConnectionString);
            writer.Write(checkpoint);
            Console.WriteLine("Saved after {0}", time.Elapsed);

        }
    }
}
