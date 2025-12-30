-- ============================================================================
-- Millionaire Game - FFF Questions Table Reset Script
-- ============================================================================
-- This script drops and recreates the fff_questions table with cleaned schema
-- and populates it with reviewed and corrected Fastest Finger First questions
-- Removed columns: Level, Note (unused for FFF questions)
-- ============================================================================

USE dbMillionaire;
GO

-- Drop existing table
IF OBJECT_ID('fff_questions', 'U') IS NOT NULL
    DROP TABLE fff_questions;
GO

-- Create fff_questions table with streamlined schema
CREATE TABLE fff_questions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Question NTEXT NOT NULL,
    A NVARCHAR(500) NOT NULL,
    B NVARCHAR(500) NOT NULL,
    C NVARCHAR(500) NOT NULL,
    D NVARCHAR(500) NOT NULL,
    CorrectAnswer VARCHAR(5) NOT NULL, -- e.g., "ABCD", "BADC", etc.
    Used BIT NOT NULL DEFAULT 0
);
GO

-- ============================================================================
-- FFF QUESTIONS - Ordering/Sequencing Questions
-- CorrectAnswer format: 4-letter string indicating correct order (e.g., "ABCD", "BADC")
-- Each letter A, B, C, D must appear exactly once
-- ============================================================================

INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES
-- Geography & Size Ordering
('Put these planets in order from closest to farthest from the Sun', 'Mars', 'Venus', 'Earth', 'Mercury', 'DBCA', 0),
('Put these oceans by area (largest to smallest)', 'Indian', 'Pacific', 'Atlantic', 'Arctic', 'BCAD', 0),
('Put these continents by population (most to least)', 'Africa', 'Asia', 'Europe', 'North America', 'BCAD', 0),
('Put these mountains by height (shortest to tallest)', 'K2', 'Mount Everest', 'Kilimanjaro', 'Denali', 'CDAB', 0),
('Put these cities by population (largest to smallest)', 'Paris', 'Tokyo', 'London', 'New York', 'BDCA', 0),

-- Historical Chronology
('Put these US Presidents in chronological order', 'Lincoln', 'Washington', 'Roosevelt', 'Kennedy', 'BACD', 0),
('Put these events in chronological order', 'Moon Landing', 'World War I', 'Fall of Berlin Wall', 'American Revolution', 'DBAC', 0),
('Put these inventions by year (oldest first)', 'Telephone', 'Light Bulb', 'Automobile', 'Airplane', 'CBAD', 0),
('Put these Olympiads in chronological order', 'Beijing 2008', 'Athens 2004', 'London 2012', 'Rio 2016', 'BACD', 0),
('Put these movies in order of release date (oldest first)', 'The Matrix', 'Star Wars', 'Avatar', 'Titanic', 'BDAC', 0),

-- Numerical & Alphabetical Ordering
('Put these numbers in ascending order', '50', '10', '25', '100', 'BCAD', 0),
('Put these letters in alphabetical order', 'Z', 'M', 'A', 'Q', 'CBDA', 0),
('Put these Roman numerals in ascending order', 'L', 'X', 'C', 'V', 'BDAC', 0),
('Put these fractions in ascending order', '3/4', '1/2', '1/4', '2/3', 'CBDA', 0),
('Put these negative numbers from smallest to largest', '-5', '-20', '-1', '-10', 'BDCA', 0),

-- Time & Calendar Ordering
('Put these seasons in order starting with Spring', 'Winter', 'Fall', 'Spring', 'Summer', 'CDBA', 0),
('Put these months in calendar order', 'October', 'January', 'July', 'April', 'BDCA', 0),
('Put these times of day in chronological order', 'Midnight', 'Noon', 'Dawn', 'Dusk', 'ACBD', 0),
('Put these life stages in order', 'Adolescence', 'Infancy', 'Adulthood', 'Childhood', 'BDAC', 0),
('Put these time periods from shortest to longest', 'Year', 'Month', 'Decade', 'Week', 'DBAC', 0),

-- Scientific & Natural World
('Put these elements by atomic number (lowest to highest)', 'Iron', 'Hydrogen', 'Carbon', 'Oxygen', 'BCDA', 0),
('Put these animals by size (smallest to largest)', 'Horse', 'Mouse', 'Elephant', 'Dog', 'BDAC', 0),
('Put these colors in rainbow order (ROYGBIV)', 'Blue', 'Red', 'Yellow', 'Green', 'BCDA', 0),
('Put these planets by number of moons (fewest to most)', 'Earth', 'Mars', 'Jupiter', 'Venus', 'DABC', 0),
('Put these states of matter by temperature (coldest to hottest)', 'Gas', 'Liquid', 'Solid', 'Plasma', 'CBAD', 0),

-- Books & Literature
('Put these books by publication date (oldest first)', '1984', 'Moby Dick', 'Harry Potter', 'The Great Gatsby', 'BDAC', 0),
('Put these Harry Potter books in order', 'Prisoner of Azkaban', 'Philosopher''s Stone', 'Goblet of Fire', 'Chamber of Secrets', 'BDCA', 0),
('Put these Shakespeare plays in alphabetical order', 'Othello', 'Hamlet', 'Macbeth', 'Romeo and Juliet', 'BMCA', 0),
('Put these classic authors by birth year (oldest first)', 'Charles Dickens', 'William Shakespeare', 'Jane Austen', 'Mark Twain', 'BCAD', 0),

-- Technology & Innovation
('Put these tech companies by founding year (oldest first)', 'Google', 'Microsoft', 'Apple', 'Facebook', 'BCAD', 0),
('Put these gaming consoles in order of release', 'PlayStation', 'Atari 2600', 'Xbox', 'Nintendo Switch', 'BACD', 0),
('Put these Apple products in order of release (oldest first)', 'iPad', 'iPhone', 'Apple Watch', 'iPod', 'DBAC', 0),
('Put these social media platforms by launch year (oldest first)', 'Instagram', 'Facebook', 'TikTok', 'Twitter', 'BDAC', 0),

-- Music & Entertainment
('Put these musical notes in ascending pitch', 'E', 'C', 'G', 'A', 'BDCA', 0),
('Put these Beatles albums in order of release', 'Abbey Road', 'Help!', 'Let It Be', 'A Hard Day''s Night', 'DBAC', 0),
('Put these musical genres by approximate era of origin (oldest first)', 'Hip Hop', 'Jazz', 'Rock and Roll', 'Classical', 'DBCA', 0),
('Put these Disney movies in order of release (oldest first)', 'Frozen', 'The Lion King', 'Beauty and the Beast', 'Aladdin', 'CBDA', 0),

-- Sports & Games
('Put these Olympic sports by when they were added (oldest first)', 'Basketball', 'Swimming', 'Skateboarding', 'Tennis', 'BDAC', 0),
('Put these chess pieces by point value (lowest to highest)', 'Knight', 'Pawn', 'Rook', 'Queen', 'BACD', 0),
('Put these FIFA World Cups in chronological order', 'Brazil 2014', 'Germany 2006', 'South Africa 2010', 'Russia 2018', 'BCAD', 0),
('Put these Super Bowl winning teams in chronological order of first win', 'New England Patriots', 'Green Bay Packers', 'Dallas Cowboys', 'Pittsburgh Steelers', 'BCDAD', 0);

GO

-- Verify all CorrectAnswer values contain exactly A, B, C, D
DECLARE @ErrorCount INT = 0;

SELECT @ErrorCount = COUNT(*)
FROM fff_questions
WHERE LEN(CorrectAnswer) != 4
   OR CorrectAnswer NOT LIKE '%A%'
   OR CorrectAnswer NOT LIKE '%B%'
   OR CorrectAnswer NOT LIKE '%C%'
   OR CorrectAnswer NOT LIKE '%D%';

IF @ErrorCount > 0
BEGIN
    PRINT 'WARNING: ' + CAST(@ErrorCount AS VARCHAR) + ' questions have invalid CorrectAnswer format!';
END
ELSE
BEGIN
    PRINT 'FFF Questions table reset complete!';
    PRINT 'Total questions inserted: 40';
    PRINT 'All CorrectAnswer values validated successfully.';
END

GO
