CREATE PROCEDURE [dbo].[copy_inventory_budget_values]
	@domain_id SMALLINT,
    @curr_project_id INTEGER,
    @curr_phase_id INTEGER,
    @curr_department_id INTEGER,
	@curr_room_id INTEGER,
	@curr_inventory_id INTEGER,
	@to_domain INTEGER,
	@to_project_id INTEGER,
	@to_phase_id INTEGER,
	@to_department_id INTEGER,
	@to_room_id INTEGER,
	@to_inventory_id INTEGER
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE target SET
		target.unit_budget = source.unit_budget, target.locked_unit_budget = source.locked_unit_budget,
		target.asset_profile_budget = source.asset_profile_budget, target.detailed_budget = source.detailed_budget,
		target.unit_markup = source.unit_markup, target.unit_freight_markup = source.unit_freight_markup,
		target.unit_freight_net = source.unit_freight_net, target.unit_escalation = source.unit_escalation,
		target.unit_install_net = source.unit_install_net, target.unit_install_markup = source.unit_install_markup,
		target.unit_tax = source.unit_tax
	FROM project_room_inventory AS target,
		project_room_inventory AS source
	WHERE source.inventory_id = @curr_inventory_id AND source.domain_id = @domain_id
		AND target.inventory_id = @to_inventory_id AND target.domain_id = @to_domain;
END
