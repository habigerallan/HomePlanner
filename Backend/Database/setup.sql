BEGIN TRANSACTION;

BEGIN TRY

    CREATE TABLE Profile (
        ProfileID INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(255) NULL,
        LastName NVARCHAR(255) NULL,
        PreferredName NVARCHAR(255) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Email NVARCHAR(255) NULL
    );

    CREATE TABLE Admin (
        AdminID INT IDENTITY(1,1) PRIMARY KEY, 
        Username NVARCHAR(255) NOT NULL UNIQUE, 
        PasswordHash NVARCHAR(255) NOT NULL, 
        PasswordSalt NVARCHAR(255) NOT NULL, 
        ProfileID INT NULL, 
        CONSTRAINT FK_Admin_Profile FOREIGN KEY (ProfileID) REFERENCES Profile(ProfileID) ON DELETE SET NULL
    );

    CREATE TABLE Account (
        AccountID INT IDENTITY(1,1) PRIMARY KEY, 
        Username NVARCHAR(255) NOT NULL UNIQUE, 
        PasswordHash NVARCHAR(255) NOT NULL, 
        PasswordSalt NVARCHAR(255) NOT NULL, 
        ProfileID INT NULL, 
        CONSTRAINT FK_Account_Profile FOREIGN KEY (ProfileID) REFERENCES Profile(ProfileID) ON DELETE SET NULL
    );

    CREATE TABLE House (
        HouseID INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL
    );

    CREATE TABLE Task (
        TaskID INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Due DATETIME NULL,
        Complete BIT DEFAULT 0
    );

    CREATE TABLE HouseAccount (
        HouseID INT NOT NULL,
        AccountID INT NOT NULL,
        PRIMARY KEY (HouseID, AccountID),
        FOREIGN KEY (HouseID) REFERENCES House(HouseID) ON DELETE CASCADE,
        FOREIGN KEY (AccountID) REFERENCES Account(AccountID) ON DELETE CASCADE
    );

    CREATE TABLE HouseTask (
        HouseID INT NOT NULL,
        TaskID INT NOT NULL,
        PRIMARY KEY (HouseID, TaskID),
        FOREIGN KEY (HouseID) REFERENCES House(HouseID) ON DELETE CASCADE,
        FOREIGN KEY (TaskID) REFERENCES Task(TaskID) ON DELETE CASCADE
    );

    CREATE TABLE TaskAccount (
        TaskID INT NOT NULL,
        AccountID INT NOT NULL,
        PRIMARY KEY (TaskID, AccountID),
        FOREIGN KEY (TaskID) REFERENCES Task(TaskID) ON DELETE CASCADE,
        FOREIGN KEY (AccountID) REFERENCES Account(AccountID) ON DELETE CASCADE
    );

    CREATE INDEX idx_account_username ON Account (Username);
    CREATE INDEX idx_admin_username ON Admin (Username);
    CREATE INDEX idx_profile_email ON Profile (Email);

    COMMIT TRANSACTION;
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Transaction rolled back due to error.';
    THROW;
END CATCH;
GO
