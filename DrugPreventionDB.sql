USE [master]
GO

CREATE DATABASE [DrugPreventionDB]
GO

USE [DrugPreventionDB]
GO

CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(512) NOT NULL,
    FullName NVARCHAR(255) NOT NULL,
    DateOfBirth DATE NULL,
    Gender NVARCHAR(20) NULL,
    Phone NVARCHAR(50) NULL,
    Address NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

CREATE TABLE Courses (
    CourseId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    TargetAudience NVARCHAR(50) NULL,
    DurationMinutes INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE UserCourses (
    UserCourseId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    CourseId INT NOT NULL,
    RegisteredAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CompletedAt DATETIME2 NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);

CREATE TABLE Surveys (
    SurveyId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

CREATE TABLE SurveyQuestions (
    QuestionId INT IDENTITY(1,1) PRIMARY KEY,
    SurveyId INT NOT NULL,
    QuestionText NVARCHAR(MAX) NOT NULL,
    QuestionType NVARCHAR(50) NOT NULL,
    FOREIGN KEY (SurveyId) REFERENCES Surveys(SurveyId)
);

CREATE TABLE SurveyOptions (
    OptionId INT IDENTITY(1,1) PRIMARY KEY,
    QuestionId INT NOT NULL,
    OptionText NVARCHAR(MAX) NOT NULL,
    Score INT NULL,
    FOREIGN KEY (QuestionId) REFERENCES SurveyQuestions(QuestionId)
);

CREATE TABLE UserSurveyResults (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    SurveyId INT NOT NULL,
    TakenAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    TotalScore INT NULL,
    Recommendation NVARCHAR(MAX) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (SurveyId) REFERENCES Surveys(SurveyId)
);

CREATE TABLE UserSurveyAnswers (
    AnswerId INT IDENTITY(1,1) PRIMARY KEY,
    ResultId INT NOT NULL,
    QuestionId INT NOT NULL,
    OptionId INT NULL,
    AnswerText NVARCHAR(MAX) NULL,
    FOREIGN KEY (ResultId) REFERENCES UserSurveyResults(ResultId),
    FOREIGN KEY (QuestionId) REFERENCES SurveyQuestions(QuestionId),
    FOREIGN KEY (OptionId) REFERENCES SurveyOptions(OptionId)
);

CREATE TABLE Consultants (
    ConsultantId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Qualification NVARCHAR(255) NULL,
    Expertise NVARCHAR(255) NULL,
    WorkSchedule NVARCHAR(MAX) NULL,
    Bio NVARCHAR(MAX) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE Appointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ConsultantId INT NOT NULL,
    AppointmentDate DATETIME2 NOT NULL,
    DurationMinutes INT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Pending',
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (ConsultantId) REFERENCES Consultants(ConsultantId)
);

CREATE TABLE CommunicationPrograms (
    ProgramId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    StartDate DATETIME2 NULL,
    EndDate DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE UserPrograms (
    UserProgramId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProgramId INT NOT NULL,
    JoinedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (ProgramId) REFERENCES CommunicationPrograms(ProgramId)
);

CREATE TABLE ProgramSurveys (
    ProgramSurveyId INT IDENTITY(1,1) PRIMARY KEY,
    ProgramId INT NOT NULL,
    SurveyId INT NOT NULL,
    SurveyType NVARCHAR(50) NOT NULL,
    FOREIGN KEY (ProgramId) REFERENCES CommunicationPrograms(ProgramId),
    FOREIGN KEY (SurveyId) REFERENCES Surveys(SurveyId)
);

CREATE TABLE AuditLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(255) NOT NULL,
    TableName NVARCHAR(255) NULL,
    RecordId INT NULL,
    LogDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Details NVARCHAR(MAX) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

INSERT INTO Roles (RoleName) VALUES
(N'Guest'),
(N'Member'),
(N'Staff'),
(N'Consultant'),
(N'Manager'),
(N'Admin');

INSERT INTO Users (Email, PasswordHash, FullName, DateOfBirth, Gender, Phone, Address)
VALUES
(N'admin@funewsmanagementsystem.org', N'@@abc123@@', N'Quản Trị Viên', '1980-05-01', N'Nam', N'0912345678', N'Hà Nội'),
(N'lethianh@gmail.com', N'123456', N'Lê Thị Anh', '2002-09-15', N'Nữ', N'0987654321', N'Đà Nẵng'),
(N'nguyenvanbinh@gmail.com', N'123456', N'Nguyễn Văn Bình', '1995-11-20', N'Nam', N'0934567890', N'Hồ Chí Minh');

INSERT INTO UserRoles (UserId, RoleId) VALUES
(1, 6), -- admin
(2, 2), -- member
(3, 4); -- consultant

INSERT INTO Courses (Title, Description, TargetAudience, DurationMinutes)
VALUES
(N'Phòng Chống Ma Túy Cho Học Sinh', N'Khóa học giúp học sinh nhận biết tác hại của ma túy.', N'Học sinh', 60),
(N'Kỹ Năng Từ Chối Ma Túy Cho Phụ Huynh', N'Hướng dẫn phụ huynh cách giáo dục con cái tránh xa ma túy.', N'Phụ huynh', 90);

INSERT INTO UserCourses (UserId, CourseId)
VALUES
(2, 1);

INSERT INTO Surveys (Name, Description)
VALUES
(N'Khảo Sát ASSIST', N'Khảo sát đo lường nguy cơ sử dụng ma túy.'),
(N'Khảo Sát CRAFFT', N'Khảo sát đánh giá hành vi nguy cơ ở thanh thiếu niên.');

INSERT INTO SurveyQuestions (SurveyId, QuestionText, QuestionType)
VALUES
(1, N'Bạn có từng thử sử dụng chất kích thích chưa?', N'SingleChoice'),
(2, N'Bạn có cảm thấy bị ép buộc dùng ma túy không?', N'SingleChoice');

INSERT INTO SurveyOptions (QuestionId, OptionText, Score)
VALUES
(1, N'Có', 1),
(1, N'Không', 0),
(2, N'Có', 1),
(2, N'Không', 0);

INSERT INTO UserSurveyResults (UserId, SurveyId, TotalScore, Recommendation)
VALUES
(2, 1, 2, N'Nên gặp tư vấn viên.');

INSERT INTO UserSurveyAnswers (ResultId, QuestionId, OptionId)
VALUES
(1, 1, 1);

INSERT INTO Consultants (UserId, Qualification, Expertise, WorkSchedule, Bio)
VALUES
(3, N'Thạc sĩ Tâm lý học', N'Tư vấn phòng chống ma túy', N'Thứ 2 - Thứ 6: 8h - 17h', N'Tôi có 10 năm kinh nghiệm tư vấn cho thanh thiếu niên.');

INSERT INTO Appointments (UserId, ConsultantId, AppointmentDate, DurationMinutes, Status, Notes)
VALUES
(2, 1, DATEADD(DAY, 1, GETDATE()), 60, N'Pending', N'Khách hàng cần tư vấn về con nghiện ma túy.');

INSERT INTO CommunicationPrograms (Title, Description, StartDate, EndDate)
VALUES
(N'Chương Trình Truyền Thông Phòng Chống Ma Túy', N'Tuyên truyền kiến thức phòng chống ma túy trong cộng đồng.', '2025-07-01', '2025-07-10');

INSERT INTO UserPrograms (UserId, ProgramId)
VALUES
(2, 1);

INSERT INTO ProgramSurveys (ProgramId, SurveyId, SurveyType)
VALUES
(1, 1, N'Pre');

INSERT INTO AuditLogs (UserId, Action, TableName, RecordId, Details)
VALUES
(2, N'INSERT', N'Users', 2, N'Tạo mới user Lê Thị Anh');
