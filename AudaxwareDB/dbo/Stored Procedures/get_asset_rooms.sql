
CREATE PROCEDURE dbo.get_asset_rooms
    @ProjectId       INT,
    @DomainId        INT,
    @AssetIds        VARCHAR(1000),
    @PhaseId         INT = -1,
    @DepartmentId    INT = -1,
    @RoomId          INT = -1
AS
BEGIN
    SET NOCOUNT ON;

    -- ========================================================================================
    -- Purpose: Returns info about all rooms to which one or more assets belong
    -- ========================================================================================

    SELECT 
        pri.serial_number,
        pri.serial_name,
        pp.description AS phase_desc,
        pri.project_id,
        pri.department_id,
        pd.phase_id,
        pd.description AS dept_desc,
        pri.room_id,
        pri.asset_id,
        a.asset_code,
        pri.asset_description,
        pri.current_location,
        COALESCE(NULLIF(pr.drawing_room_number, '') + '-', '') + pr.drawing_room_name AS drawing_room_name,
        budget_qty,
        SUM(pri.lease_qty) AS lease_qty,
        pri.resp,
        SUM(pri.dnp_qty) AS dnp_qty,
        SUM(pri.total_budget_amt) AS total_budget_amt,
        COALESCE(po.status, 'None') AS po_status,
        coalesce(pri.tag, '') AS tag 
    FROM 
        project_room_inventory pri
        INNER JOIN assets a 
            ON pri.asset_id = a.asset_id 
           AND pri.asset_domain_id = a.domain_id
        INNER JOIN project_room pr 
            ON pri.project_id = pr.project_id 
           AND pri.room_id = pr.room_id
        INNER JOIN project_department pd 
            ON pri.project_id = pd.project_id 
           AND pri.department_id = pd.department_id
        INNER JOIN project_phase pp 
            ON pd.project_id = pp.project_id 
           AND pd.phase_id = pp.phase_id
        LEFT JOIN inventory_purchase_order ipo 
            ON pri.project_id = ipo.project_id 
           AND pri.asset_id = ipo.asset_id 
           AND pri.asset_domain_id = ipo.asset_domain_id
        LEFT JOIN purchase_order po 
            ON ipo.po_id = po.po_id 
           AND ipo.project_id = po.project_id
    WHERE 
        pri.project_id = @ProjectId
        AND pri.domain_id = @DomainId
        AND CONCAT(pri.asset_id, ',', pri.asset_domain_id) IN (SELECT value FROM string_split(@AssetIds, ';'))
        AND (@PhaseId = -1 OR pp.phase_id = @PhaseId)
        AND (@DepartmentId = -1 OR pri.department_id = @DepartmentId)
        AND (@RoomId = -1 OR pri.room_id = @RoomId)
    GROUP BY 
        pri.serial_number,
        pri.serial_name,
        pp.description,
        pri.project_id,
        pri.department_id,
        pd.phase_id,
        pd.description,
        pri.room_id,
        pr.drawing_room_number,
        pr.drawing_room_name,
        pri.asset_id,
        a.asset_code,
        pri.asset_description,
        budget_qty,
        pri.current_location,
        pri.resp,
        po.status,
        pri.tag
    ORDER BY 
        pp.description,
        pd.description,
        pr.drawing_room_number,
        pr.drawing_room_name;
END;
GO


