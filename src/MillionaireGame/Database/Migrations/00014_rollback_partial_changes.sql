-- Migration 00014: Rollback partial 00015 application
-- The previous migration partially applied - this cleans up those changes

-- Remove AnswerLabel straps that were inserted before the failure
DELETE FROM ThemeStraps WHERE StrapType = 'AnswerLabel';

PRINT 'Removed partially created AnswerLabel straps';

-- Drop the new StrapType constraint that was added
DECLARE @ConstraintName NVARCHAR(200);
SELECT @ConstraintName = name 
FROM sys.check_constraints 
WHERE parent_object_id = OBJECT_ID('ThemeStraps') 
  AND definition LIKE '%AnswerLabel%';

IF @ConstraintName IS NOT NULL
BEGIN
    DECLARE @SQL NVARCHAR(500) = 'ALTER TABLE ThemeStraps DROP CONSTRAINT ' + QUOTENAME(@ConstraintName);
    EXEC sp_executesql @SQL;
    PRINT 'Dropped new StrapType constraint: ' + @ConstraintName;
END
GO

-- Re-add original constraint without AnswerLabel
ALTER TABLE ThemeStraps 
ADD CONSTRAINT CK_ThemeStraps_StrapType_Original 
CHECK (StrapType IN ('Question', 'Answer', 'MoneyAmount', 'PlayerName', 'HostMessage'));
GO

PRINT 'Restored original StrapType constraint';

PRINT 'Migration 00014 completed: Rolled back partial changes from failed 00015 attempt';
GO
