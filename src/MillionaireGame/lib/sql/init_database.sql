-- ============================================================================
-- The Millionaire Game - Database Initialization Script
-- Version: 1.0.0
-- Description: Creates and populates Questions and FFF Questions tables
-- ============================================================================
-- This script will:
--   1. Drop existing questions and fff_questions tables (if they exist)
--   2. Create fresh table structures
--   3. Populate with default question data (80 questions + 41 FFF questions)
-- 
-- Note: Other tables (ApplicationSettings, ATAVotes, FFFAnswers, Participants,
--       Sessions, settings_Contestants, settings_HostMessages) are created
--       automatically by the application at runtime.
-- ============================================================================

USE dbMillionaire;
GO

-- ============================================================================
-- DROP TABLES (if they exist)
-- ============================================================================

IF OBJECT_ID('dbo.questions', 'U') IS NOT NULL
BEGIN
    PRINT 'Dropping existing questions table...';
    DROP TABLE dbo.questions;
END
GO

IF OBJECT_ID('dbo.fff_questions', 'U') IS NOT NULL
BEGIN
    PRINT 'Dropping existing fff_questions table...';
    DROP TABLE dbo.fff_questions;
END
GO

-- ============================================================================
-- CREATE QUESTIONS TABLE
-- ============================================================================

PRINT 'Creating questions table...';

CREATE TABLE dbo.questions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Question NTEXT NOT NULL,
    A NVARCHAR(500) NOT NULL,
    B NVARCHAR(500) NOT NULL,
    C NVARCHAR(500) NOT NULL,
    D NVARCHAR(500) NOT NULL,
    CorrectAnswer VARCHAR(1) NOT NULL CHECK (CorrectAnswer IN ('A', 'B', 'C', 'D')),
    Level INT NOT NULL CHECK (Level BETWEEN 1 AND 15),
    Note NTEXT NULL,
    Used BIT NOT NULL DEFAULT 0,
    Difficulty_Type VARCHAR(20) NULL,
    CONSTRAINT CHK_Questions_Level CHECK (Level >= 1 AND Level <= 15)
);
GO

-- ============================================================================
-- CREATE FFF_QUESTIONS TABLE
-- ============================================================================

PRINT 'Creating fff_questions table...';

CREATE TABLE dbo.fff_questions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Question NTEXT NOT NULL,
    A NVARCHAR(500) NOT NULL,
    B NVARCHAR(500) NOT NULL,
    C NVARCHAR(500) NOT NULL,
    D NVARCHAR(500) NOT NULL,
    CorrectAnswer VARCHAR(5) NOT NULL CHECK (LEN(CorrectAnswer) = 4),
    Used BIT NOT NULL DEFAULT 0
);
GO

PRINT 'Tables created successfully.';
PRINT 'Ready to populate with question data...';
GO

-- ============================================================================
-- INSERT QUESTIONS DATA (80 Questions)
-- ============================================================================

PRINT 'Inserting questions data...';
GO

INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the capital of France?', N'London', N'Berlin', N'Paris', N'Madrid', 'C', 1, N'This city is home to the Eiffel Tower and was called the "City of Light"', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many days are in a week?', N'Five', N'Six', N'Seven', N'Eight', 'C', 1, N'Think about how many days from Monday through Sunday', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What color is the sky on a clear day?', N'Green', N'Blue', N'Red', N'Yellow', 'B', 1, N'This is the same color as the ocean on a sunny day', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What do bees make?', N'Milk', N'Honey', N'Silk', N'Wool', 'B', 1, N'This sweet golden substance is often used on toast or in tea', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many wheels does a bicycle have?', N'One', N'Two', N'Three', N'Four', 'B', 1, N'A bicycle is different from a tricycle by having one less wheel', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is 2 + 2?', N'Three', N'Four', N'Five', N'Six', 'B', 2, N'If you have two apples and get two more apples, you have this many', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which animal is known as man''s best friend?', N'Cat', N'Dog', N'Horse', N'Bird', 'B', 2, N'This pet is known for loyalty, wagging tails, and barking', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What do you use to write on a chalkboard?', N'Pen', N'Pencil', N'Chalk', N'Marker', 'C', 2, N'This writing tool shares the same name as the board itself', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many sides does a triangle have?', N'Two', N'Three', N'Four', N'Five', 'B', 2, N'The prefix "tri" means this number in Latin', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the color of grass?', N'Blue', N'Green', N'Red', N'Yellow', 'B', 2, N'This color is associated with nature, forests, and healthy plants', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which planet do we live on?', N'Mars', N'Venus', N'Earth', N'Jupiter', 'C', 3, N'This planet is known as the "Blue Planet" and has one moon', 1, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What do we call frozen water?', N'Steam', N'Ice', N'Rain', N'Snow', 'B', 3, N'Hockey is played on rinks made of this solid form of water', 1, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many months are in a year?', N'Ten', N'Eleven', N'Twelve', N'Thirteen', 'C', 3, N'Think of a dozen - that''s how many months we have', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the largest ocean on Earth?', N'Atlantic', N'Indian', N'Arctic', N'Pacific', 'D', 3, N'This ocean touches both Asia and the Americas', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which fruit is red and grows on trees?', N'Banana', N'Apple', N'Orange', N'Grape', 'B', 3, N'This fruit is famously associated with Isaac Newton and gravity', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many cents are in a dollar?', N'Fifty', N'Seventy-five', N'One hundred', N'One hundred twenty-five', 'C', 4, N'Ten dimes equals this many cents, same as four quarters', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What do we call a baby dog?', N'Kitten', N'Puppy', N'Cub', N'Calf', 'B', 4, N'This term for a young dog sounds similar to the word "pup"', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which season comes after winter?', N'Summer', N'Spring', N'Fall', N'Autumn', 'B', 4, N'This season is when flowers bloom and birds return from migration', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What shape is a stop sign?', N'Circle', N'Square', N'Triangle', N'Octagon', 'D', 4, N'This eight-sided shape is unique among traffic signs', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many hours are in a day?', N'Twelve', N'Twenty', N'Twenty-four', N'Thirty-six', 'C', 4, N'This is twice the number of hours from noon to midnight', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who painted the Mona Lisa?', N'Vincent van Gogh', N'Leonardo da Vinci', N'Pablo Picasso', N'Claude Monet', 'B', 6, N'This Renaissance artist was also an inventor who designed flying machines', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the chemical symbol for gold?', N'Go', N'Gd', N'Au', N'Ag', 'C', 6, N'This symbol comes from the Latin word "aurum" meaning shining dawn', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'In what year did World War II end?', N'1943', N'1944', N'1945', N'1946', 'C', 6, N'This was the year the atomic bombs were dropped on Japan', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many strings does a standard guitar have?', N'Four', N'Five', N'Six', N'Seven', 'C', 6, N'This is the same number of sides as a hexagon', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the tallest mountain in the world?', N'K2', N'Kilimanjaro', N'Mount Everest', N'Denali', 'C', 6, N'This Himalayan peak is named after a British surveyor', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who wrote "Romeo and Juliet"?', N'Charles Dickens', N'William Shakespeare', N'Mark Twain', N'Jane Austen', 'B', 7, N'This Elizabethan playwright also wrote Hamlet and Macbeth', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the speed of light approximately?', N'186,000 mph', N'186,000 km/s', N'300,000 mph', N'300,000 km/s', 'D', 7, N'This speed is measured in kilometers per second, not miles per hour', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which country is home to the kangaroo?', N'New Zealand', N'Australia', N'South Africa', N'Brazil', 'B', 7, N'This continent-country is the world''s largest island', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the square root of 144?', N'Ten', N'Eleven', N'Twelve', N'Thirteen', 'C', 7, N'This number multiplied by itself equals 144, and it''s a dozen', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who was the first President of the United States?', N'Thomas Jefferson', N'John Adams', N'George Washington', N'Benjamin Franklin', 'C', 7, N'This founding father led the Continental Army during the Revolution', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the smallest country in the world?', N'Monaco', N'Vatican City', N'San Marino', N'Liechtenstein', 'B', 8, N'This country is entirely surrounded by Rome and is headquarters of the Catholic Church', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many bones are in the adult human body?', N'186', N'206', N'226', N'246', 'B', 8, N'This number is just over 200, specifically 6 more than 200', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the chemical formula for water?', N'CO2', N'H2O', N'O2', N'NaCl', 'B', 8, N'This molecule has two hydrogen atoms and one oxygen atom', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which planet is known as the Red Planet?', N'Venus', N'Mars', N'Jupiter', N'Saturn', 'B', 8, N'This planet is named after the Roman god of war', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'In which year did the Titanic sink?', N'1910', N'1911', N'1912', N'1913', 'C', 8, N'The "unsinkable" ship sank on its maiden voyage in April of this year', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the capital of Japan?', N'Osaka', N'Kyoto', N'Tokyo', N'Hiroshima', 'C', 9, N'This city hosted the Summer Olympics in 1964 and again in 2021', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many teeth does an adult human typically have?', N'28', N'30', N'32', N'34', 'C', 9, N'This includes four wisdom teeth if they all develop', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the largest organ in the human body?', N'Liver', N'Brain', N'Heart', N'Skin', 'D', 9, N'This organ covers your entire body and protects you from the outside world', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who developed the theory of relativity?', N'Isaac Newton', N'Albert Einstein', N'Nikola Tesla', N'Stephen Hawking', 'B', 9, N'This physicist with wild hair is famous for E=mcÂ²', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the longest river in the world?', N'Amazon', N'Nile', N'Mississippi', N'Yangtze', 'B', 9, N'This African river flows through Egypt and into the Mediterranean Sea', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the only mammal capable of true flight?', N'Flying squirrel', N'Bat', N'Flying lemur', N'Sugar glider', 'B', 11, N'This nocturnal creature uses echolocation to navigate in darkness', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which element has the atomic number 1?', N'Helium', N'Hydrogen', N'Oxygen', N'Nitrogen', 'B', 11, N'This lightest element makes up 75% of all matter in the universe', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who wrote "1984"?', N'Aldous Huxley', N'Ray Bradbury', N'George Orwell', N'H.G. Wells', 'C', 11, N'This British author also wrote "Animal Farm" about revolutionary pigs', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'In what year did the Berlin Wall fall?', N'1987', N'1988', N'1989', N'1990', 'C', 11, N'This happened the same year as the Tiananmen Square protests', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the hardest natural substance on Earth?', N'Granite', N'Steel', N'Diamond', N'Tungsten', 'C', 11, N'This gemstone rates 10 on the Mohs hardness scale', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many symphonies did Beethoven compose?', N'Seven', N'Eight', N'Nine', N'Ten', 'C', 12, N'His final symphony included the "Ode to Joy" choral movement', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the rarest blood type?', N'O negative', N'AB positive', N'AB negative', N'B negative', 'C', 12, N'This blood type is found in less than 1% of the population', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which country has the most time zones?', N'Russia', N'United States', N'Canada', N'France', 'D', 12, N'This country has 12 time zones due to its overseas territories', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the smallest bone in the human body?', N'Stapes', N'Malleus', N'Incus', N'Patella', 'A', 12, N'This tiny stirrup-shaped bone is located in the middle ear', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who discovered penicillin?', N'Louis Pasteur', N'Marie Curie', N'Alexander Fleming', N'Jonas Salk', 'C', 12, N'This Scottish bacteriologist made his discovery by accident in 1928', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the capital of Mongolia?', N'Ulaanbaatar', N'Astana', N'Bishkek', N'Tashkent', 'A', 13, N'This city''s name means "Red Hero" in the Mongolian language', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many keys are on a standard piano?', N'76', N'82', N'88', N'94', 'C', 13, N'This number consists of 52 white keys and 36 black keys', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the most abundant gas in Earth''s atmosphere?', N'Oxygen', N'Carbon dioxide', N'Nitrogen', N'Argon', 'C', 13, N'This gas makes up about 78% of our atmosphere', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who painted "The Starry Night"?', N'Claude Monet', N'Vincent van Gogh', N'Paul CÃ©zanne', N'Henri Matisse', 'B', 13, N'This Dutch post-impressionist artist famously cut off his own ear', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the largest desert in the world?', N'Sahara', N'Arabian', N'Antarctic', N'Gobi', 'C', 13, N'A desert is defined by precipitation, not temperature - this icy continent qualifies', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'In which year did the French Revolution begin?', N'1787', N'1788', N'1789', N'1790', 'C', 14, N'The Storming of the Bastille happened on July 14th of this year', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the second most spoken language in the world?', N'English', N'Spanish', N'Mandarin Chinese', N'Hindi', 'B', 14, N'This Romance language is the official language in 20 countries', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'How many chambers does a human heart have?', N'Two', N'Three', N'Four', N'Five', 'C', 14, N'This number includes two atria and two ventricles', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the currency of Switzerland?', N'Euro', N'Swiss Franc', N'Krone', N'Pound', 'B', 14, N'Switzerland is not in the EU and uses its own currency', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who invented the telephone?', N'Thomas Edison', N'Nikola Tesla', N'Alexander Graham Bell', N'Guglielmo Marconi', 'C', 14, N'This Scottish-born inventor famously made the first call to his assistant Watson', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the oldest continuously inhabited city in the world?', N'Athens', N'Damascus', N'Jerusalem', N'Jericho', 'B', 15, N'This Syrian city has been continuously inhabited since 11,000 BCE', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who was the first woman to win a Nobel Prize?', N'Rosalind Franklin', N'Marie Curie', N'Dorothy Hodgkin', N'Barbara McClintock', 'B', 15, N'This Polish-French physicist discovered radium and polonium', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the longest-running Broadway show?', N'Cats', N'Les MisÃ©rables', N'The Phantom of the Opera', N'Chicago', 'C', 15, N'This Andrew Lloyd Webber musical ran for over 35 years', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'In what year was the first email sent?', N'1969', N'1971', N'1973', N'1975', 'B', 15, N'Ray Tomlinson sent the first networked email two years after ARPANET launched', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the only letter that doesn''t appear in any US state name?', N'Q', N'X', N'Z', N'J', 'A', 15, N'This letter is the 17th in the alphabet and starts few English words', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which Shakespeare play features the character Caliban?', N'The Tempest', N'Othello', N'Macbeth', N'Hamlet', 'A', 15, N'In this play, Caliban is the son of the witch Sycorax and lives on an island', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the smallest prime number?', N'0', N'1', N'2', N'3', 'C', 15, N'This is the only even prime number that exists', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who was the first person to reach the South Pole?', N'Robert Falcon Scott', N'Roald Amundsen', N'Ernest Shackleton', N'Richard Byrd', 'B', 15, N'This Norwegian explorer arrived in December 1911, beating Scott by 34 days', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the most expensive spice in the world by weight?', N'Vanilla', N'Cardamom', N'Saffron', N'Cinnamon', 'C', 15, N'This spice comes from crocus flowers and requires 75,000 flowers per pound', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'In which year was the first atom split?', N'1917', N'1919', N'1932', N'1938', 'C', 15, N'Cockcroft and Walton achieved this nuclear physics breakthrough in the early 1930s', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the national animal of Scotland?', N'Lion', N'Eagle', N'Unicorn', N'Dragon', 'C', 15, N'This mythical creature appears on the Scottish royal coat of arms', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which planet has the most moons?', N'Jupiter', N'Saturn', N'Uranus', N'Neptune', 'B', 15, N'This ringed planet has 146 known moons, surpassing Jupiter', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the oldest known musical instrument?', N'Lyre', N'Flute', N'Drum', N'Harp', 'B', 15, N'A 40,000-year-old example made from bone was found in a German cave', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who was the first computer programmer?', N'Alan Turing', N'Ada Lovelace', N'Grace Hopper', N'Charles Babbage', 'B', 15, N'This mathematician wrote algorithms for Charles Babbage''s Analytical Engine in the 1840s', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the deepest point in Earth''s oceans?', N'Puerto Rico Trench', N'Java Trench', N'Mariana Trench', N'Philippine Trench', 'C', 15, N'Challenger Deep in this trench is nearly 36,000 feet below sea level', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Which element is named after the Greek word for "sun"?', N'Gold', N'Helium', N'Radium', N'Uranium', 'B', 15, N'This noble gas was first discovered in the sun''s spectrum before being found on Earth', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the fastest land animal over long distances?', N'Cheetah', N'Pronghorn antelope', N'Springbok', N'Wildebeest', 'B', 15, N'While cheetahs are fastest in sprints, this North American animal wins marathons at 55 mph', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'In which city was the first subway system opened?', N'Paris', N'New York', N'London', N'Berlin', 'C', 15, N'The Metropolitan Railway opened here in 1863, pioneering underground transit', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'What is the only continent without an active volcano?', N'Europe', N'Australia', N'Antarctica', N'South America', 'B', 15, N'This continent-country has no recent volcanic activity despite its ancient geology', 0, 'Range');
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note, Used, Difficulty_Type) VALUES (N'Who wrote the original "Sherlock Holmes" stories?', N'Agatha Christie', N'Arthur Conan Doyle', N'Edgar Allan Poe', N'G.K. Chesterton', 'B', 15, N'This Scottish physician created the famous detective and his companion Watson', 0, 'Range');

GO

PRINT 'Questions inserted successfully (80 questions).';
GO

-- ============================================================================
-- INSERT FFF_QUESTIONS DATA (41 Questions)
-- ============================================================================

PRINT 'Inserting FFF questions data...';
GO

INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these planets in order from closest to farthest from the Sun', N'Mars', N'Venus', N'Earth', N'Mercury', 'DBCA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these oceans by area (largest to smallest)', N'Indian', N'Pacific', N'Atlantic', N'Arctic', 'BCAD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these continents by population (most to least)', N'Africa', N'Asia', N'Europe', N'North America', 'BCAD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these mountains by height (shortest to tallest)', N'K2', N'Mount Everest', N'Kilimanjaro', N'Denali', 'CDAB', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these cities by population (largest to smallest)', N'Paris', N'Tokyo', N'London', N'New York', 'BDCA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these US Presidents in chronological order', N'Lincoln', N'Washington', N'Roosevelt', N'Kennedy', 'BACD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these events in chronological order', N'Moon Landing', N'World War I', N'Fall of Berlin Wall', N'American Revolution', 'DBAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these inventions by year (oldest first)', N'Telephone', N'Light Bulb', N'Automobile', N'Airplane', 'CBAD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these Olympiads in chronological order', N'Beijing 2008', N'Athens 2004', N'London 2012', N'Rio 2016', 'BACD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these movies in order of release date (oldest first)', N'The Matrix', N'Star Wars', N'Avatar', N'Titanic', 'BDAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these numbers in ascending order', N'50', N'10', N'25', N'100', 'BCAD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these letters in alphabetical order', N'Z', N'M', N'A', N'Q', 'CBDA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these Roman numerals in ascending order', N'L', N'X', N'C', N'V', 'BDAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these fractions in ascending order', N'3/4', N'1/2', N'1/4', N'2/3', 'CBDA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these negative numbers from smallest to largest', N'-5', N'-20', N'-1', N'-10', 'BDCA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these seasons in order starting with Spring', N'Winter', N'Fall', N'Spring', N'Summer', 'CDBA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these months in calendar order', N'October', N'January', N'July', N'April', 'BDCA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these times of day in chronological order', N'Midnight', N'Noon', N'Dawn', N'Dusk', 'ACBD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these life stages in order', N'Adolescence', N'Infancy', N'Adulthood', N'Childhood', 'BDAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these time periods from shortest to longest', N'Year', N'Month', N'Decade', N'Week', 'DBAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these elements by atomic number (lowest to highest)', N'Iron', N'Hydrogen', N'Carbon', N'Oxygen', 'BCDA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these animals by size (smallest to largest)', N'Horse', N'Mouse', N'Elephant', N'Dog', 'BDAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these colors in rainbow order (ROYGBIV)', N'Blue', N'Red', N'Yellow', N'Green', 'BCDA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these planets by number of moons (fewest to most)', N'Earth', N'Mars', N'Jupiter', N'Venus', 'DABC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these states of matter by temperature (coldest to hottest)', N'Gas', N'Liquid', N'Solid', N'Plasma', 'CBAD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these books by publication date (oldest first)', N'1984', N'Moby Dick', N'Harry Potter', N'The Great Gatsby', 'BDAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these Harry Potter books in order', N'Prisoner of Azkaban', N'Philosopher''s Stone', N'Goblet of Fire', N'Chamber of Secrets', 'BDCA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these Shakespeare plays in alphabetical order', N'Othello', N'Hamlet', N'Macbeth', N'Romeo and Juliet', 'BCAD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these classic authors by birth year (oldest first)', N'Charles Dickens', N'William Shakespeare', N'Jane Austen', N'Mark Twain', 'BCAD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these tech companies by founding year (oldest first)', N'Google', N'Microsoft', N'Apple', N'Facebook', 'BCAD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these gaming consoles in order of release', N'PlayStation', N'Atari 2600', N'Xbox', N'Nintendo Switch', 'BACD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these Apple products in order of release (oldest first)', N'iPad', N'iPhone', N'Apple Watch', N'iPod', 'DBAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these social media platforms by launch year (oldest first)', N'Instagram', N'Facebook', N'TikTok', N'Twitter', 'BDAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these musical notes in ascending pitch', N'E', N'C', N'G', N'A', 'BDCA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these Beatles albums in order of release', N'Abbey Road', N'Help!', N'Let It Be', N'A Hard Day''s Night', 'DBAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these musical genres by approximate era of origin (oldest first)', N'Hip Hop', N'Jazz', N'Rock and Roll', N'Classical', 'DBCA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these Disney movies in order of release (oldest first)', N'Frozen', N'The Lion King', N'Beauty and the Beast', N'Aladdin', 'CBDA', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these Olympic sports by when they were added (oldest first)', N'Basketball', N'Swimming', N'Skateboarding', N'Tennis', 'BDAC', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these chess pieces by point value (lowest to highest)', N'Knight', N'Pawn', N'Rook', N'Queen', 'BACD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these FIFA World Cups in chronological order', N'Brazil 2014', N'Germany 2006', N'South Africa 2010', N'Russia 2018', 'BCAD', 0);
INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES (N'Put these Super Bowl winning teams in chronological order of first win', N'New England Patriots', N'Green Bay Packers', N'Dallas Cowboys', N'Pittsburgh Steelers', 'BCDAD', 0);

GO

PRINT 'FFF Questions inserted successfully (41 questions).';
PRINT '';
PRINT '============================================================================';
PRINT 'Database initialization complete!';
PRINT '============================================================================';
PRINT 'Questions: 80';
PRINT 'FFF Questions: 41';
PRINT 'All other tables will be created automatically by the application at runtime.';
GO
