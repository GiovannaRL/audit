CREATE FUNCTION [Security].[enterpriseAccessPredicateWithAudaxwareDomain]
(
	@domain_id smallint
)
RETURNS TABLE
WITH SCHEMABINDING
AS
    RETURN SELECT 1 AS accessResult
  WHERE 
	(
		SESSION_CONTEXT(N'domain_id') is not null 
		AND
		CURRENT_USER like 'dbo'
		AND
		(	
			CAST(SESSION_CONTEXT(N'domain_id') AS SMALLINT) = 1
			OR 
			@domain_id = CAST(SESSION_CONTEXT(N'domain_id') AS SMALLINT)
			OR
			(@domain_id = 1 AND CAST(SESSION_CONTEXT(N'show_audax') AS BIT) = 1)
		)
	)
	OR
	(
		CURRENT_USER not like 'dbo'
		AND
		(
			@domain_id = 1
			OR
			@domain_id = CAST(SUBSTRING(CURRENT_USER, CHARINDEX('_', CURRENT_USER)+1, 20) as SMALLINT)
		)
	)
