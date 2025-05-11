CREATE TABLE [dbo].[lu_state] (
    [id]    INT          IDENTITY (1, 1) NOT NULL,
    [state] VARCHAR (50) NOT NULL,
    [abrv]  VARCHAR (3)  NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [unique_state] UNIQUE NONCLUSTERED ([state] ASC)
);

