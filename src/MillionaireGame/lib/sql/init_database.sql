-- ============================================================================
-- The Millionaire Game - Database Initialization Script
-- Version: 1.0.0
-- Description: Creates and populates Questions and FFF Questions tables
-- ============================================================================
-- This script will:
--   1. Drop existing questions and fff_questions tables (if they exist)
--   2. Create fresh table structures
--   3. Populate with default question data (80 questions + 44 FFF questions)
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
    Level INT NOT NULL CHECK (Level BETWEEN 1 AND 4),
    Used BIT NOT NULL DEFAULT 0,
    Note NTEXT NULL
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
    CorrectAnswer VARCHAR(4) NOT NULL,
    Used BIT NOT NULL DEFAULT 0
);
GO

PRINT 'Tables created successfully.';
PRINT 'Ready to populate with question data...';
GO

-- ============================================================================
-- INSERT QUESTIONS DATA (80 Questions)
-- Level 1 = Easy (Q1-5), Level 2 = Medium (Q6-10), Level 3 = Hard (Q11-14), Level 4 = Million (Q15)
-- ============================================================================

PRINT 'Inserting questions data...';
GO

-- LEVEL 1 QUESTIONS (Easy - Game Questions 1-5: $100 to $1,000)
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note) VALUES
('What is the capital of France?', 'London', 'Berlin', 'Paris', 'Madrid', 'C', 1, 'This city is home to the Eiffel Tower and was called the "City of Light"'),
('How many days are in a week?', 'Five', 'Six', 'Seven', 'Eight', 'C', 1, 'Think about how many days from Monday through Sunday'),
('What color is the sky on a clear day?', 'Green', 'Blue', 'Red', 'Yellow', 'B', 1, 'This is the same color as the ocean on a sunny day'),
('What do bees make?', 'Milk', 'Honey', 'Silk', 'Wool', 'B', 1, 'This sweet golden substance is often used on toast or in tea'),
('How many wheels does a bicycle have?', 'One', 'Two', 'Three', 'Four', 'B', 1, 'A bicycle is different from a tricycle by having one less wheel'),

('What is 2 + 2?', 'Three', 'Four', 'Five', 'Six', 'B', 1, 'If you have two apples and get two more apples, you have this many'),
('Which animal is known as man''s best friend?', 'Cat', 'Dog', 'Horse', 'Bird', 'B', 1, 'This pet is known for loyalty, wagging tails, and barking'),
('What do you use to write on a chalkboard?', 'Pen', 'Pencil', 'Chalk', 'Marker', 'C', 1, 'This writing tool shares the same name as the board itself'),
('How many sides does a triangle have?', 'Two', 'Three', 'Four', 'Five', 'B', 1, 'The prefix "tri" means this number in Latin'),
('What is the color of grass?', 'Blue', 'Green', 'Red', 'Yellow', 'B', 1, 'This color is associated with nature, forests, and healthy plants'),

('Which planet do we live on?', 'Mars', 'Venus', 'Earth', 'Jupiter', 'C', 1, 'This planet is known as the "Blue Planet" and has one moon'),
('What do we call frozen water?', 'Steam', 'Ice', 'Rain', 'Snow', 'B', 1, 'Hockey is played on rinks made of this solid form of water'),
('How many months are in a year?', 'Ten', 'Eleven', 'Twelve', 'Thirteen', 'C', 1, 'Think of a dozen - that''s how many months we have'),
('What is the largest ocean on Earth?', 'Atlantic', 'Indian', 'Arctic', 'Pacific', 'D', 1, 'This ocean touches both Asia and the Americas'),
('Which fruit is red and grows on trees?', 'Banana', 'Apple', 'Orange', 'Grape', 'B', 1, 'This fruit is famously associated with Isaac Newton and gravity'),

('How many cents are in a dollar?', 'Fifty', 'Seventy-five', 'One hundred', 'One hundred twenty-five', 'C', 1, 'Ten dimes equals this many cents, same as four quarters'),
('What do we call a baby dog?', 'Kitten', 'Puppy', 'Cub', 'Calf', 'B', 1, 'This term for a young dog sounds similar to the word "pup"'),
('Which season comes after winter?', 'Summer', 'Spring', 'Fall', 'Autumn', 'B', 1, 'This season is when flowers bloom and birds return from migration'),
('What shape is a stop sign?', 'Circle', 'Square', 'Triangle', 'Octagon', 'D', 1, 'This eight-sided shape is unique among traffic signs'),
('How many hours are in a day?', 'Twelve', 'Twenty', 'Twenty-four', 'Thirty-six', 'C', 1, 'This is twice the number of hours from noon to midnight');
GO

-- LEVEL 2 QUESTIONS (Medium - Game Questions 6-10: $2,000 to $32,000)
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note) VALUES
('Who painted the Mona Lisa?', 'Vincent van Gogh', 'Leonardo da Vinci', 'Pablo Picasso', 'Claude Monet', 'B', 2, 'This Renaissance artist was also an inventor who designed flying machines'),
('What is the chemical symbol for gold?', 'Go', 'Gd', 'Au', 'Ag', 'C', 2, 'This symbol comes from the Latin word "aurum" meaning shining dawn'),
('In what year did World War II end?', '1943', '1944', '1945', '1946', 'C', 2, 'This was the year the atomic bombs were dropped on Japan'),
('How many strings does a standard guitar have?', 'Four', 'Five', 'Six', 'Seven', 'C', 2, 'This is the same number of sides as a hexagon'),
('What is the tallest mountain in the world?', 'K2', 'Kilimanjaro', 'Mount Everest', 'Denali', 'C', 2, 'This Himalayan peak is named after a British surveyor'),

('Who wrote "Romeo and Juliet"?', 'Charles Dickens', 'William Shakespeare', 'Mark Twain', 'Jane Austen', 'B', 2, 'This Elizabethan playwright also wrote Hamlet and Macbeth'),
('What is the speed of light approximately?', '186,000 mph', '186,000 km/s', '300,000 mph', '300,000 km/s', 'D', 2, 'This speed is measured in kilometers per second, not miles per hour'),
('Which country is home to the kangaroo?', 'New Zealand', 'Australia', 'South Africa', 'Brazil', 'B', 2, 'This continent-country is the world''s largest island'),
('What is the square root of 144?', 'Ten', 'Eleven', 'Twelve', 'Thirteen', 'C', 2, 'This number multiplied by itself equals 144, and it''s a dozen'),
('Who was the first President of the United States?', 'Thomas Jefferson', 'John Adams', 'George Washington', 'Benjamin Franklin', 'C', 2, 'This founding father led the Continental Army during the Revolution'),

('What is the smallest country in the world?', 'Monaco', 'Vatican City', 'San Marino', 'Liechtenstein', 'B', 2, 'This country is entirely surrounded by Rome and is headquarters of the Catholic Church'),
('How many bones are in the adult human body?', '186', '206', '226', '246', 'B', 2, 'This number is just over 200, specifically 6 more than 200'),
('What is the chemical formula for water?', 'CO2', 'H2O', 'O2', 'NaCl', 'B', 2, 'This molecule has two hydrogen atoms and one oxygen atom'),
('Which planet is known as the Red Planet?', 'Venus', 'Mars', 'Jupiter', 'Saturn', 'B', 2, 'This planet is named after the Roman god of war'),
('In which year did the Titanic sink?', '1910', '1911', '1912', '1913', 'C', 2, 'The "unsinkable" ship sank on its maiden voyage in April of this year'),

('What is the capital of Japan?', 'Osaka', 'Kyoto', 'Tokyo', 'Hiroshima', 'C', 2, 'This city hosted the Summer Olympics in 1964 and again in 2021'),
('How many teeth does an adult human typically have?', '28', '30', '32', '34', 'C', 2, 'This includes four wisdom teeth if they all develop'),
('What is the largest organ in the human body?', 'Liver', 'Brain', 'Heart', 'Skin', 'D', 2, 'This organ covers your entire body and protects you from the outside world'),
('Who developed the theory of relativity?', 'Isaac Newton', 'Albert Einstein', 'Nikola Tesla', 'Stephen Hawking', 'B', 2, 'This physicist with wild hair is famous for E=mc²'),
('What is the longest river in the world?', 'Amazon', 'Nile', 'Mississippi', 'Yangtze', 'B', 2, 'This African river flows through Egypt and into the Mediterranean Sea');
GO

-- LEVEL 3 QUESTIONS (Hard - Game Questions 11-14: $64,000 to $500,000)
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note) VALUES
('What is the only mammal capable of true flight?', 'Flying squirrel', 'Bat', 'Flying lemur', 'Sugar glider', 'B', 3, 'This nocturnal creature uses echolocation to navigate in darkness'),
('Which element has the atomic number 1?', 'Helium', 'Hydrogen', 'Oxygen', 'Nitrogen', 'B', 3, 'This lightest element makes up 75% of all matter in the universe'),
('Who wrote "1984"?', 'Aldous Huxley', 'Ray Bradbury', 'George Orwell', 'H.G. Wells', 'C', 3, 'This British author also wrote "Animal Farm" about revolutionary pigs'),
('In what year did the Berlin Wall fall?', '1987', '1988', '1989', '1990', 'C', 3, 'This happened the same year as the Tiananmen Square protests'),
('What is the hardest natural substance on Earth?', 'Granite', 'Steel', 'Diamond', 'Tungsten', 'C', 3, 'This gemstone rates 10 on the Mohs hardness scale'),

('How many symphonies did Beethoven compose?', 'Seven', 'Eight', 'Nine', 'Ten', 'C', 3, 'His final symphony included the "Ode to Joy" choral movement'),
('What is the rarest blood type?', 'O negative', 'AB positive', 'AB negative', 'B negative', 'C', 3, 'This blood type is found in less than 1% of the population'),
('Which country has the most time zones?', 'Russia', 'United States', 'Canada', 'France', 'D', 3, 'This country has 12 time zones due to its overseas territories'),
('What is the smallest bone in the human body?', 'Stapes', 'Malleus', 'Incus', 'Patella', 'A', 3, 'This tiny stirrup-shaped bone is located in the middle ear'),
('Who discovered penicillin?', 'Louis Pasteur', 'Marie Curie', 'Alexander Fleming', 'Jonas Salk', 'C', 3, 'This Scottish bacteriologist made his discovery by accident in 1928'),

('What is the capital of Mongolia?', 'Ulaanbaatar', 'Astana', 'Bishkek', 'Tashkent', 'A', 3, 'This city''s name means "Red Hero" in the Mongolian language'),
('How many keys are on a standard piano?', '76', '82', '88', '94', 'C', 3, 'This number consists of 52 white keys and 36 black keys'),
('What is the most abundant gas in Earth''s atmosphere?', 'Oxygen', 'Carbon dioxide', 'Nitrogen', 'Argon', 'C', 3, 'This gas makes up about 78% of our atmosphere'),
('Who painted "The Starry Night"?', 'Claude Monet', 'Vincent van Gogh', 'Paul Cezanne', 'Henri Matisse', 'B', 3, 'This Dutch post-impressionist artist famously cut off his own ear'),
('What is the largest desert in the world?', 'Sahara', 'Arabian', 'Antarctic', 'Gobi', 'C', 3, 'A desert is defined by precipitation, not temperature - this icy continent qualifies'),

('In which year did the French Revolution begin?', '1787', '1788', '1789', '1790', 'C', 3, 'The Storming of the Bastille happened on July 14th of this year'),
('What is the second most spoken language in the world?', 'English', 'Spanish', 'Mandarin Chinese', 'Hindi', 'B', 3, 'This Romance language is the official language in 20 countries'),
('How many chambers does a human heart have?', 'Two', 'Three', 'Four', 'Five', 'C', 3, 'This number includes two atria and two ventricles'),
('What is the currency of Switzerland?', 'Euro', 'Swiss Franc', 'Krone', 'Pound', 'B', 3, 'Switzerland is not in the EU and uses its own currency'),
('Who invented the telephone?', 'Thomas Edison', 'Nikola Tesla', 'Alexander Graham Bell', 'Guglielmo Marconi', 'C', 3, 'This Scottish-born inventor famously made the first call to his assistant Watson');
GO

-- LEVEL 4 QUESTIONS (Million Dollar - Game Question 15)
INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Note) VALUES
('What is the oldest continuously inhabited city in the world?', 'Athens', 'Damascus', 'Jerusalem', 'Jericho', 'B', 4, 'This Syrian city has been continuously inhabited since 11,000 BCE'),
('Who was the first woman to win a Nobel Prize?', 'Rosalind Franklin', 'Marie Curie', 'Dorothy Hodgkin', 'Barbara McClintock', 'B', 4, 'This Polish-French physicist discovered radium and polonium'),
('What is the longest-running Broadway show?', 'Cats', 'Les Miserables', 'The Phantom of the Opera', 'Chicago', 'C', 4, 'This Andrew Lloyd Webber musical ran for over 35 years'),
('In what year was the first email sent?', '1969', '1971', '1973', '1975', 'B', 4, 'Ray Tomlinson sent the first networked email two years after ARPANET launched'),
('What is the only letter that doesn''t appear in any US state name?', 'Q', 'X', 'Z', 'J', 'A', 4, 'This letter is the 17th in the alphabet and starts few English words'),

('Which Shakespeare play features the character Caliban?', 'The Tempest', 'Othello', 'Macbeth', 'Hamlet', 'A', 4, 'In this play, Caliban is the son of the witch Sycorax and lives on an island'),
('What is the smallest prime number?', '0', '1', '2', '3', 'C', 4, 'This is the only even prime number that exists'),
('Who was the first person to reach the South Pole?', 'Robert Falcon Scott', 'Roald Amundsen', 'Ernest Shackleton', 'Richard Byrd', 'B', 4, 'This Norwegian explorer arrived in December 1911, beating Scott by 34 days'),
('What is the most expensive spice in the world by weight?', 'Vanilla', 'Cardamom', 'Saffron', 'Cinnamon', 'C', 4, 'This spice comes from crocus flowers and requires 75,000 flowers per pound'),
('In which year was the first atom split?', '1917', '1919', '1932', '1938', 'C', 4, 'Cockcroft and Walton achieved this nuclear physics breakthrough in the early 1930s'),

('What is the national animal of Scotland?', 'Lion', 'Eagle', 'Unicorn', 'Dragon', 'C', 4, 'This mythical creature appears on the Scottish royal coat of arms'),
('Which planet has the most moons?', 'Jupiter', 'Saturn', 'Uranus', 'Neptune', 'B', 4, 'This ringed planet has 146 known moons, surpassing Jupiter'),
('What is the oldest known musical instrument?', 'Lyre', 'Flute', 'Drum', 'Harp', 'B', 4, 'A 40,000-year-old example made from bone was found in a German cave'),
('Who was the first computer programmer?', 'Alan Turing', 'Ada Lovelace', 'Grace Hopper', 'Charles Babbage', 'B', 4, 'This mathematician wrote algorithms for Charles Babbage''s Analytical Engine in the 1840s'),
('What is the deepest point in Earth''s oceans?', 'Puerto Rico Trench', 'Java Trench', 'Mariana Trench', 'Philippine Trench', 'C', 4, 'Challenger Deep in this trench is nearly 36,000 feet below sea level'),

('Which element is named after the Greek word for "sun"?', 'Gold', 'Helium', 'Radium', 'Uranium', 'B', 4, 'This noble gas was first discovered in the sun''s spectrum before being found on Earth'),
('What is the fastest land animal over long distances?', 'Cheetah', 'Pronghorn antelope', 'Springbok', 'Wildebeest', 'B', 4, 'While cheetahs are fastest in sprints, this North American animal wins marathons at 55 mph'),
('In which city was the first subway system opened?', 'Paris', 'New York', 'London', 'Berlin', 'C', 4, 'The Metropolitan Railway opened here in 1863, pioneering underground transit'),
('What is the only continent without an active volcano?', 'Europe', 'Australia', 'Antarctica', 'South America', 'B', 4, 'This continent-country has no recent volcanic activity despite its ancient geology'),
('Who wrote the original "Sherlock Holmes" stories?', 'Agatha Christie', 'Arthur Conan Doyle', 'Edgar Allan Poe', 'G.K. Chesterton', 'B', 4, 'This Scottish physician created the famous detective and his companion Watson');
GO

PRINT 'Questions table populated: 80 questions (20 per level)';
GO

-- ============================================================================
-- INSERT FFF_QUESTIONS DATA (44 Questions)
-- ============================================================================

PRINT 'Inserting FFF questions data...';
GO

INSERT INTO fff_questions (Question, A, B, C, D, CorrectAnswer, Used) VALUES
-- Geography & Size Ordering
('Put these planets in order from closest to farthest from the Sun', 'Mars', 'Venus', 'Earth', 'Mercury', 'DBCA', 0),
('Put these oceans by area (largest to smallest)', 'Indian', 'Pacific', 'Atlantic', 'Arctic', 'BCAD', 0),
('Put these continents by population (most to least)', 'Africa', 'Asia', 'Europe', 'North America', 'BACD', 0),
('Put these mountains by height (shortest to tallest)', 'K2', 'Mount Everest', 'Kilimanjaro', 'Denali', 'CDAB', 0),
('Put these cities by population (largest to smallest)', 'Paris', 'Tokyo', 'London', 'New York', 'BDCA', 0),

-- Historical Chronology
('Put these US Presidents in chronological order (oldest to newest)', 'Lincoln', 'Washington', 'Roosevelt', 'Kennedy', 'BACD', 0),
('Put these events in chronological order (oldest to newest)', 'Moon Landing', 'World War I', 'Fall of Berlin Wall', 'American Revolution', 'DBAC', 0),
('Put these inventions by year (oldest first)', 'Airplane', 'Telephone', 'Automobile', 'Light Bulb', 'BDCA', 0),
('Put these Olympiads in chronological order (oldest to newest)', 'Beijing 2008', 'Athens 2004', 'London 2012', 'Rio 2016', 'BACD', 0),
('Put these movies in order of release date (oldest first)', 'The Matrix', 'Star Wars', 'Avatar', 'Titanic', 'BDAC', 0),

-- Numerical & Alphabetical Ordering
('Put these numbers in ascending order', '50', '10', '25', '100', 'BCAD', 0),
('Put these letters in alphabetical order', 'Z', 'M', 'A', 'Q', 'CBDA', 0),
('Put these Roman numerals in ascending order', 'L', 'X', 'C', 'V', 'DBAC', 0),
('Put these fractions in ascending order', '3/4', '1/2', '1/4', '2/3', 'CBDA', 0),
('Put these negative numbers from smallest to largest', '-5', '-20', '-1', '-10', 'BDAC', 0),

-- Time & Calendar Ordering
('Put these seasons in order starting with Spring', 'Winter', 'Fall', 'Spring', 'Summer', 'CDBA', 0),
('Put these months in calendar order', 'October', 'January', 'July', 'April', 'BDCA', 0),
('Put these times of day in chronological order (earliest to latest)', 'Midnight', 'Noon', 'Dawn', 'Dusk', 'ACBD', 0),
('Put these life stages in order (youngest to oldest)', 'Adolescence', 'Infancy', 'Adulthood', 'Childhood', 'BDAC', 0),
('Put these time periods from shortest to longest', 'Year', 'Month', 'Decade', 'Week', 'DBAC', 0),

-- Scientific & Natural World
('Put these elements by atomic number (lowest to highest)', 'Iron', 'Hydrogen', 'Carbon', 'Oxygen', 'BCDA', 0),
('Put these animals by size (smallest to largest)', 'Horse', 'Mouse', 'Elephant', 'Dog', 'BDAC', 0),
('Put these colors in rainbow order (ROYGBIV)', 'Blue', 'Red', 'Yellow', 'Green', 'BCDA', 0),
('Put these planets by number of moons (fewest to most)', 'Earth', 'Mars', 'Jupiter', 'Venus', 'DABC', 0),
('Put these states of matter by temperature (coldest to hottest)', 'Gas', 'Liquid', 'Solid', 'Plasma', 'CBAD', 0),

-- Books & Literature
('Put these books by publication date (oldest first)', '1984', 'Moby Dick', 'Harry Potter', 'The Great Gatsby', 'BDAC', 0),
('Put these Harry Potter books in order (first to last)', 'Prisoner of Azkaban', 'Philosopher''s Stone', 'Goblet of Fire', 'Chamber of Secrets', 'BDCA', 0),
('Put these Shakespeare plays in alphabetical order', 'Othello', 'Hamlet', 'Macbeth', 'Romeo and Juliet', 'BCAD', 0),
('Put these classic authors by birth year (oldest first)', 'Charles Dickens', 'William Shakespeare', 'Jane Austen', 'Mark Twain', 'BCAD', 0),
('Put these book series by first publication (oldest to newest)', 'The Hunger Games', 'Lord of the Rings', 'Twilight', 'Chronicles of Narnia', 'DBAC', 0),

-- Technology & Innovation
('Put these tech companies by founding year (oldest first)', 'Google', 'Microsoft', 'Apple', 'Facebook', 'BCAD', 0),
('Put these gaming consoles in order of release (oldest to newest)', 'PlayStation', 'Atari 2600', 'Xbox', 'Nintendo Switch', 'BACD', 0),
('Put these Apple products in order of release (oldest first)', 'iPad', 'iPhone', 'Apple Watch', 'iPod', 'DBAC', 0),
('Put these social media platforms by launch year (oldest first)', 'Instagram', 'Facebook', 'TikTok', 'Twitter', 'BDAC', 0),
('Put these programming languages by creation year (oldest to newest)', 'Python', 'C', 'JavaScript', 'Java', 'BCDA', 0),

-- Music & Entertainment
('Put these musical notes in ascending pitch', 'E', 'C', 'G', 'A', 'BDCA', 0),
('Put these Beatles albums in order of release (oldest to newest)', 'Abbey Road', 'Help!', 'Let It Be', 'A Hard Day''s Night', 'DBAC', 0),
('Put these musical genres by approximate era of origin (oldest first)', 'Hip Hop', 'Jazz', 'Rock and Roll', 'Classical', 'DBCA', 0),
('Put these Disney movies in order of release (oldest first)', 'Frozen', 'The Lion King', 'Beauty and the Beast', 'Aladdin', 'CBDA', 0),
('Put these TV shows by premiere date (oldest to newest)', 'Breaking Bad', 'Friends', 'Game of Thrones', 'The Office (US)', 'BDCA', 0),

-- Sports & Games
('Put these Olympic sports by when they were added (oldest first)', 'Basketball', 'Swimming', 'Skateboarding', 'Tennis', 'BDAC', 0),
('Put these chess pieces by point value (lowest to highest)', 'Knight', 'Pawn', 'Rook', 'Queen', 'BACD', 0),
('Put these FIFA World Cups in chronological order (oldest to newest)', 'Brazil 2014', 'Germany 2006', 'South Africa 2010', 'Russia 2018', 'BCAD', 0),
('Put these Super Bowl winning teams in chronological order of first win', 'New England Patriots', 'Green Bay Packers', 'Dallas Cowboys', 'Pittsburgh Steelers', 'BCDA', 0),
('Put these track and field events by distance (shortest to longest)', '100m', 'Marathon', '400m', '5000m', 'ACDB', 0);
GO

PRINT 'FFF questions table populated: 44 questions';
GO

PRINT '============================================================================';
PRINT 'Database initialization complete!';
PRINT 'Tables created: questions (80 questions), fff_questions (44 questions)';
PRINT '============================================================================';
GO
