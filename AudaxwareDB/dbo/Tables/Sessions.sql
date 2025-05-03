CREATE TABLE [dbo].[Sessions] (
    [SessionId]       VARCHAR (80)  NOT NULL,
    [ApplicationName] VARCHAR (255) NOT NULL,
    [Created]         DATETIME2 (7) NOT NULL,
    [Expires]         DATETIME2 (7) NOT NULL,
    [Timeout]         INT           NOT NULL,
    [Locked]          BIT           NOT NULL,
    [LockId]          INT           NOT NULL,
    [LockDate]        DATETIME2 (7) NOT NULL,
    [Data]            TEXT          NULL,
    [Flags]           INT           NOT NULL,
    CONSTRAINT [sessions_pkey] PRIMARY KEY CLUSTERED ([SessionId] ASC, [ApplicationName] ASC)
);

