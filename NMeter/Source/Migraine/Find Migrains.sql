set nocount on
if exists(select * from sys.objects where name = 'Migraines' and type = 'U')
	select Name from Migraines