CREATE TABLE [dbo].[User] (
    [UserID]       INT          IDENTITY (1, 1) NOT NULL,
    [Username]     VARCHAR (50) NOT NULL,
    [PasswordHash] CHAR (128)   NOT NULL,
    [CreationDate] DATETIME     NOT NULL,
    [Email]        VARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([UserID] ASC),
    CONSTRAINT [UC_Username] UNIQUE NONCLUSTERED ([Username] ASC),
    CONSTRAINT [UC_Email] UNIQUE NONCLUSTERED ([Email] ASC)
);

CREATE TABLE [dbo].[Project] (
    [ProjectID]    INT           IDENTITY (1, 1) NOT NULL,
    [UserID]       INT           NOT NULL,
    [Title]        VARCHAR (255) NOT NULL,
    [CreationDate] DATETIME      NOT NULL,
    PRIMARY KEY CLUSTERED ([ProjectID] ASC),
    CONSTRAINT [FK_Project_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[User] ([UserID])
);

CREATE TABLE [dbo].[Tag] (
    [TagID] INT          IDENTITY (1, 1) NOT NULL,
    [Text]  VARCHAR (30) NOT NULL,
    PRIMARY KEY CLUSTERED ([TagID] ASC),
    CONSTRAINT [UC_Text] UNIQUE NONCLUSTERED ([Text] ASC)
);

CREATE TABLE [dbo].[ProjectTag] (
    [ProjectID] INT NOT NULL,
    [TagID]     INT NOT NULL,
    PRIMARY KEY CLUSTERED ([ProjectID] ASC, [TagID] ASC),
    CONSTRAINT [FK_ProjectTag_Project] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Project] ([ProjectID]),
    CONSTRAINT [FK_ProjectTag_Tag] FOREIGN KEY ([TagID]) REFERENCES [dbo].[Tag] ([TagID])
);

CREATE TABLE [dbo].[Comment] (
    [CommentID]       INT            IDENTITY (1, 1) NOT NULL,
    [ProjectID]       INT            NOT NULL,
    [UserID]          INT            NOT NULL,
    [ParentCommentID] INT            NULL,
    [CreationDate]    DATETIME       NOT NULL,
    [EditedDate]      DATETIME       NULL,
    [BodyText]        VARCHAR (1000) NULL,
    [IsSynthesis]     BIT            NOT NULL,
    [IsContribution]  BIT            NOT NULL,
    [IsSpecification] BIT            NOT NULL,
    PRIMARY KEY CLUSTERED ([CommentID] ASC),
    CONSTRAINT [FK_Comment_ParentComment] FOREIGN KEY ([ParentCommentID]) REFERENCES [dbo].[Comment] ([CommentID]),
    CONSTRAINT [FK_Comment_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[User] ([UserID]),
    CONSTRAINT [FK_Comment_Project] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Project] ([ProjectID])
);

CREATE TABLE [dbo].[Bookmark] (
    [UserID]       INT      NOT NULL,
    [CommentID]    INT      NOT NULL,
    [CreationDate] DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([UserID] ASC, [CommentID] ASC),
    CONSTRAINT [FK_Bookmark_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[User] ([UserID]),
    CONSTRAINT [FK_Bookmark_Comment] FOREIGN KEY ([CommentID]) REFERENCES [dbo].[Comment] ([CommentID])
);

CREATE TABLE [dbo].[SynthesisJunction] (
    [SynthesisCommentID] INT           NOT NULL,
    [LinkedCommentID]    INT           NOT NULL,
    [Subject]            VARCHAR (255) NULL,
    PRIMARY KEY CLUSTERED ([SynthesisCommentID] ASC, [LinkedCommentID] ASC),
    CONSTRAINT [FK_SynthesisJunction_SynthesisComment] FOREIGN KEY ([SynthesisCommentID]) REFERENCES [dbo].[Comment] ([CommentID]),
    CONSTRAINT [FK_SynthesisJunction_LinkedComment] FOREIGN KEY ([LinkedCommentID]) REFERENCES [dbo].[Comment] ([CommentID])
);

CREATE TABLE [dbo].[ContributionFile] (
    [ContributionFileID] INT             IDENTITY (1, 1) NOT NULL,
    [CommentID]          INT             NOT NULL,
    [Filename]           VARCHAR (255)   NOT NULL,
    [Data]               VARBINARY (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([ContributionFileID] ASC),
    CONSTRAINT [FK_ContributionFile_Comment] FOREIGN KEY ([CommentID]) REFERENCES [dbo].[Comment] ([CommentID])
);
