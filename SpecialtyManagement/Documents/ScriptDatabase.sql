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

CREATE TABLE TypesGroups(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[Type] NVARCHAR(10) NOT NULL)

CREATE TABLE Groups(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[IdType] INT FOREIGN KEY REFERENCES TypesGroups(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[Group] NVARCHAR(3) NOT NULL)

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
[Note] NVARCHAR(MAX))

CREATE TABLE ExpelledStudents(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[Surname] NVARCHAR(50) NOT NULL,
[Name] NVARCHAR(50) NOT NULL,
[Patronymic] NVARCHAR(50),
[Birthday] DATE NOT NULL,
[IdGroup] INT FOREIGN KEY REFERENCES Groups(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[Note] NVARCHAR(MAX))

CREATE TABLE StudentsPerformance(
[Id] INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
[IdStudent] INT FOREIGN KEY REFERENCES Students(Id) ON UPDATE CASCADE ON DELETE CASCADE NOT NULL,
[NumberOfStudents] INT NOT NULL,
[NumberOfSuccessful] INT NOT NULL,
[NumberOfUnsuccessful] INT NOT NULL)

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

INSERT INTO TypesGroups VALUES
('Бюджет'),
('Внебюджет')

INSERT INTO Groups VALUES
(1, '11П'),
(1, '12П'),
(2, '13П'),
(2, '14П'),
(1, '21П'),
(1, '22В'),
(2, '23П'),
(2, '24В'),
(1, '31П'),
(1, '32В'),
(2, '33П'),
(2, '34В'),
(1, '41П'),
(1, '42В'),
(2, '43П'),
(2, '44В')

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
('Арзамасова', 'Дарья', 'Алексеевна', '2006/12/06', 13, 'пер. Пр. 04-01/1/15 от 16.01.2023 из гр. 11Б'),
('Богаткова', 'Анастасия', 'Андреевна', '2006/12/06', 13, NULL),
('Брусова', 'Полина', 'Андреевна', '2006/07/21', 13, NULL),
('Глушенкова', 'Влада', 'Алексеевна', '2005/11/23', 13, NULL),
('Головачёва', 'Екатерина', 'Михайловна', '2005/02/04', 13, NULL),
('Гусев', 'Илья', 'Александрович', '2006/08/18', 13, NULL),
('Гусенков', 'Никита', 'Алексеевич', '2006/10/12', 13, NULL),
('Дунаева', 'Анастасия', 'Владимировна', '2006/02/14', 13, NULL),
('Ерыкалова', 'Виктория', 'Александровна', '2006/03/02', 13, NULL),
('Замятина', 'Анастасия', 'Сергеевна', '1999/11/06', 13, NULL),
('Карманов', 'Артём', 'Денисович', '2006/08/04', 13, NULL),
('Карпов', 'Дмитрий', 'Александрович', '2006/09/09', 13, NULL),
('Коротков', 'Александр', 'Евгеньевич', '2006/07/30', 13, NULL),
('Кузнецов', 'Семён', 'Сергеевич', '2006/07/04', 13, NULL),
('Кукина', 'Анастасия', 'Игоревна', '2006/08/02', 13, NULL),
('Мирянгин', 'Артемий', 'Русланович', '2007/03/03', 13, NULL),
('Подопледов', 'Евгений', 'Алексеевич', '2006/05/11', 13, NULL),
('Савуков', 'Егор', 'Александрович', '2006/06/03', 13, NULL),
('Селехова', 'Ярослава', 'Викторовна', '2006/03/25', 13, NULL),
('Сибирков', 'Егор', 'Игоревич', '2006/10/05', 13, NULL),
('Ставцева', 'Дарья', 'Андреевна', '2006/07/10', 13, NULL),
('Сучкина', 'Евгения', 'Денисовна', '2006/11/01', 13, NULL),
('Шеронов', 'Денис', 'Сергеевич', '2006/06/16', 13, NULL),
('Шкунов', 'Сергей', 'Аркадьевич', '2006/10/04', 13, NULL),
('Шохрин', 'Дмитрий', 'Андреевич', '2006/03/31', 13, NULL),
('Постовая', 'Мария', 'Павловна', '2002/09/06', 13, 'отч. Пр. 04-01/1/144 от 22.09.2022'),
('Стругова', 'Елизавета', 'Андреевна', '2002/12/04', 13, 'отч. Пр. 04-01/1/144 от 22.09.2022')

INSERT INTO TypesArrears VALUES
('Академическая'),
('Комиссионная')

INSERT INTO Arrears VALUES
(1, 1, 2022, 1),
(2, 1, 2022, 1),
(8, 1, 2022, 1),
(5, 2, 2023, 2),
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