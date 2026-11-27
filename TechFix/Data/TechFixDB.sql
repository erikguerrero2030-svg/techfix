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
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FullName] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [BusinessHours] (
        [Id] int NOT NULL IDENTITY,
        [Day] int NOT NULL,
        [OpensAt] time NOT NULL,
        [ClosesAt] time NOT NULL,
        [IsOpen] bit NOT NULL,
        CONSTRAINT [PK_BusinessHours] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [Services] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Price] decimal(10,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [Icon] nvarchar(60) NOT NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [Clients] (
        [UserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_Clients_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [Devices] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] nvarchar(450) NOT NULL,
        [Type] nvarchar(60) NOT NULL,
        [Brand] nvarchar(60) NOT NULL,
        [Model] nvarchar(80) NOT NULL,
        CONSTRAINT [PK_Devices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Devices_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [Technicians] (
        [UserId] nvarchar(450) NOT NULL,
        [Specialty] nvarchar(120) NOT NULL,
        CONSTRAINT [PK_Technicians] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_Technicians_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [Appointments] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] nvarchar(450) NOT NULL,
        [DeviceId] int NOT NULL,
        [ServiceItemId] int NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [Problem] nvarchar(1000) NOT NULL,
        [ScheduledAt] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [TrackingCode] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Appointments_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Appointments_Devices_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Devices] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Appointments_Services_ServiceItemId] FOREIGN KEY ([ServiceItemId]) REFERENCES [Services] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [Repairs] (
        [Id] int NOT NULL IDENTITY,
        [AppointmentId] int NOT NULL,
        [TechnicianId] nvarchar(450) NULL,
        [State] int NOT NULL,
        [ClientApproved] bit NOT NULL,
        [CompletedAt] datetime2 NULL,
        CONSTRAINT [PK_Repairs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Repairs_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Repairs_AspNetUsers_TechnicianId] FOREIGN KEY ([TechnicianId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [Components] (
        [Id] int NOT NULL IDENTITY,
        [RepairId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitCost] decimal(10,2) NOT NULL,
        CONSTRAINT [PK_Components] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Components_Repairs_RepairId] FOREIGN KEY ([RepairId]) REFERENCES [Repairs] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [Diagnoses] (
        [Id] int NOT NULL IDENTITY,
        [RepairId] int NOT NULL,
        [TechnicianId] nvarchar(450) NOT NULL,
        [Findings] nvarchar(2000) NOT NULL,
        [ProposedAction] nvarchar(2000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Diagnoses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Diagnoses_AspNetUsers_TechnicianId] FOREIGN KEY ([TechnicianId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Diagnoses_Repairs_RepairId] FOREIGN KEY ([RepairId]) REFERENCES [Repairs] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [History] (
        [Id] int NOT NULL IDENTITY,
        [RepairId] int NOT NULL,
        [State] int NOT NULL,
        [ChangedAt] datetime2 NOT NULL,
        [ChangedById] nvarchar(450) NOT NULL,
        [Notes] nvarchar(2000) NOT NULL,
        CONSTRAINT [PK_History] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_History_AspNetUsers_ChangedById] FOREIGN KEY ([ChangedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_History_Repairs_RepairId] FOREIGN KEY ([RepairId]) REFERENCES [Repairs] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [RepairId] int NOT NULL,
        [Amount] decimal(10,2) NOT NULL,
        [Method] nvarchar(40) NOT NULL,
        [RegisteredAt] datetime2 NOT NULL,
        [RegisteredById] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_AspNetUsers_RegisteredById] FOREIGN KEY ([RegisteredById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Payments_Repairs_RepairId] FOREIGN KEY ([RepairId]) REFERENCES [Repairs] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE TABLE [Reviews] (
        [Id] int NOT NULL IDENTITY,
        [RepairId] int NOT NULL,
        [ClientId] nvarchar(450) NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Review_Rating] CHECK ([Rating] BETWEEN 1 AND 5),
        CONSTRAINT [FK_Reviews_AspNetUsers_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reviews_Repairs_RepairId] FOREIGN KEY ([RepairId]) REFERENCES [Repairs] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_ClientId] ON [Appointments] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_DeviceId] ON [Appointments] ([DeviceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Appointments_ScheduledAt] ON [Appointments] ([ScheduledAt]) WHERE [Status] <> 2');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_ServiceItemId] ON [Appointments] ([ServiceItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Appointments_TrackingCode] ON [Appointments] ([TrackingCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AspNetUsers_Email] ON [AspNetUsers] ([Email]) WHERE [Email] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BusinessHours_Day] ON [BusinessHours] ([Day]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Components_RepairId] ON [Components] ([RepairId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Devices_ClientId] ON [Devices] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Diagnoses_RepairId] ON [Diagnoses] ([RepairId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Diagnoses_TechnicianId] ON [Diagnoses] ([TechnicianId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_History_ChangedById] ON [History] ([ChangedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_History_RepairId] ON [History] ([RepairId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_RegisteredById] ON [Payments] ([RegisteredById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_RepairId] ON [Payments] ([RepairId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Repairs_AppointmentId] ON [Repairs] ([AppointmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Repairs_TechnicianId] ON [Repairs] ([TechnicianId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reviews_ClientId] ON [Reviews] ([ClientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Reviews_RepairId] ON [Reviews] ([RepairId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261126235210_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261126235210_InitialCreate', N'8.0.14');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127185641_AddSyntheticFlag'
)
BEGIN
    ALTER TABLE [Appointments] ADD [IsSynthetic] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261127185641_AddSyntheticFlag'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261127185641_AddSyntheticFlag', N'8.0.14');
END;
GO

COMMIT;
GO

