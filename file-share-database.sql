CREATE TABLE [users] (
	[id] bigint IDENTITY(1,1) NOT NULL UNIQUE,
	[name] nvarchar(max) NOT NULL,
	[email] nvarchar(255) NOT NULL UNIQUE,
	[password] nvarchar(max) NOT NULL,
	[created_at] datetime2(7) NOT NULL,
	[updated_at] datetime2(7),
	[deleted_at] datetime2(7),
	[role_id] bigint NOT NULL,
	[password_reset_id] nvarchar(max),
	[verified_at] datetime2(7),
	[status] nvarchar(max) NOT NULL DEFAULT 'pending',
	PRIMARY KEY ([id]),
	CONSTRAINT CK_Users_ValidStatus CHECK (status IN ('ok','pending', 'disabled'))
);

CREATE TABLE [roles] (
	[id] bigint IDENTITY(1,1) NOT NULL UNIQUE,
	[role_name] nvarchar(20) NOT NULL UNIQUE,
	PRIMARY KEY ([id]),
	CONSTRAINT CK_Role_ValidRoles CHECK (role_name IN ('super_admin','admin', 'user', 'guest'))
);

CREATE TABLE [file_metadata] (
	[id] bigint IDENTITY(1,1) NOT NULL UNIQUE,
	[file_name] nvarchar(32) NOT NULL,
	[file_name_system] nvarchar(32) NOT NULL UNIQUE,
	[user_id] bigint NOT NULL,
	[filesize_bytes] bigint ,
	[created_at] datetime2(7) NOT NULL,
	[updated_at] datetime2(7),
	[deleted_at] datetime2(7),
	PRIMARY KEY ([id])
);

ALTER TABLE [users] ADD CONSTRAINT [users_fk7] FOREIGN KEY ([role_id]) REFERENCES [roles]([id]);

ALTER TABLE [file_metadata] ADD CONSTRAINT [file_metadata_fk3] FOREIGN KEY ([user_id]) REFERENCES [users]([id]);