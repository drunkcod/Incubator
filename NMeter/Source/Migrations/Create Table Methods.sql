create table Methods(
	[Checkpoint] uniqueidentifier,
	Signature varchar(max),
	ParameterCount int,
	InstructionCount int,
	IsGenerated bit,
	IsStatic bit,
	Fingerprint binary(16))
