CREATE DATABASE [SpecialtyManagement]
GO

USE [SpecialtyManagement]

CREATE TABLE Teachers(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[Surname] NVARCHAR(50) NOT NULL,
[Name] NVARCHAR(50) NOT NULL,
[Patronymic] NVARCHAR(50))

CREATE TABLE TypesLessons(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[Type] NVARCHAR(5) NOT NULL)

CREATE TABLE Lessons(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[Name] NVARCHAR(100) NOT NULL,
[IdType] INT FOREIGN KEY REFERENCES TypesLessons(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[Code] NVARCHAR(5) NOT NULL)

CREATE TABLE Groups(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[Group] NVARCHAR(10) NOT NULL)

CREATE TABLE DistributionLessons(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[IdTeacher] INT FOREIGN KEY REFERENCES Teachers(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[IdLesson] INT FOREIGN KEY REFERENCES Lessons(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[IdGroup] INT FOREIGN KEY REFERENCES Groups(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL)

CREATE TABLE Students(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[Surname] NVARCHAR(50) NOT NULL,
[Name] NVARCHAR(50) NOT NULL,
[Patronymic] NVARCHAR(50),
[Birthday] DATE NOT NULL,
[IdGroup] INT FOREIGN KEY REFERENCES Groups(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[Note] NVARCHAR(MAX),
[IsExpelled] BIT NOT NULL,
[IsAcademic] BIT NOT NULL)

CREATE TABLE Arrears(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[IdStudent] INT FOREIGN KEY REFERENCES Students(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[StartYear] INT NOT NULL,
[SemesterNumber] INT NOT NULL,
[SemesterSequenceNumber] INT NOT NULL)

CREATE TABLE TypesArrears(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[Type] NVARCHAR(13) NOT NULL)

CREATE TABLE ArrearsLessons(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[IdArrear] INT FOREIGN KEY REFERENCES Arrears(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[IdLesson] INT FOREIGN KEY REFERENCES Lessons(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[IdType] INT FOREIGN KEY REFERENCES TypesArrears(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[IsLiquidated] BIT NOT NULL,
[IsGoodReason] BIT NOT NULL)

CREATE TABLE Specialty(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
Code NVARCHAR(10) NOT NULL,
[Departament] NVARCHAR(100) NOT NULL,
[Name] NVARCHAR(100) NOT NULL,
[Head] NVARCHAR(100) NOT NULL)

INSERT INTO TypesArrears
VALUES ('Первичная'),
('Комиссионная')