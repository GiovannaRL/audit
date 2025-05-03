CREATE FUNCTION [dbo].[fn_get_project_comparison_column]
(
	@project_id AS INT,
	@copy_link as uniqueidentifier
)
RETURNS @comparison table
(
	pfd_budget_qty int,
	pfd_resp varchar(10),
	drawing_room_number varchar(50),
	drawing_room_name varchar(100)
)
as
BEGIN
	DECLARE @budget_qty int,
	@resp varchar(10),
	@drawing_room_number varchar(50),
	@drawing_room_name varchar(100)
	
		select @resp = resp, @budget_qty = budget_qty, @drawing_room_number = pr.drawing_room_number, @drawing_room_name = pr.drawing_room_name from project_room_inventory pri 
		inner join project_room pr on pr.project_id = pri.project_id and pr.phase_id = pri.phase_id and pr.department_id = pri.department_id and pr.room_id = pri.room_id and pr.domain_id = pri.domain_id
		where pri.project_id = @project_id and pri.copy_link = @copy_link

		INSERT INTO @comparison values(@budget_qty, @resp, @drawing_room_number, @drawing_room_name)
	
	return;
END
GO
