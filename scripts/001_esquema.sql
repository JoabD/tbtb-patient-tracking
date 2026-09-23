IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923225646_InitialCreate'
)
BEGIN
    CREATE TABLE [Patients] (
        [Id] uniqueidentifier NOT NULL,
        [FullName] varchar(150) NOT NULL,
        [DocumentType] varchar(50) NOT NULL,
        [DocumentNumber] varchar(50) NOT NULL,
        [Country] char(2) NOT NULL,
        [City] varchar(100) NOT NULL,
        [Phone] varchar(20) NOT NULL,
        [Email] varchar(100) NULL,
        [TreatmentStartDate] date NOT NULL,
        [TrackingStatus] varchar(20) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [PrivacyConsentAt] datetimeoffset NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] varchar(50) NOT NULL,
        CONSTRAINT [PK_Patients] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923225646_InitialCreate'
)
BEGIN
    CREATE TABLE [Contacts] (
        [Id] uniqueidentifier NOT NULL,
        [PatientId] uniqueidentifier NOT NULL,
        [GestorUsername] varchar(50) NOT NULL,
        [ContactDate] datetimeoffset NOT NULL,
        [Channel] varchar(20) NOT NULL,
        [ResultCode] varchar(50) NOT NULL,
        [Observations] varchar(500) NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CorrectionReason] varchar(300) NULL,
        [CorrectedAt] datetimeoffset NULL,
        [ReplacedByContactId] uniqueidentifier NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Contacts_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923225646_InitialCreate'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Contacts_PatientId_ContactDate] ON [Contacts] ([PatientId], [ContactDate] DESC) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923225646_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Patients_Country_DocumentType_DocumentNumber] ON [Patients] ([Country], [DocumentType], [DocumentNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923225646_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260923225646_InitialCreate', N'8.0.31');
END;
GO

COMMIT;
GO

