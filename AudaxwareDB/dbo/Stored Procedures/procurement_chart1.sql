
CREATE PROCEDURE [dbo].[procurement_chart1](
    @project_id integer,
    @phase_id integer,
    @department_id integer,
    @cost_center_id integer,
    @domain_id1 integer,
	@domain_id2 integer)
  AS
BEGIN

declare
    @v_start_date date,
    @v_end_date date,
    @period_end_date date,
    @projected_budget decimal(28,16),
    @po_amt decimal(28,16),
    @pct_committed decimal(28,16),
    @equip_purchased decimal(28,16),
    @equip_budgeted decimal(28,16),
    @pct_purchased decimal(28,16),
    @max_date_a date,
    @max_date_b date,
    @max_date_c date,
    @max_date_d date;

	declare @return_table table(
		period_end_date date,
		pct_committed decimal(28,16),
		pct_purchased decimal(28,16)
	);

    if @department_id is not null or @phase_id is not null or @cost_center_id is not null BEGIN
        select @projected_budget = coalesce((sum(a.total_budget_amt) + sum(-a.buyout_delta)), 0)
        from inventory_w_relo_v a
            inner join project_department b on a.project_id = b.project_id and a.department_id = b.department_id
        where 
            a.project_id = @project_id
            and (@phase_id = null or b.phase_id = @phase_id)
            and (@department_id = null or a.department_id = @department_id)
            and (@cost_center_id = null or a.cost_center_id = @cost_center_id)
            and a.asset_domain_id IN (@domain_id1, @domain_id2);
	end
    else begin
        select @projected_budget = coalesce(((sum(b.total_budget_amt) + sum(-b.buyout_delta)) + (min(a.freight_projected) + min(a.warehouse_projected) + min(a.tax_projected) + min(a.misc_projected))), 0)
        from ancillary_v a
            inner join inventory_w_relo_v b on a.project_id = b.project_id and b.domain_id in (@domain_id1, @domain_id2)
        where b.type = 'NEW'
            and a.project_id = @project_id;
    end;
	
    select @equip_budgeted = coalesce(sum(a.budget_qty_sf - a.dnp_qty_sf), 0)
    from inventory_w_relo_v a
        inner join project_department b on a.project_id = b.project_id and a.department_id = b.department_id
    where
         a.project_id = @project_id
        and (@phase_id is null or b.phase_id = @phase_id)
        and (@department_id is null or a.department_id = @department_id)
        and (@cost_center_id is null or a.cost_center_id = @cost_center_id)
        and a.domain_id in (@domain_id1, @domain_id2);

    select @v_start_date = min(po.quote_requested_date), 
			@max_date_a = max(po.po_received_date), 
			@max_date_b = max(po.po_requested_date), 
			@max_date_c = max(po.quote_received_date), 
			@max_date_d = max(po.quote_requested_date)
    from inventory_purchase_order ipo
		inner join purchase_order po on ipo.po_id = po.po_id and ipo.po_domain_id = po.domain_id and ipo.project_id = po.project_id
        inner join project_room_inventory pri on ipo.inventory_id = pri.inventory_id
    where ipo.project_id = @project_id
        and (@phase_id is null or pri.phase_id = @phase_id)
        and (@department_id is null or pri.department_id = @department_id)
        and (@cost_center_id is null or pri.cost_center_id = @cost_center_id)
        and ipo.asset_domain_id in(@domain_id1, @domain_id2);
		
    set @period_end_date = @v_start_date;

    set @v_end_date = @max_date_a;

    if @v_end_date is null or @max_date_b > @v_end_date begin
        set @v_end_date = @max_date_b;
    end;

    if @v_end_date is null or @max_date_c > @v_end_date begin
        set @v_end_date = @max_date_c;
    end;

    if @v_end_date is null or @max_date_d > @v_end_date begin
        set @v_end_date = @max_date_d;
    end;

    while (@period_end_date <= @v_end_date) begin
        select @po_amt = coalesce(sum(base.po_subtotal + base.ipo_subtotal), 0),
            @equip_purchased = coalesce(sum(base.po_qty), 0)    
        from
            (
                select ipo.project_id, pri.phase_id, pri.department_id, po.po_id, sum(CASE WHEN em.eq_unit_desc = 'per sf' THEN CASE WHEN COALESCE(ipo.po_qty, 0) = 0 THEN 0 ELSE 1 END ELSE COALESCE(ipo.po_qty, 0) END) as po_qty, sum(ipo.po_qty * ipo.po_unit_amt) as ipo_subtotal, (min(po.freight) + min(po.warehouse) + min(po.tax) + /*min(po.warranty) +*/ min(po.misc)) as po_subtotal
                from inventory_purchase_order ipo
					inner join purchase_order po on ipo.po_id = po.po_id and ipo.po_domain_id = po.domain_id and ipo.project_id = po.project_id
                    inner join project_room_inventory pri on ipo.inventory_id = pri.inventory_id
                    inner join assets e on e.asset_id = ipo.asset_id and e.domain_id = ipo.asset_domain_id
                    inner join assets_measurement em on e.eq_measurement_id = em.eq_unit_measure_id
                where ipo.project_id = @project_id
                    and (@phase_id is null or pri.phase_id = @phase_id)
                    and (@department_id is null or pri.department_id = @department_id)
                    and (@cost_center_id is null or pri.cost_center_id = @cost_center_id)
                    and po.quote_received_date between @v_start_date and @period_end_date
                    and ipo.asset_domain_id in(@domain_id1, @domain_id2)
                group by ipo.project_id, pri.phase_id, pri.department_id, po.po_id
            ) base;

        if @projected_budget = 0 begin 
			set @pct_committed = 0;
		end
        else begin 
			set @pct_committed = (@po_amt/@projected_budget) * 100;
        end;

        if @equip_budgeted = 0 begin
            set @pct_purchased = 0;
		end
        else begin
			set @pct_purchased = (@equip_purchased/@equip_budgeted) * 100;
        end;

		insert into @return_table values(@period_end_date, @pct_committed, @pct_purchased);
        
        set @period_end_date = DateADD(day, 1, @period_end_date);
    end;

	select * from @return_table;

    END
