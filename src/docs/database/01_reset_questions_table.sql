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
('What is the capital of France?', 'London', 'Berlin', 'Paris', 'Madrid', 'C', 1, 'Range', 'This city is home to the Eiffel Tower and was called the "City of Light"'),
('How many days are in a week?', 'Five', 'Six', 'Seven', 'Eight', 'C', 1, 'Range', 'Think about how many days from Monday through Sunday'),
('What color is the sky on a clear day?', 'Green', 'Blue', 'Red', 'Yellow', 'B', 1, 'Range', 'This is the same color as the ocean on a sunny day'),
('What do bees make?', 'Milk', 'Honey', 'Silk', 'Wool', 'B', 1, 'Range', 'This sweet golden substance is often used on toast or in tea'),
('How many wheels does a bicycle have?', 'One', 'Two', 'Three', 'Four', 'B', 1, 'Range', 'A bicycle is different from a tricycle by having one less wheel'),

('What is 2 + 2?', 'Three', 'Four', 'Five', 'Six', 'B', 2, 'Range', 'If you have two apples and get two more apples, you have this many'),
('Which animal is known as man''s best friend?', 'Cat', 'Dog', 'Horse', 'Bird', 'B', 2, 'Range', 'This pet is known for loyalty, wagging tails, and barking'),
('What do you use to write on a chalkboard?', 'Pen', 'Pencil', 'Chalk', 'Marker', 'C', 2, 'Range', 'This writing tool shares the same name as the board itself'),
('How many sides does a triangle have?', 'Two', 'Three', 'Four', 'Five', 'B', 2, 'Range', 'The prefix "tri" means this number in Latin'),
('What is the color of grass?', 'Blue', 'Green', 'Red', 'Yellow', 'B', 2, 'Range', 'This color is associated with nature, forests, and healthy plants'),

('Which planet do we live on?', 'Mars', 'Venus', 'Earth', 'Jupiter', 'C', 3, 'Range', 'This planet is known as the "Blue Planet" and has one moon'),
('What do we call frozen water?', 'Steam', 'Ice', 'Rain', 'Snow', 'B', 3, 'Range', 'Hockey is played on rinks made of this solid form of water'),
('How many months are in a year?', 'Ten', 'Eleven', 'Twelve', 'Thirteen', 'C', 3, 'Range', 'Think of a dozen - that''s how many months we have'),
('What is the largest ocean on Earth?', 'Atlantic', 'Indian', 'Arctic', 'Pacific', 'D', 3, 'Range', 'This ocean touches both Asia and the Americas'),
('Which fruit is red and grows on trees?', 'Banana', 'Apple', 'Orange', 'Grape', 'B', 3, 'Range', 'This fruit is famously associated with Isaac Newton and gravity'),

('How many cents are in a dollar?', 'Fifty', 'Seventy-five', 'One hundred', 'One hundred twenty-five', 'C', 4, 'Range', 'Ten dimes equals this many cents, same as four quarters'),
('What do we call a baby dog?', 'Kitten', 'Puppy', 'Cub', 'Calf', 'B', 4, 'Range', 'This term for a young dog sounds similar to the word "pup"'),
('Which season comes after winter?', 'Summer', 'Spring', 'Fall', 'Autumn', 'B', 4, 'Range', 'This season is when flowers bloom and birds return from migration'),
('What shape is a stop sign?', 'Circle', 'Square', 'Triangle', 'Octagon', 'D', 4, 'Range', 'This eight-sided shape is unique among traffic signs'),
('How many hours are in a day?', 'Twelve', 'Twenty', 'Twenty-four', 'Thirty-six', 'C', 4, 'Range', 'This is twice the number of hours from noon to midnight');

-- ============================================================================
-- LEVEL 6-10 QUESTIONS (Medium - $2,000 to $32,000)
-- Difficulty_Type: Range
-- ============================================================================

INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Difficulty_Type, Note) VALUES
('Who painted the Mona Lisa?', 'Vincent van Gogh', 'Leonardo da Vinci', 'Pablo Picasso', 'Claude Monet', 'B', 6, 'Range', 'This Renaissance artist was also an inventor who designed flying machines'),
('What is the chemical symbol for gold?', 'Go', 'Gd', 'Au', 'Ag', 'C', 6, 'Range', 'This symbol comes from the Latin word "aurum" meaning shining dawn'),
('In what year did World War II end?', '1943', '1944', '1945', '1946', 'C', 6, 'Range', 'This was the year the atomic bombs were dropped on Japan'),
('How many strings does a standard guitar have?', 'Four', 'Five', 'Six', 'Seven', 'C', 6, 'Range', 'This is the same number of sides as a hexagon'),
('What is the tallest mountain in the world?', 'K2', 'Kilimanjaro', 'Mount Everest', 'Denali', 'C', 6, 'Range', 'This Himalayan peak is named after a British surveyor'),

('Who wrote "Romeo and Juliet"?', 'Charles Dickens', 'William Shakespeare', 'Mark Twain', 'Jane Austen', 'B', 7, 'Range', 'This Elizabethan playwright also wrote Hamlet and Macbeth'),
('What is the speed of light approximately?', '186,000 mph', '186,000 km/s', '300,000 mph', '300,000 km/s', 'D', 7, 'Range', 'This speed is measured in kilometers per second, not miles per hour'),
('Which country is home to the kangaroo?', 'New Zealand', 'Australia', 'South Africa', 'Brazil', 'B', 7, 'Range', 'This continent-country is the world''s largest island'),
('What is the square root of 144?', 'Ten', 'Eleven', 'Twelve', 'Thirteen', 'C', 7, 'Range', 'This number multiplied by itself equals 144, and it''s a dozen'),
('Who was the first President of the United States?', 'Thomas Jefferson', 'John Adams', 'George Washington', 'Benjamin Franklin', 'C', 7, 'Range', 'This founding father led the Continental Army during the Revolution'),

('What is the smallest country in the world?', 'Monaco', 'Vatican City', 'San Marino', 'Liechtenstein', 'B', 8, 'Range', 'This country is entirely surrounded by Rome and is headquarters of the Catholic Church'),
('How many bones are in the adult human body?', '186', '206', '226', '246', 'B', 8, 'Range', 'This number is just over 200, specifically 6 more than 200'),
('What is the chemical formula for water?', 'CO2', 'H2O', 'O2', 'NaCl', 'B', 8, 'Range', 'This molecule has two hydrogen atoms and one oxygen atom'),
('Which planet is known as the Red Planet?', 'Venus', 'Mars', 'Jupiter', 'Saturn', 'B', 8, 'Range', 'This planet is named after the Roman god of war'),
('In which year did the Titanic sink?', '1910', '1911', '1912', '1913', 'C', 8, 'Range', 'The "unsinkable" ship sank on its maiden voyage in April of this year'),

('What is the capital of Japan?', 'Osaka', 'Kyoto', 'Tokyo', 'Hiroshima', 'C', 9, 'Range', 'This city hosted the Summer Olympics in 1964 and again in 2021'),
('How many teeth does an adult human typically have?', '28', '30', '32', '34', 'C', 9, 'Range', 'This includes four wisdom teeth if they all develop'),
('What is the largest organ in the human body?', 'Liver', 'Brain', 'Heart', 'Skin', 'D', 9, 'Range', 'This organ covers your entire body and protects you from the outside world'),
('Who developed the theory of relativity?', 'Isaac Newton', 'Albert Einstein', 'Nikola Tesla', 'Stephen Hawking', 'B', 9, 'Range', 'This physicist with wild hair is famous for E=mc²'),
('What is the longest river in the world?', 'Amazon', 'Nile', 'Mississippi', 'Yangtze', 'B', 9, 'Range', 'This African river flows through Egypt and into the Mediterranean Sea');

-- ============================================================================
-- LEVEL 11-14 QUESTIONS (Hard - $64,000 to $500,000)
-- Difficulty_Type: Range
-- ============================================================================

INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Difficulty_Type, Note) VALUES
('What is the only mammal capable of true flight?', 'Flying squirrel', 'Bat', 'Flying lemur', 'Sugar glider', 'B', 11, 'Range', 'This nocturnal creature uses echolocation to navigate in darkness'),
('Which element has the atomic number 1?', 'Helium', 'Hydrogen', 'Oxygen', 'Nitrogen', 'B', 11, 'Range', 'This lightest element makes up 75% of all matter in the universe'),
('Who wrote "1984"?', 'Aldous Huxley', 'Ray Bradbury', 'George Orwell', 'H.G. Wells', 'C', 11, 'Range', 'This British author also wrote "Animal Farm" about revolutionary pigs'),
('In what year did the Berlin Wall fall?', '1987', '1988', '1989', '1990', 'C', 11, 'Range', 'This happened the same year as the Tiananmen Square protests'),
('What is the hardest natural substance on Earth?', 'Granite', 'Steel', 'Diamond', 'Tungsten', 'C', 11, 'Range', 'This gemstone rates 10 on the Mohs hardness scale'),

('How many symphonies did Beethoven compose?', 'Seven', 'Eight', 'Nine', 'Ten', 'C', 12, 'Range', 'His final symphony included the "Ode to Joy" choral movement'),
('What is the rarest blood type?', 'O negative', 'AB positive', 'AB negative', 'B negative', 'C', 12, 'Range', 'This blood type is found in less than 1% of the population'),
('Which country has the most time zones?', 'Russia', 'United States', 'Canada', 'France', 'D', 12, 'Range', 'This country has 12 time zones due to its overseas territories'),
('What is the smallest bone in the human body?', 'Stapes', 'Malleus', 'Incus', 'Patella', 'A', 12, 'Range', 'This tiny stirrup-shaped bone is located in the middle ear'),
('Who discovered penicillin?', 'Louis Pasteur', 'Marie Curie', 'Alexander Fleming', 'Jonas Salk', 'C', 12, 'Range', 'This Scottish bacteriologist made his discovery by accident in 1928'),

('What is the capital of Mongolia?', 'Ulaanbaatar', 'Astana', 'Bishkek', 'Tashkent', 'A', 13, 'Range', 'This city''s name means "Red Hero" in the Mongolian language'),
('How many keys are on a standard piano?', '76', '82', '88', '94', 'C', 13, 'Range', 'This number consists of 52 white keys and 36 black keys'),
('What is the most abundant gas in Earth''s atmosphere?', 'Oxygen', 'Carbon dioxide', 'Nitrogen', 'Argon', 'C', 13, 'Range', 'This gas makes up about 78% of our atmosphere'),
('Who painted "The Starry Night"?', 'Claude Monet', 'Vincent van Gogh', 'Paul Cézanne', 'Henri Matisse', 'B', 13, 'Range', 'This Dutch post-impressionist artist famously cut off his own ear'),
('What is the largest desert in the world?', 'Sahara', 'Arabian', 'Antarctic', 'Gobi', 'C', 13, 'Range', 'A desert is defined by precipitation, not temperature - this icy continent qualifies'),

('In which year did the French Revolution begin?', '1787', '1788', '1789', '1790', 'C', 14, 'Range', 'The Storming of the Bastille happened on July 14th of this year'),
('What is the second most spoken language in the world?', 'English', 'Spanish', 'Mandarin Chinese', 'Hindi', 'B', 14, 'Range', 'This Romance language is the official language in 20 countries'),
('How many chambers does a human heart have?', 'Two', 'Three', 'Four', 'Five', 'C', 14, 'Range', 'This number includes two atria and two ventricles'),
('What is the currency of Switzerland?', 'Euro', 'Swiss Franc', 'Krone', 'Pound', 'B', 14, 'Range', 'Switzerland is not in the EU and uses its own currency'),
('Who invented the telephone?', 'Thomas Edison', 'Nikola Tesla', 'Alexander Graham Bell', 'Guglielmo Marconi', 'C', 14, 'Range', 'This Scottish-born inventor famously made the first call to his assistant Watson');

-- ============================================================================
-- LEVEL 15 QUESTIONS (Million Dollar Questions)
-- Difficulty_Type: Range
-- ============================================================================

INSERT INTO questions (Question, A, B, C, D, CorrectAnswer, Level, Difficulty_Type, Note) VALUES
('What is the oldest continuously inhabited city in the world?', 'Athens', 'Damascus', 'Jerusalem', 'Jericho', 'B', 15, 'Range', 'This Syrian city has been continuously inhabited since 11,000 BCE'),
('Who was the first woman to win a Nobel Prize?', 'Rosalind Franklin', 'Marie Curie', 'Dorothy Hodgkin', 'Barbara McClintock', 'B', 15, 'Range', 'This Polish-French physicist discovered radium and polonium'),
('What is the longest-running Broadway show?', 'Cats', 'Les Misérables', 'The Phantom of the Opera', 'Chicago', 'C', 15, 'Range', 'This Andrew Lloyd Webber musical ran for over 35 years'),
('In what year was the first email sent?', '1969', '1971', '1973', '1975', 'B', 15, 'Range', 'Ray Tomlinson sent the first networked email two years after ARPANET launched'),
('What is the only letter that doesn''t appear in any US state name?', 'Q', 'X', 'Z', 'J', 'A', 15, 'Range', 'This letter is the 17th in the alphabet and starts few English words'),

('Which Shakespeare play features the character Caliban?', 'The Tempest', 'Othello', 'Macbeth', 'Hamlet', 'A', 15, 'Range', 'In this play, Caliban is the son of the witch Sycorax and lives on an island'),
('What is the smallest prime number?', '0', '1', '2', '3', 'C', 15, 'Range', 'This is the only even prime number that exists'),
('Who was the first person to reach the South Pole?', 'Robert Falcon Scott', 'Roald Amundsen', 'Ernest Shackleton', 'Richard Byrd', 'B', 15, 'Range', 'This Norwegian explorer arrived in December 1911, beating Scott by 34 days'),
('What is the most expensive spice in the world by weight?', 'Vanilla', 'Cardamom', 'Saffron', 'Cinnamon', 'C', 15, 'Range', 'This spice comes from crocus flowers and requires 75,000 flowers per pound'),
('In which year was the first atom split?', '1917', '1919', '1932', '1938', 'C', 15, 'Range', 'Cockcroft and Walton achieved this nuclear physics breakthrough in the early 1930s'),

('What is the national animal of Scotland?', 'Lion', 'Eagle', 'Unicorn', 'Dragon', 'C', 15, 'Range', 'This mythical creature appears on the Scottish royal coat of arms'),
('Which planet has the most moons?', 'Jupiter', 'Saturn', 'Uranus', 'Neptune', 'B', 15, 'Range', 'This ringed planet has 146 known moons, surpassing Jupiter'),
('What is the oldest known musical instrument?', 'Lyre', 'Flute', 'Drum', 'Harp', 'B', 15, 'Range', 'A 40,000-year-old example made from bone was found in a German cave'),
('Who was the first computer programmer?', 'Alan Turing', 'Ada Lovelace', 'Grace Hopper', 'Charles Babbage', 'B', 15, 'Range', 'This mathematician wrote algorithms for Charles Babbage''s Analytical Engine in the 1840s'),
('What is the deepest point in Earth''s oceans?', 'Puerto Rico Trench', 'Java Trench', 'Mariana Trench', 'Philippine Trench', 'C', 15, 'Range', 'Challenger Deep in this trench is nearly 36,000 feet below sea level'),

('Which element is named after the Greek word for "sun"?', 'Gold', 'Helium', 'Radium', 'Uranium', 'B', 15, 'Range', 'This noble gas was first discovered in the sun''s spectrum before being found on Earth'),
('What is the fastest land animal over long distances?', 'Cheetah', 'Pronghorn antelope', 'Springbok', 'Wildebeest', 'B', 15, 'Range', 'While cheetahs are fastest in sprints, this North American animal wins marathons at 55 mph'),
('In which city was the first subway system opened?', 'Paris', 'New York', 'London', 'Berlin', 'C', 15, 'Range', 'The Metropolitan Railway opened here in 1863, pioneering underground transit'),
('What is the only continent without an active volcano?', 'Europe', 'Australia', 'Antarctica', 'South America', 'B', 15, 'Range', 'This continent-country has no recent volcanic activity despite its ancient geology'),
('Who wrote the original "Sherlock Holmes" stories?', 'Agatha Christie', 'Arthur Conan Doyle', 'Edgar Allan Poe', 'G.K. Chesterton', 'B', 15, 'Range', 'This Scottish physician created the famous detective and his companion Watson');

GO

PRINT 'Questions table reset complete!';
PRINT 'Total questions inserted: 80 (20 per difficulty level)';
GO
