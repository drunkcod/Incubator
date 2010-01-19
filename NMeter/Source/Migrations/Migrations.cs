using NMeter.Migraine;

[assembly: ResourceMigration("Methods", Up = "NMeter.Migrations.Create Table Methods.sql", DependsOn = new[]{ "Checkpoints" })]
[assembly: ResourceMigration("Classes", Up = "NMeter.Migrations.Create Table Classes.sql", DependsOn = new[] { "Checkpoints" })]
[assembly: ResourceMigration("Checkpoints", Up = "NMeter.Migrations.Create Table Checkpoints.sql")]