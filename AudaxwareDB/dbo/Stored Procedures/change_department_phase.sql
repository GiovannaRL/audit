
-- =============================================
-- Author:		Camila Silva
-- Create date: 12/29/2015
-- Description:	Change a equipment from phase
-- =============================================
CREATE PROCEDURE [dbo].[change_department_phase](@project_id integer, @old_phase_id integer, @department_id integer, @new_phase_id integer) 
--RETURNS TABLE
AS	
BEGIN
	SET NOCOUNT ON;
	
	UPDATE project_department SET phase_id = @new_phase_id WHERE project_id = @project_id AND phase_id = @old_phase_id AND department_id = @department_id;

END
