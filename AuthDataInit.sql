IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'AuthUserDatabase')
BEGIN
	CREATE DATABASE AuthUserDatabase
END
GO

USE AuthUserDatabase
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AuthUser' and xtype='U')
BEGIN
    CREATE TABLE AuthUser (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NewID(),
        Name VARCHAR(50) UNIQUE,
		Password VARCHAR(50),
		RefreshToken TEXT,
		RefreshTokenExpiryTime DATETIME,
    )
END

--INSERT INTO AuthUser (Name, Password) VALUES('ABC', '12345');

SELECT * FROM AuthUser;

UPDATE AuthUser SET RefreshToken = NULL WHERE Name = 'faf';