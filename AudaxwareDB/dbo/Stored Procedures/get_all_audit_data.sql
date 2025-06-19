CREATE PROCEDURE [dbo].[get_all_audit_data]
	@domainId int,
	@dateFrom datetime = null, 
	@dateAt datetime = null
AS
	SELECT audit_log_id, b.UserName as username, operation, table_name, table_pk, a.comment, original, modified, header, modified_date, 
	p.project_description, c.asset_code 
	FROM audit_log (NOLOCK) a
	INNER JOIN AspNetUsers (NOLOCK) b on b.Id = a.user_id
	LEFT JOIN project (NOLOCK) p on p.project_id = a.project_id AND p.domain_id = a.domain_id
	LEFT JOIN assets (NOLOCK) c on c.asset_id = a.asset_id and c.domain_id = a.asset_domain_id
	WHERE a.domain_id = @domainId AND 
	(
		(@dateFrom is null AND a.modified_date > DATEADD(day,-90,Getdate())) 
		OR 
		(@dateFrom is not null AND a.modified_date between @dateFrom AND  @dateAt)
	)
	ORDER BY a.modified_date desc



