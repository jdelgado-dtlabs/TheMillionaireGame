-- Cleanup duplicate rounds that were created due to multiple StartNewRound calls
-- Keep only the first round for each SessionId + RoundNumber combination

-- Find duplicates
SELECT SessionId, RoundNumber, COUNT(*) as DuplicateCount
FROM GameRounds
GROUP BY SessionId, RoundNumber
HAVING COUNT(*) > 1;

-- Delete duplicates, keeping only the one with the lowest RoundId (first created)
WITH RoundCTE AS (
    SELECT 
        RoundId,
        ROW_NUMBER() OVER (PARTITION BY SessionId, RoundNumber ORDER BY RoundId ASC) as RowNum
    FROM GameRounds
)
DELETE FROM GameRounds
WHERE RoundId IN (
    SELECT RoundId FROM RoundCTE WHERE RowNum > 1
);

PRINT 'Duplicate rounds cleaned up';
