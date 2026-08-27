CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

CREATE TABLE [User] (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    PhoneNumber VARCHAR(20)
);

CREATE TABLE EventType (
    EventTypeID INT IDENTITY(1,1) PRIMARY KEY,
    TypeName VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventTypeID INT NOT NULL,
    EventName VARCHAR(100) NOT NULL,
    Description VARCHAR(255),
    EventDate DATETIME NOT NULL,
    Location VARCHAR(150) NOT NULL,
    Distance DECIMAL(10,2),

    FOREIGN KEY (OrganiserID) REFERENCES [User](UserID),
    FOREIGN KEY (EventTypeID) REFERENCES EventType(EventTypeID)
);

CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    Description VARCHAR(255),

    FOREIGN KEY (EventID) REFERENCES Event(EventID)
);

CREATE TABLE Enrolment (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL,

    FOREIGN KEY (ParticipantID) REFERENCES [User](UserID),
    FOREIGN KEY (EventID) REFERENCES Event(EventID),
    FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);

CREATE TABLE Result (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME,
    FinishingPosition INT,

    FOREIGN KEY (EnrolmentID) REFERENCES Enrolment(EnrolmentID)
);

-- Seed data for EventType

INSERT INTO EventType (TypeName)
VALUES
('Run'),
('Walk'),
('Cycle');

-- Seed data for Users

INSERT INTO [User] (FirstName, LastName, Email, PasswordHash, Role, PhoneNumber)
VALUES
('Thabo', 'Mokoena', 'thabo@example.com', 'hashedpassword1', 'Organiser', '0712345678'),
('Lerato', 'Nkosi', 'lerato@example.com', 'hashedpassword2', 'Organiser', '0723456789'),
('Sipho', 'Dlamini', 'sipho@example.com', 'hashedpassword3', 'Participant', '0734567890'),
('Naledi', 'Khumalo', 'naledi@example.com', 'hashedpassword4', 'Participant', '0745678901');

-- Seed data for Events

INSERT INTO Event (OrganiserID, EventTypeID, EventName, Description, EventDate, Location, Distance)
VALUES
(1, 1, 'Johannesburg City Run', 'Annual city running event', '2026-10-10 08:00:00', 'Johannesburg', 10.00),
(2, 2, 'Pretoria Fun Walk', 'Community walking event', '2026-11-15 07:30:00', 'Pretoria', 5.00),
(1, 3, 'Soweto Cycle Challenge', 'Community cycling event', '2026-12-05 06:30:00', 'Soweto', 25.00);

-- Seed data for Categories

INSERT INTO Category (EventID, CategoryName, Description)
VALUES
(1, 'Open 10km', 'Open category for the 10km run'),
(1, 'Veteran 10km', 'Veteran category for the 10km run'),
(2, 'Open 5km', 'Open category for the 5km walk'),
(3, 'Open 25km', 'Open category for the 25km cycle');

-- Seed data for Enrolments

INSERT INTO Enrolment (ParticipantID, EventID, CategoryID, EnrolmentDate)
VALUES
(3, 1, 1, '2026-08-20 10:00:00'),
(4, 1, 2, '2026-08-21 11:30:00'),
(3, 2, 3, '2026-08-22 09:15:00'),
(4, 3, 4, '2026-08-23 14:00:00');

-- Seed data for Results

INSERT INTO Result (EnrolmentID, FinishTime, FinishingPosition)
VALUES
(1, '00:48:35', 1),
(2, '00:55:20', 2),
(3, '01:05:10', 1),
(4, '01:32:45', 1);