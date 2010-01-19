create table Checkpoints(
	LocalId int unique identity,
	Id uniqueidentifier,
	Project varchar(max),
	Name varchar(max),
	Created datetime)