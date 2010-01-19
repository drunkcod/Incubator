using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.SqlClient;
using System.IO;

namespace NMeter.Migraine
{
    class ResourceMigration
    {
        readonly string source;
        public ResourceMigration(string name, string source) {
            this.Name = name;
            this.source = source;
        }

        public string Name;
        public string[] DependsOn;

        public string GetCommandText() {
            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(source)) {
                if(stream == null)
                    throw new ArgumentException("Failed to locate:" + source);
                using(var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
    }

    public static class Migrations
    {
        public static void ApplyMissing(string connectionString) {
            using(var db = new SqlConnection(connectionString)) {
                db.Open();
                var appliedMigrations = new HashSet<string>();
                WithAppliedMigrations(db, x => appliedMigrations.Add(x));

                var migrationByname = new Dictionary<string, ResourceMigration>();
                foreach(var item in FindMigrations()) {
                    if(appliedMigrations.Contains(item.Name))
                        continue;
                    migrationByname.Add(item.Name, item);
                }
                var applyStack = new Stack<ResourceMigration>();
                foreach(var item in migrationByname.Values) {
                    if(appliedMigrations.Contains(item.Name))
                        continue;
                    applyStack.Push(item);
                    while(applyStack.Count > 0) {
                        var current = applyStack.Peek();
                        foreach(var dependency in current.DependsOn)
                            if(!appliedMigrations.Contains(dependency))
                                applyStack.Push(migrationByname[dependency]);
                        if(current == applyStack.Peek()) {
                            applyStack.Pop();
                            using(var command = db.CreateCommand()) {
                                command.CommandText = current.GetCommandText();
                                command.ExecuteNonQuery();

                                appliedMigrations.Add(current.Name);
                                command.CommandText = "insert into Migraines(Name, Created) values(@Name, @Created)";
                                command.Parameters.AddWithValue("@Name", current.Name);
                                command.Parameters.AddWithValue("@Created", DateTime.Now);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        static void WithAppliedMigrations(SqlConnection db, Action<string> withMigration) {
            using(var cmd = db.CreateCommand()) {
                using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NMeter.Migraine.Find Migrains.sql"))
                using(var reader = new StreamReader(stream))
                    cmd.CommandText = reader.ReadToEnd();
                using(var data = cmd.ExecuteReader())
                    while(data.Read())
                        withMigration(data.GetString(0));
            }
        }


        static IEnumerable<ResourceMigration> FindMigrations() {
            var NoDependencies = new []{ "Migraines" };
            var migrations = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(ResourceMigrationAttribute), false);
            foreach(ResourceMigrationAttribute item in migrations) {
                var migration = new ResourceMigration(item.Name, item.Up){ DependsOn = item.DependsOn ?? NoDependencies };
                yield return migration;
            }
        }
    }
}
