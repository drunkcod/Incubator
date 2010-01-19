using System;
using NMeter.Migraine;

[assembly: ResourceMigration("Migraines", Up = "NMeter.Migraine.Create Table Migraines.sql", DependsOn = new string[0])]

namespace NMeter.Migraine
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ResourceMigrationAttribute : Attribute
    {
        readonly string name;

        public ResourceMigrationAttribute(string name) { this.name = name; }

        public string Name { get { return name; } }
        public string Up;
        public string[] DependsOn;
    }
}
