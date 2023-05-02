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
[Type] NVARCHAR(3) NOT NULL)

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
[IsExpelled] BIT)

CREATE TABLE TeachersSchedule(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[IdTeacher] INT FOREIGN KEY REFERENCES Teachers(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[Date] DATETIME NOT NULL,
[AudienceNumber] NVARCHAR(15) NOT NULL,
[StartYear] INT NOT NULL,
[SemesterNumber] INT NOT NULL)

CREATE TABLE TypesArrears(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[Type] NVARCHAR(13) NOT NULL)

CREATE TABLE Arrears(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[IdStudent] INT FOREIGN KEY REFERENCES Students(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[IdType] INT FOREIGN KEY REFERENCES TypesArrears(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[StartYear] INT NOT NULL,
[SemesterNumber] INT NOT NULL)

CREATE TABLE ArrearsLessons(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[IdArrear] INT FOREIGN KEY REFERENCES Arrears(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[IdLesson] INT FOREIGN KEY REFERENCES Lessons(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[Date] DATETIME)

CREATE TABLE LiquidationsCompositions(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[IdArrearLesson] INT FOREIGN KEY REFERENCES ArrearsLessons(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[IdTeacher] INT FOREIGN KEY REFERENCES Teachers(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL)

CREATE TABLE Specialty(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
Code NVARCHAR(10) NOT NULL,
[Name] NVARCHAR(100) NOT NULL,
[Head] NVARCHAR(100) NOT NULL)

INSERT INTO Specialty VALUES
('09.02.07','Информационные системы и программирование', 'Крылова Лариса Ивановна')

INSERT INTO Groups VALUES
('11П'),
('12П'),
('13П'),
('14П'),
('21П'),
('22В'),
('23П'),
('24В'),
('31П'),
('32В'),
('33П'),
('34В'),
('41П'),
('42В'),
('43П'),
('44В')

INSERT INTO Teachers VALUES
('Крылова', 'Лариса', 'Ивановна'),
('Мухина', 'Людмила', 'Васильевна'),
('Мухин', 'Николай', 'Александрович'),
('Голубева', 'Елизавета', 'Павловна'),
('Мамшева', 'Юлия', 'Сергеевна'),
('Васильева', 'Полина', 'Александровна'),
('Авакян', 'Варта', 'Акоповна'),
('Циркова', 'Виктория', 'Витальевна'),
('Полетаева', 'Марина', 'Викторовна'),
('Стерлядева', 'Ольга', 'Викторовна'),
('Муреева', 'Ольга', 'Николевана'),
('Железнова', 'Алина', 'Викторовна'),
('Разенко', 'Александра', 'Дмитриевна'),
('Авдеева', 'Анна', 'Константиновна'),
('Зубарева', 'Екатерина', 'Дмитриевна'),
('Дубровская', 'Светлана', 'Владимировна'),
('Герасимова', 'Ирина', 'Владимировна'),
('Ботезат', 'Ирина', 'Владимировна'),
('Александрова', 'Наталья', 'Серафимовна'),
('Звягин', 'Илья', 'Дмитриевич'),
('Колодин', 'Юрий', 'Иванович'),
('Романова', 'Полина', 'Сергеевна'),
('Борышнева', 'Наталья', 'Николаевна'),
('Орешкова', 'Мария', 'Сергеевна'),
('Доброхотова', 'Татьяна', 'Викторовна'),
('Канакова', 'Анна', 'Евгеньевна'),
('Маланова', 'Анна', 'Петровна'),
('Сатаева', 'Татьяна', 'Ивановна'),
('Ракова', 'Наталья', 'Николаевна'),
('Булдакова', 'Галина', 'Владимировна'),
('Овчинникова', 'Ольга', 'Юрьевна')

INSERT INTO TypesLessons VALUES
('МДК'),
('ОП'),
('УП'),
('ПП'),
('ПДП')

INSERT INTO Lessons VALUES
('Разработка программных модулей', 1, '01.01'),
('Тестирование программных модулей', 1, '01.02'),
('Разработка мобильных приложений', 1, '01.03'),
('Системное программирование', 1, '01.04'),
('Технология разработки ПО', 1, '02.01'),
('Инструментальные средства разработки ПО', 1, '02.02'),
('Математическое моделирование', 1, '02.03'),
('Внедрение и поддержка компьютерных систем', 1, '04.01'),
('Обеспечение качества функционирования компьютерных систем', 1, '04.02'),
('Технология разработки и защиты баз данных', 1, '11.01'),
('Конфигурирование и программирование в среде 1С', 1, '11.02'),
('Экономика отрасли', 2, '05'),
('Правовое обеспечение профессиональной деятельности', 2, '08'),
('Английский язык', 2, 'нет'),
('Основы философии', 2, 'нет'),
('Физическая культура', 2, 'нет')

INSERT INTO DistributionLessons VALUES
(2, 1, 13),
(4, 2, 13),
(5, 3, 13),
(3, 4, 13),
(15, 12, 13),
(14, 13, 13),
(26, 14, 13),
(16, 15, 13),
(18, 16, 13)

INSERT INTO Students VALUES
('Арзамасова', 'Дарья', 'Алексеевна', '2006/12/06', 13, 'пер. Пр. 04-01/1/15 от 16.01.2023 из гр. 11Б', 0),
('Богаткова', 'Анастасия', 'Андреевна', '2006/12/06', 13, NULL, 0),
('Брусова', 'Полина', 'Андреевна', '2006/07/21', 13, NULL, 0),
('Глушенкова', 'Влада', 'Алексеевна', '2005/11/23', 13, NULL),
('Головачёва', 'Екатерина', 'Михайловна', '2005/02/04', 13, NULL, 0),
('Гусев', 'Илья', 'Александрович', '2006/08/18', 13, NULL, 0),
('Гусенков', 'Никита', 'Алексеевич', '2006/10/12', 13, NULL, 0),
('Дунаева', 'Анастасия', 'Владимировна', '2006/02/14', 13, NULL, 0),
('Ерыкалова', 'Виктория', 'Александровна', '2006/03/02', 13, NULL, 0),
('Замятина', 'Анастасия', 'Сергеевна', '1999/11/06', 13, NULL, 0),
('Карманов', 'Артём', 'Денисович', '2006/08/04', 13, NULL, 0),
('Карпов', 'Дмитрий', 'Александрович', '2006/09/09', 13, NULL, 0),
('Коротков', 'Александр', 'Евгеньевич', '2006/07/30', 13, NULL, 0),
('Кузнецов', 'Семён', 'Сергеевич', '2006/07/04', 13, NULL, 0),
('Кукина', 'Анастасия', 'Игоревна', '2006/08/02', 13, NULL, 0),
('Мирянгин', 'Артемий', 'Русланович', '2007/03/03', 13, NULL, 0),
('Подопледов', 'Евгений', 'Алексеевич', '2006/05/11', 13, NULL, 0),
('Савуков', 'Егор', 'Александрович', '2006/06/03', 13, NULL, 0),
('Селехова', 'Ярослава', 'Викторовна', '2006/03/25', 13, NULL, 0),
('Сибирков', 'Егор', 'Игоревич', '2006/10/05', 13, NULL, 0),
('Ставцева', 'Дарья', 'Андреевна', '2006/07/10', 13, NULL, 0),
('Сучкина', 'Евгения', 'Денисовна', '2006/11/01', 13, NULL, 0),
('Шеронов', 'Денис', 'Сергеевич', '2006/06/16', 13, NULL, 0),
('Шкунов', 'Сергей', 'Аркадьевич', '2006/10/04', 13, NULL, 0),
('Шохрин', 'Дмитрий', 'Андреевич', '2006/03/31', 13, NULL, 0),
('Постовая', 'Мария', 'Павловна', '2002/09/06', 13, 'отч. Пр. 04-01/1/144 от 22.09.2022', 1),
('Стругова', 'Елизавета', 'Андреевна', '2002/12/04', 13, 'отч. Пр. 04-01/1/144 от 22.09.2022', 1)

INSERT INTO TypesArrears VALUES
('Первичная'),
('Комиссионная')

INSERT INTO Arrears VALUES
(1, 1, 2022, 1),
(2, 1, 2022, 1),
(8, 1, 2022, 1),
(5, 1, 2023, 2),
(5, 2, 2023, 2),
(12, 1, 2023, 2)

INSERT INTO ArrearsLessons VALUES
(1, 1, NULL),
(1, 3, NULL),
(2, 12, NULL),
(3, 15, NULL),
(3, 16, NULL),
(3, 13, NULL),
(4, 4, NULL),
(5, 2, NULL),
(5, 2, NULL),
(5, 3, NULL),
(5, 15, NULL),
(5, 16, NULL),
(6, 12, NULL)