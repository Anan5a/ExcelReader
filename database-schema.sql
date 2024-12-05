CREATE TABLE [users] (
	[user_id] bigint IDENTITY(1,1) NOT NULL UNIQUE,
	[name] nvarchar(max) NOT NULL,
	[email] nvarchar(255) NOT NULL UNIQUE,
	[password] nvarchar(max) NOT NULL,
	[role_id] bigint NOT NULL,
	[password_reset_id] nvarchar(max),
	[verified_at] datetime2(7),
	[status] nvarchar(max) NOT NULL DEFAULT 'ok',
	[social_login] nvarchar(max),
	[created_at] datetime2(7) NOT NULL DEFAULT 'NOW()',
	[updated_at] datetime2(7),
	[deleted_at] datetime2(7),
	PRIMARY KEY ([user_id])
);

CREATE TABLE [roles] (
	[role_id] bigint IDENTITY(1,1) NOT NULL UNIQUE,
	[role_name] nvarchar(20) NOT NULL UNIQUE,
	PRIMARY KEY ([role_id])
);

CREATE TABLE [file_metadata] (
	[file_metadata_id] bigint IDENTITY(1,1) NOT NULL UNIQUE,
	[file_name] nvarchar(255) NOT NULL,
	[file_name_system] nvarchar(255) NOT NULL UNIQUE,
	[filesize_bytes] bigint,
	[user_id] bigint NOT NULL,
	[created_at] datetime2(7) NOT NULL,
	[updated_at] datetime2(7),
	[deleted_at] datetime2(7),
	PRIMARY KEY ([file_metadata_id])
);

CREATE TABLE [groups] (
	[group_id] bigint IDENTITY(1,1) NOT NULL UNIQUE,
	[group_name] nvarchar(255) NOT NULL UNIQUE,
	[created_at] datetime2(7) NOT NULL,
	[deleted_at] datetime2(7),
	PRIMARY KEY ([group_id])
);

CREATE TABLE [group_memberships] (
	[group_membership_id] bigint IDENTITY(1,1) NOT NULL UNIQUE,
	[group_id] bigint NOT NULL,
	[user_id] bigint NOT NULL,
	[created_at] datetime2(7) NOT NULL,
	[deleted_at] datetime2(7),
	PRIMARY KEY ([group_membership_id])
);

CREATE TABLE [chat_history] (
	[chat_history_id] bigint IDENTITY(1,1) NOT NULL UNIQUE,
	[sender_id] bigint NOT NULL,
	[receiver_id] bigint NOT NULL,
	[content] nvarchar(max) NOT NULL,
	[created_at] datetime2(7) NOT NULL,
	PRIMARY KEY ([chat_history_id])
);

ALTER TABLE [users] ADD CONSTRAINT [users_fk4] FOREIGN KEY ([role_id]) REFERENCES [roles]([role_id]);

ALTER TABLE [file_metadata] ADD CONSTRAINT [file_metadata_fk3] FOREIGN KEY ([user_id]) REFERENCES [users]([user_id]);

ALTER TABLE [group_memberships] ADD CONSTRAINT [group_memberships_fk1] FOREIGN KEY ([group_id]) REFERENCES [groups]([group_id]);

ALTER TABLE [group_memberships] ADD CONSTRAINT [group_memberships_fk2] FOREIGN KEY ([user_id]) REFERENCES [users]([user_id]);
ALTER TABLE [chat_history] ADD CONSTRAINT [chat_history_fk1] FOREIGN KEY ([sender_id]) REFERENCES [users]([user_id]);

ALTER TABLE [chat_history] ADD CONSTRAINT [chat_history_fk2] FOREIGN KEY ([receiver_id]) REFERENCES [users]([user_id]);


-----
--- insert initial role data

INSERT INTO roles(role_name) values ('user'),('admin'),('super_admin'),('guest');