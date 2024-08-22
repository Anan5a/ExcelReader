CREATE TABLE Users
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,   -- Auto-incrementing column
    UUID NVARCHAR(255) NOT NULL UNIQUE,    -- UUID column with a unique constraint
    Name NVARCHAR(255) NOT NULL UNIQUE,    -- Name column with a unique constraint
    Email NVARCHAR(255) NOT NULL UNIQUE,   -- Email column with a unique constraint
    CreatedAt DATETIME NOT NULL            -- CreatedAt column with datetime values
);
