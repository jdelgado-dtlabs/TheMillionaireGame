-- ============================================================================
-- Millionaire Game - Questions Table Reset Script
-- ============================================================================
-- This script drops and recreates the questions table with cleaned schema
-- and populates it with 80 sample questions (20 per difficulty level)
-- Removed columns: Custom_FiftyFifty, FiftyFifty_1, FiftyFifty_2, Custom_ATA, ATA_A, ATA_B, ATA_C, ATA_D
-- ============================================================================

USE dbMillionaire;
GO

-- Drop existing table
IF OBJECT_ID('questions', 'U') IS NOT NULL
    DROP TABLE questions;
GO

-- Create questions table with streamlined schema
CREATE TABLE questions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Question NTEXT NOT NULL,
    A NVARCHAR(500) NOT NULL,
    B NVARCHAR(500) NOT NULL,
    C NVARCHAR(500) NOT NULL,
    D NVARCHAR(500) NOT NULL,
    CorrectAnswer VARCHAR(1) NOT NULL CHECK (CorrectAnswer IN ('A', 'B', 'C', 'D')),
    Level INT NOT NULL CHECK (Level BETWEEN 1 AND 15),
    Used BIT NOT NULL DEFAULT 0,
    Note NTEXT NULL,
    Difficulty_Type VARCHAR(20) NOT NULL CHECK (Difficulty_Type IN ('Specific', 'Range'))
);
GO

-- ============================================================================
-- LEVEL 1-5 QUESTIONS (Easy - $100 to $1,000)
-- Difficulty_Type: Range
-- ============================================================================

INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Difficulty_Type, Note) VALUES
('What is the capital of France?', 'London', 'Berlin', 'Paris', 'Madrid', 'C', 1, 'Range', 'Basic geography'),
('How many days are in a week?', 'Five', 'Six', 'Seven', 'Eight', 'C', 1, 'Range', 'Basic time knowledge'),
('What color is the sky on a clear day?', 'Green', 'Blue', 'Red', 'Yellow', 'B', 1, 'Range', 'Basic observation'),
('What do bees make?', 'Milk', 'Honey', 'Silk', 'Wool', 'B', 1, 'Range', 'Nature knowledge'),
('How many wheels does a bicycle have?', 'One', 'Two', 'Three', 'Four', 'B', 1, 'Range', 'Basic counting'),

('What is 2 + 2?', 'Three', 'Four', 'Five', 'Six', 'B', 2, 'Range', 'Basic arithmetic'),
('Which animal is known as man''s best friend?', 'Cat', 'Dog', 'Horse', 'Bird', 'B', 2, 'Range', 'Common knowledge'),
('What do you use to write on a chalkboard?', 'Pen', 'Pencil', 'Chalk', 'Marker', 'C', 2, 'Range', 'School supplies'),
('How many sides does a triangle have?', 'Two', 'Three', 'Four', 'Five', 'B', 2, 'Range', 'Basic geometry'),
('What is the color of grass?', 'Blue', 'Green', 'Red', 'Yellow', 'B', 2, 'Range', 'Nature observation'),

('Which planet do we live on?', 'Mars', 'Venus', 'Earth', 'Jupiter', 'C', 3, 'Range', 'Basic astronomy'),
('What do we call frozen water?', 'Steam', 'Ice', 'Rain', 'Snow', 'B', 3, 'Range', 'States of matter'),
('How many months are in a year?', 'Ten', 'Eleven', 'Twelve', 'Thirteen', 'C', 3, 'Range', 'Calendar knowledge'),
('What is the largest ocean on Earth?', 'Atlantic', 'Indian', 'Arctic', 'Pacific', 'D', 3, 'Range', 'Geography'),
('Which fruit is red and grows on trees?', 'Banana', 'Apple', 'Orange', 'Grape', 'B', 3, 'Range', 'Fruit identification'),

('How many cents are in a dollar?', 'Fifty', 'Seventy-five', 'One hundred', 'One hundred twenty-five', 'C', 4, 'Range', 'Money knowledge'),
('What do we call a baby dog?', 'Kitten', 'Puppy', 'Cub', 'Calf', 'B', 4, 'Range', 'Animal terms'),
('Which season comes after winter?', 'Summer', 'Spring', 'Fall', 'Autumn', 'B', 4, 'Range', 'Seasons'),
('What shape is a stop sign?', 'Circle', 'Square', 'Triangle', 'Octagon', 'D', 4, 'Range', 'Traffic signs'),
('How many hours are in a day?', 'Twelve', 'Twenty', 'Twenty-four', 'Thirty-six', 'C', 4, 'Range', 'Time measurement');

-- ============================================================================
-- LEVEL 6-10 QUESTIONS (Medium - $2,000 to $32,000)
-- Difficulty_Type: Range
-- ============================================================================

INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Difficulty_Type, Note) VALUES
('Who painted the Mona Lisa?', 'Vincent van Gogh', 'Leonardo da Vinci', 'Pablo Picasso', 'Claude Monet', 'B', 6, 'Range', 'Art history'),
('What is the chemical symbol for gold?', 'Go', 'Gd', 'Au', 'Ag', 'C', 6, 'Range', 'Chemistry'),
('In what year did World War II end?', '1943', '1944', '1945', '1946', 'C', 6, 'Range', 'World history'),
('How many strings does a standard guitar have?', 'Four', 'Five', 'Six', 'Seven', 'C', 6, 'Range', 'Music'),
('What is the tallest mountain in the world?', 'K2', 'Kilimanjaro', 'Mount Everest', 'Denali', 'C', 6, 'Range', 'Geography'),

('Who wrote "Romeo and Juliet"?', 'Charles Dickens', 'William Shakespeare', 'Mark Twain', 'Jane Austen', 'B', 7, 'Range', 'Literature'),
('What is the speed of light approximately?', '186,000 mph', '186,000 km/s', '300,000 mph', '300,000 km/s', 'D', 7, 'Range', 'Physics'),
('Which country is home to the kangaroo?', 'New Zealand', 'Australia', 'South Africa', 'Brazil', 'B', 7, 'Range', 'Wildlife'),
('What is the square root of 144?', 'Ten', 'Eleven', 'Twelve', 'Thirteen', 'C', 7, 'Range', 'Mathematics'),
('Who was the first President of the United States?', 'Thomas Jefferson', 'John Adams', 'George Washington', 'Benjamin Franklin', 'C', 7, 'Range', 'US history'),

('What is the smallest country in the world?', 'Monaco', 'Vatican City', 'San Marino', 'Liechtenstein', 'B', 8, 'Range', 'Geography'),
('How many bones are in the adult human body?', '186', '206', '226', '246', 'B', 8, 'Range', 'Human anatomy'),
('What is the chemical formula for water?', 'CO2', 'H2O', 'O2', 'NaCl', 'B', 8, 'Range', 'Chemistry'),
('Which planet is known as the Red Planet?', 'Venus', 'Mars', 'Jupiter', 'Saturn', 'B', 8, 'Range', 'Astronomy'),
('In which year did the Titanic sink?', '1910', '1911', '1912', '1913', 'C', 8, 'Range', 'Historical events'),

('What is the capital of Japan?', 'Osaka', 'Kyoto', 'Tokyo', 'Hiroshima', 'C', 9, 'Range', 'World capitals'),
('How many teeth does an adult human typically have?', '28', '30', '32', '34', 'C', 9, 'Range', 'Dental knowledge'),
('What is the largest organ in the human body?', 'Liver', 'Brain', 'Heart', 'Skin', 'D', 9, 'Range', 'Human biology'),
('Who developed the theory of relativity?', 'Isaac Newton', 'Albert Einstein', 'Nikola Tesla', 'Stephen Hawking', 'B', 9, 'Range', 'Physics'),
('What is the longest river in the world?', 'Amazon', 'Nile', 'Mississippi', 'Yangtze', 'B', 9, 'Range', 'Geography');

-- ============================================================================
-- LEVEL 11-14 QUESTIONS (Hard - $64,000 to $500,000)
-- Difficulty_Type: Range
-- ============================================================================

INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Difficulty_Type, Note) VALUES
('What is the only mammal capable of true flight?', 'Flying squirrel', 'Bat', 'Flying lemur', 'Sugar glider', 'B', 11, 'Range', 'Zoology'),
('Which element has the atomic number 1?', 'Helium', 'Hydrogen', 'Oxygen', 'Nitrogen', 'B', 11, 'Range', 'Chemistry'),
('Who wrote "1984"?', 'Aldous Huxley', 'Ray Bradbury', 'George Orwell', 'H.G. Wells', 'C', 11, 'Range', 'Literature'),
('In what year did the Berlin Wall fall?', '1987', '1988', '1989', '1990', 'C', 11, 'Range', 'Modern history'),
('What is the hardest natural substance on Earth?', 'Granite', 'Steel', 'Diamond', 'Tungsten', 'C', 11, 'Range', 'Geology'),

('How many symphonies did Beethoven compose?', 'Seven', 'Eight', 'Nine', 'Ten', 'C', 12, 'Range', 'Classical music'),
('What is the rarest blood type?', 'O negative', 'AB positive', 'AB negative', 'B negative', 'C', 12, 'Range', 'Medical science'),
('Which country has the most time zones?', 'Russia', 'United States', 'Canada', 'France', 'D', 12, 'Range', 'Geography - France has 12 with overseas territories'),
('What is the smallest bone in the human body?', 'Stapes', 'Malleus', 'Incus', 'Patella', 'A', 12, 'Range', 'Anatomy - ear bone'),
('Who discovered penicillin?', 'Louis Pasteur', 'Marie Curie', 'Alexander Fleming', 'Jonas Salk', 'C', 12, 'Range', 'Medical history'),

('What is the capital of Mongolia?', 'Ulaanbaatar', 'Astana', 'Bishkek', 'Tashkent', 'A', 13, 'Range', 'World capitals'),
('How many keys are on a standard piano?', '76', '82', '88', '94', 'C', 13, 'Range', 'Music'),
('What is the most abundant gas in Earth''s atmosphere?', 'Oxygen', 'Carbon dioxide', 'Nitrogen', 'Argon', 'C', 13, 'Range', 'Atmospheric science'),
('Who painted "The Starry Night"?', 'Claude Monet', 'Vincent van Gogh', 'Paul Cézanne', 'Henri Matisse', 'B', 13, 'Range', 'Art history'),
('What is the largest desert in the world?', 'Sahara', 'Arabian', 'Antarctic', 'Gobi', 'C', 13, 'Range', 'Geography - Antarctica is technically the largest desert'),

('In which year did the French Revolution begin?', '1787', '1788', '1789', '1790', 'C', 14, 'Range', 'European history'),
('What is the second most spoken language in the world?', 'English', 'Spanish', 'Mandarin Chinese', 'Hindi', 'B', 14, 'Range', 'Linguistics'),
('How many chambers does a human heart have?', 'Two', 'Three', 'Four', 'Five', 'C', 14, 'Range', 'Cardiology'),
('What is the currency of Switzerland?', 'Euro', 'Swiss Franc', 'Krone', 'Pound', 'B', 14, 'Range', 'World currencies'),
('Who invented the telephone?', 'Thomas Edison', 'Nikola Tesla', 'Alexander Graham Bell', 'Guglielmo Marconi', 'C', 14, 'Range', 'Invention history');

-- ============================================================================
-- LEVEL 15 QUESTIONS (Million Dollar Questions)
-- Difficulty_Type: Range
-- ============================================================================

INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Difficulty_Type, Note) VALUES
('What is the oldest continuously inhabited city in the world?', 'Athens', 'Damascus', 'Jerusalem', 'Jericho', 'B', 15, 'Range', 'Ancient history - Damascus, Syria'),
('Who was the first woman to win a Nobel Prize?', 'Rosalind Franklin', 'Marie Curie', 'Dorothy Hodgkin', 'Barbara McClintock', 'B', 15, 'Range', 'Nobel Prize history'),
('What is the longest-running Broadway show?', 'Cats', 'Les Misérables', 'The Phantom of the Opera', 'Chicago', 'C', 15, 'Range', 'Theatre history'),
('In what year was the first email sent?', '1969', '1971', '1973', '1975', 'B', 15, 'Range', 'Technology history'),
('What is the only letter that doesn''t appear in any US state name?', 'Q', 'X', 'Z', 'J', 'A', 15, 'Range', 'US geography trivia'),

('Which Shakespeare play features the character Caliban?', 'The Tempest', 'Othello', 'Macbeth', 'Hamlet', 'A', 15, 'Range', 'Shakespeare literature'),
('What is the smallest prime number?', '0', '1', '2', '3', 'C', 15, 'Range', 'Number theory'),
('Who was the first person to reach the South Pole?', 'Robert Falcon Scott', 'Roald Amundsen', 'Ernest Shackleton', 'Richard Byrd', 'B', 15, 'Range', 'Exploration history - 1911'),
('What is the most expensive spice in the world by weight?', 'Vanilla', 'Cardamom', 'Saffron', 'Cinnamon', 'C', 15, 'Range', 'Culinary knowledge'),
('In which year was the first atom split?', '1917', '1919', '1932', '1938', 'C', 15, 'Range', 'Physics history - Cockcroft and Walton'),

('What is the national animal of Scotland?', 'Lion', 'Eagle', 'Unicorn', 'Dragon', 'C', 15, 'Range', 'National symbols'),
('Which planet has the most moons?', 'Jupiter', 'Saturn', 'Uranus', 'Neptune', 'B', 15, 'Range', 'Astronomy - Saturn has 146 known moons'),
('What is the oldest known musical instrument?', 'Lyre', 'Flute', 'Drum', 'Harp', 'B', 15, 'Range', 'Archaeology - 40,000-year-old bone flute'),
('Who was the first computer programmer?', 'Alan Turing', 'Ada Lovelace', 'Grace Hopper', 'Charles Babbage', 'B', 15, 'Range', 'Computing history'),
('What is the deepest point in Earth''s oceans?', 'Puerto Rico Trench', 'Java Trench', 'Mariana Trench', 'Philippine Trench', 'C', 15, 'Range', 'Oceanography - Challenger Deep'),

('Which element is named after the Greek word for "sun"?', 'Gold', 'Helium', 'Radium', 'Uranium', 'B', 15, 'Range', 'Etymology - Helios'),
('What is the fastest land animal over long distances?', 'Cheetah', 'Pronghorn antelope', 'Springbok', 'Wildebeest', 'B', 15, 'Range', 'Zoology - sustained speed'),
('In which city was the first subway system opened?', 'Paris', 'New York', 'London', 'Berlin', 'C', 15, 'Range', 'Transportation history - 1863'),
('What is the only continent without an active volcano?', 'Europe', 'Australia', 'Antarctica', 'South America', 'B', 15, 'Range', 'Geology'),
('Who wrote the original "Sherlock Holmes" stories?', 'Agatha Christie', 'Arthur Conan Doyle', 'Edgar Allan Poe', 'G.K. Chesterton', 'B', 15, 'Range', 'Detective fiction');

GO

PRINT 'Questions table reset complete!';
PRINT 'Total questions inserted: 80 (20 per difficulty level)';
GO
