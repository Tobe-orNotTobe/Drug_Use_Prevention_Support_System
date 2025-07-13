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

INSERT INTO Roles (RoleName) VALUES
(N'Guest'),
(N'Member'),
(N'Staff'),
(N'Consultant'),
(N'Manager'),
(N'Admin');

INSERT INTO Users (Email, PasswordHash, FullName, DateOfBirth, Gender, Phone, Address)
VALUES
(N'admin@DrugPreventionSystem.org', N'@@abc123@@', N'Admin', '1980-05-01', N'Nam', N'0912345678', N'Hà Nội'),
(N'lethianh@gmail.com', N'123456', N'Lê Thị Anh', '2002-09-15', N'Nữ', N'0987654321', N'Đà Nẵng'),
(N'nguyenvanbinh@gmail.com', N'123456', N'Nguyễn Văn Bình', '1995-11-20', N'Nam', N'0934567890', N'Hồ Chí Minh'),
(N'hoa.nguyen@dups.org', N'123456', N'Nguyễn Thị Hoa', '1985-03-15', N'Nữ', N'0901234567', N'Hà Nội'),
(N'long.tran@dups.org', N'123456', N'Trần Văn Long', '1978-07-22', N'Nam', N'0912345678', N'Hồ Chí Minh'),
(N'mai.pham@dups.org', N'123456', N'Phạm Thị Mai', '1990-12-01', N'Nữ', N'0923456789', N'Đà Nẵng'),
(N'dung.le@dups.org', N'123456', N'Lê Quốc Dũng', '1982-05-10', N'Nam', N'0934567890', N'Cần Thơ'),
(N'lan.vo@dups.org', N'123456', N'Võ Thị Lan', '1988-08-08', N'Nữ', N'0945678901', N'Bình Dương');

INSERT INTO UserRoles (UserId, RoleId) VALUES
(1, 6), 
(2, 2),
(3, 4), 
(4, 4),
(5, 4),
(6, 4),
(7, 4),
(8, 4);

INSERT INTO Consultants (UserId, Qualification, Expertise, WorkSchedule, Bio)
VALUES
(3, N'Thạc sĩ Tâm lý học', N'Tư vấn phòng chống ma túy', N'Thứ 2 - Thứ 6: 8h - 17h', N'Tôi có 10 năm kinh nghiệm tư vấn cho thanh thiếu niên.'),
(4, N'Thạc sĩ Tâm lý học', N'Tư vấn phòng chống ma túy', N'T2-T6: 8h-17h', N'10 năm kinh nghiệm hỗ trợ học sinh, sinh viên.'),
(5, N'Cử nhân Công tác xã hội', N'Tư vấn cộng đồng', N'T3-T7: 9h-16h', N'Tôi luôn sẵn sàng lắng nghe và hỗ trợ các vấn đề xã hội.'),
(6, N'Thạc sĩ Y tế cộng đồng', N'Phòng chống nghiện ma túy', N'T2-T6: 7h30-16h30', N'Đam mê giúp cộng đồng hiểu và phòng tránh ma túy.'),
(7, N'Cử nhân Luật', N'Tư vấn pháp luật liên quan ma túy', N'T2-T6: 8h30-17h', N'Tôi mong muốn giúp mọi người hiểu rõ pháp luật để tránh rủi ro liên quan ma túy.'),
(8, N'Thạc sĩ Tâm lý giáo dục', N'Tư vấn học đường', N'T3-T7: 8h-17h', N'Luôn quan tâm đến sức khỏe tinh thần của học sinh.');

INSERT INTO Courses (Title, Description, TargetAudience, DurationMinutes)
VALUES
(N'Phòng Chống Ma Túy Cho Học Sinh', N'Khóa học giúp học sinh nhận biết tác hại của ma túy.', N'Học sinh', 60),
(N'Kỹ Năng Từ Chối Ma Túy Cho Phụ Huynh', N'Hướng dẫn phụ huynh cách giáo dục con cái tránh xa ma túy.', N'Phụ huynh', 90);

INSERT INTO UserCourses (UserId, CourseId)
VALUES (2, 1);

INSERT INTO Surveys (Name, Description)
VALUES
(N'Khảo Sát ASSIST', N'Khảo sát đo lường nguy cơ sử dụng ma túy.'),
(N'Khảo Sát CRAFFT', N'Khảo sát đánh giá hành vi nguy cơ ở thanh thiếu niên.'),
(N'Khảo Sát Phòng Chống Ma Túy Cho Học Sinh', N'Truy vấn kiến thức và trải nghiệm của học sinh về ma túy.'),
(N'Khảo Sát Phòng Chống Ma Túy Cho Phụ Huynh', N'Tìm hiểu mức độ hiểu biết của phụ huynh về phòng chống ma túy.'),
(N'Khảo Sát Nhận Thức Về Phòng Chống Ma Túy Trong Cộng Đồng', N'Khảo sát mức độ hiểu biết của cộng đồng về ma túy.');

INSERT INTO SurveyQuestions (SurveyId, QuestionText, QuestionType)
VALUES
(1, N'Bạn có từng thử sử dụng chất kích thích chưa?', N'SingleChoice'),
(2, N'Bạn có cảm thấy bị ép buộc dùng ma túy không?', N'SingleChoice'),

(3, N'Bạn có biết các loại ma túy phổ biến không?', N'SingleChoice'),
(3, N'Bạn có từng được giáo viên nói về tác hại của ma túy không?', N'SingleChoice'),
(3, N'Bạn sẽ làm gì khi có người rủ bạn thử ma túy?', N'SingleChoice'),

(4, N'Bạn có biết cách nhận diện con mình có dấu hiệu dùng ma túy không?', N'SingleChoice'),
(4, N'Bạn có nghĩ nên nói chuyện trực tiếp với con về ma túy không?', N'SingleChoice'),

(5, N'Bạn nghĩ ma túy chỉ ảnh hưởng đến người dùng không?', N'SingleChoice'),
(5, N'Theo bạn cộng đồng có vai trò gì trong phòng chống ma túy?', N'SingleChoice');

INSERT INTO SurveyOptions (QuestionId, OptionText, Score)
VALUES
(1, N'Có', 1),
(1, N'Không', 0),
(2, N'Có', 1),
(2, N'Không', 0),

(3, N'Biết', 1),
(3, N'Không biết', 0),
(4, N'Có', 1),
(4, N'Không', 0),
(5, N'Từ chối', 1),
(5, N'Thử cho biết', 0),
(6, N'Có', 1),
(6, N'Không', 0),
(7, N'Nên', 1),
(7, N'Không cần', 0),
(8, N'Đúng', 0),
(8, N'Sai', 1),
(9, N'Rất quan trọng', 1),
(9, N'Không quan trọng', 0);

INSERT INTO UserSurveyResults (UserId, SurveyId, TotalScore, Recommendation)
VALUES (2, 1, 2, N'Nên gặp tư vấn viên.');

INSERT INTO UserSurveyResults (UserId, SurveyId, TotalScore, Recommendation)
VALUES (2, 1, 2, N'Nên gặp tư vấn viên.');

INSERT INTO Appointments (UserId, ConsultantId, AppointmentDate, DurationMinutes, Status, Notes)
VALUES (2, 1, DATEADD(DAY, 1, GETDATE()), 60, N'Pending', N'Khách hàng cần tư vấn về con nghiện ma túy.');

