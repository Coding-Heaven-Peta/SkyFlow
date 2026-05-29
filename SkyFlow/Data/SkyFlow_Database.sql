-- =============================================
-- SkyFlow Terminal Manager - Database Setup
-- Run this script in SQL Server Management Studio
-- or via LocalDB before starting the application.
-- =============================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SkyFlowDB')
BEGIN
    CREATE DATABASE SkyFlowDB;
END
GO

USE SkyFlowDB;
GO

-- =============================================
-- TABLE CREATION
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
CREATE TABLE Users (
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role         NVARCHAR(20)  NOT NULL,   -- 'Admin' | 'GateAgent'
    Email        NVARCHAR(100),
    FirstName    NVARCHAR(50),
    LastName     NVARCHAR(50),
    CreatedAt    DATETIME DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Airport')
CREATE TABLE Airport (
    AirportId   INT IDENTITY(1,1) PRIMARY KEY,
    AirportCode NVARCHAR(10)  NOT NULL UNIQUE,
    AirportName NVARCHAR(100),
    City        NVARCHAR(50),
    Country     NVARCHAR(50)
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Aircraft')
CREATE TABLE Aircraft (
    AircraftId         INT IDENTITY(1,1) PRIMARY KEY,
    AircraftType       NVARCHAR(50),
    RegistrationNumber NVARCHAR(20) NOT NULL UNIQUE,
    Manufacturer       NVARCHAR(50),
    Capacity           INT,
    Status             NVARCHAR(20) DEFAULT 'Active'
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Flight')
CREATE TABLE Flight (
    FlightId         INT IDENTITY(1,1) PRIMARY KEY,
    FlightNumber     NVARCHAR(10)  NOT NULL UNIQUE,
    Origin           NVARCHAR(50),
    Destination      NVARCHAR(50),
    DepartureTime    DATETIME,
    ArrivalTime      DATETIME,
    Capacity         INT,
    Status           NVARCHAR(20) DEFAULT 'Scheduled',
    CurrentOccupancy INT DEFAULT 0,
    GateAgentId      INT FOREIGN KEY REFERENCES Users(UserId)
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FlightAssignment')
CREATE TABLE FlightAssignment (
    AssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    FlightId     INT FOREIGN KEY REFERENCES Flight(FlightId),
    AircraftId   INT FOREIGN KEY REFERENCES Aircraft(AircraftId),
    PilotId      INT FOREIGN KEY REFERENCES Users(UserId),
    CoPilotId    INT FOREIGN KEY REFERENCES Users(UserId),
    AssignedDate DATETIME DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Passenger')
CREATE TABLE Passenger (
    PassengerId    INT IDENTITY(1,1) PRIMARY KEY,
    UserId         INT FOREIGN KEY REFERENCES Users(UserId),
    PassportNumber NVARCHAR(20),
    DateOfBirth    DATE,
    Nationality    NVARCHAR(50),
    ContactNumber  NVARCHAR(20)
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Booking')
CREATE TABLE Booking (
    BookingId     INT IDENTITY(1,1) PRIMARY KEY,
    FlightId      INT FOREIGN KEY REFERENCES Flight(FlightId),
    PassengerId   INT FOREIGN KEY REFERENCES Passenger(PassengerId),
    SeatNumber    NVARCHAR(10),
    BookingStatus NVARCHAR(20) DEFAULT 'Booked',  -- Booked | CheckedIn | Boarded | Cancelled
    BookingDate   DATETIME DEFAULT GETDATE(),
    CheckInTime   DATETIME,
    BoardingTime  DATETIME
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Baggage')
CREATE TABLE Baggage (
    BaggageId  INT IDENTITY(1,1) PRIMARY KEY,
    BookingId  INT FOREIGN KEY REFERENCES Booking(BookingId),
    Weight     DECIMAL(5,2),
    BaggageTag NVARCHAR(20),
    Status     NVARCHAR(20) DEFAULT 'Checked'
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Crew')
CREATE TABLE Crew (
    CrewId            INT IDENTITY(1,1) PRIMARY KEY,
    UserId            INT FOREIGN KEY REFERENCES Users(UserId),
    CrewType          NVARCHAR(30),
    LicenseNumber     NVARCHAR(50),
    YearsOfExperience INT,
    Status            NVARCHAR(20) DEFAULT 'Available'
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FlightLog')
CREATE TABLE FlightLog (
    LogId       INT IDENTITY(1,1) PRIMARY KEY,
    FlightId    INT FOREIGN KEY REFERENCES Flight(FlightId),
    Action      NVARCHAR(50),
    PerformedBy INT FOREIGN KEY REFERENCES Users(UserId),
    PerformedAt DATETIME DEFAULT GETDATE(),
    Details     NVARCHAR(500)
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notification')
CREATE TABLE Notification (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId         INT FOREIGN KEY REFERENCES Users(UserId),
    Message        NVARCHAR(500),
    Type           NVARCHAR(30),
    IsRead         BIT DEFAULT 0,
    CreatedAt      DATETIME DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLog')
CREATE TABLE AuditLog (
    AuditId   INT IDENTITY(1,1) PRIMARY KEY,
    UserId    INT FOREIGN KEY REFERENCES Users(UserId),
    Action    NVARCHAR(100),
    TableName NVARCHAR(50),
    RecordId  INT,
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    Timestamp DATETIME DEFAULT GETDATE()
);
GO

-- =============================================
-- SEED DATA
-- =============================================

-- Users (password hash = BCrypt of 'Admin@123' and 'Agent@123')
-- For demo, we use plain SHA256-style placeholders;
-- the application compares using simple hash method.
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
INSERT INTO Users (Username, PasswordHash, Role, Email, FirstName, LastName)
VALUES
  ('admin',      'admin123',  'Admin',     'admin@skyflow.com',   'Sky',    'Admin'),
  ('agent01',    'agent123',  'GateAgent', 'agent01@skyflow.com', 'Thabo',  'Mokoena'),
  ('agent02',    'agent123',  'GateAgent', 'agent02@skyflow.com', 'Priya',  'Naidoo');
GO

-- Airports
IF NOT EXISTS (SELECT * FROM Airport WHERE AirportCode = 'JNB')
INSERT INTO Airport (AirportCode, AirportName, City, Country)
VALUES
  ('JNB', 'O.R. Tambo International',    'Johannesburg', 'South Africa'),
  ('CPT', 'Cape Town International',     'Cape Town',    'South Africa'),
  ('DUR', 'King Shaka International',    'Durban',       'South Africa'),
  ('PLZ', 'Chief Dawid Stuurman Intl',   'Port Elizabeth','South Africa'),
  ('LHR', 'Heathrow Airport',            'London',       'United Kingdom');
GO

-- Aircraft
IF NOT EXISTS (SELECT * FROM Aircraft WHERE RegistrationNumber = 'ZS-SNA')
INSERT INTO Aircraft (AircraftType, RegistrationNumber, Manufacturer, Capacity, Status)
VALUES
  ('Boeing 737-800',  'ZS-SNA', 'Boeing',  189, 'Active'),
  ('Airbus A320',     'ZS-GAA', 'Airbus',  180, 'Active'),
  ('Boeing 787-9',    'ZS-BDA', 'Boeing',  296, 'Active');
GO

-- Flights
IF NOT EXISTS (SELECT * FROM Flight WHERE FlightNumber = 'SF101')
INSERT INTO Flight (FlightNumber, Origin, Destination, DepartureTime, ArrivalTime, Capacity, Status, CurrentOccupancy, GateAgentId)
VALUES
  ('SF101', 'JNB', 'CPT', DATEADD(HOUR, 2,  GETDATE()), DATEADD(HOUR, 4,  GETDATE()), 180, 'Scheduled',  3, 2),
  ('SF202', 'CPT', 'DUR', DATEADD(HOUR, 5,  GETDATE()), DATEADD(HOUR, 7,  GETDATE()), 189, 'Boarding',   2, 2),
  ('SF303', 'DUR', 'JNB', DATEADD(HOUR, 8,  GETDATE()), DATEADD(HOUR, 10, GETDATE()), 180, 'Scheduled',  1, 3),
  ('SF404', 'JNB', 'LHR', DATEADD(HOUR, 12, GETDATE()), DATEADD(HOUR, 23, GETDATE()), 296, 'Scheduled',  0, 3);
GO

-- Passengers (linked to no user account — standalone travellers)
IF NOT EXISTS (SELECT * FROM Passenger WHERE PassportNumber = 'SA001234')
INSERT INTO Passenger (UserId, PassportNumber, DateOfBirth, Nationality, ContactNumber)
VALUES
  (NULL, 'SA001234', '1990-03-15', 'South African', '0821234567'),
  (NULL, 'SA005678', '1985-07-22', 'South African', '0839876543'),
  (NULL, 'UK009876', '1978-11-05', 'British',        '0711122334'),
  (NULL, 'SA003344', '1995-01-30', 'South African', '0601234567'),
  (NULL, 'ZW007788', '1982-09-18', 'Zimbabwean',    '0761234567'),
  (NULL, 'SA002211', '2000-06-12', 'South African', '0831234567');
GO

-- Bookings
IF NOT EXISTS (SELECT * FROM Booking WHERE SeatNumber = '12A' AND FlightId = 1)
INSERT INTO Booking (FlightId, PassengerId, SeatNumber, BookingStatus, BookingDate)
VALUES
  (1, 1, '12A', 'CheckedIn', GETDATE()),
  (1, 2, '14B', 'Booked',    GETDATE()),
  (1, 3, '22C', 'Booked',    GETDATE()),
  (2, 4, '5D',  'CheckedIn', GETDATE()),
  (2, 5, '6E',  'Booked',    GETDATE()),
  (3, 6, '1A',  'Booked',    GETDATE());
GO

-- Baggage
IF NOT EXISTS (SELECT * FROM Baggage WHERE BaggageTag = 'BAG-00001')
INSERT INTO Baggage (BookingId, Weight, BaggageTag, Status)
VALUES
  (1, 23.5, 'BAG-00001', 'Checked'),
  (2, 18.0, 'BAG-00002', 'Checked'),
  (4, 20.0, 'BAG-00003', 'Checked');
GO

PRINT 'SkyFlowDB setup and seed complete.';
GO
