
CREATE PROCEDURE update_cost(
	@po_domain_id SMALLINT,
	@project_id INTEGER,
	@asset_domain_id SMALLINT,
	@asset_id INTEGER,
	@po_id INTEGER,
	@inventory_id INTEGER,
	@unit_amt NUMERIC(10,2)
)	
AS
BEGIN

	-- options
	DECLARE @op_domain_id SMALLINT, @op_id INTEGER, @op_unit_price NUMERIC(10, 2), @op_min NUMERIC(10, 2), @op_max NUMERIC(10, 2), 
		@op_avg NUMERIC(10, 2), @op_new_min NUMERIC(10, 2);

	--DECLARE @domain_id SMALLINT, @project_id INTEGER, @asset_domain_id INTEGER, @asset_id INTEGER, @po_id INTEGER, @unit_amt NUMERIC(10, 2),
		--@inventory_id INTEGER;

	--DECLARE [cursor] CURSOR LOCAL FOR SELECT DISTINCT po_domain_id, project_id, asset_domain_id, asset_id, po_id, po_unit_amt, inventory_id
		--FROM inserted;
	--OPEN [cursor];
	--FETCH NEXT FROM [cursor] INTO @domain_id, @project_id, @asset_domain_id, @asset_id, @po_id, @unit_amt, @inventory_id;
	--WHILE @@FETCH_STATUS = 0
		--BEGIN

			IF (@unit_amt > 0)
				BEGIN

					DECLARE @exist_count INTEGER, @return_val INTEGER, @t_min NUMERIC(10, 2), @t_max NUMERIC(10, 2), @t_avg NUMERIC(10, 2), @t_vendor_id INTEGER;
					DECLARE @t_vendor_domain_id INTEGER, @t_stddev NUMERIC(10, 2), @t_curr_avg NUMERIC(10, 2), @t_curr_min NUMERIC(10, 2);
					DECLARE @t_curr_max NUMERIC(10, 2), @t_count NUMERIC(10, 2), @t_last_purchase_date DATE;

					SELECT @t_curr_min = COALESCE(min_cost,0), @t_curr_max = COALESCE(max_cost,0), @t_curr_avg = COALESCE(avg_cost,0)
					FROM assets
					WHERE asset_id = @asset_id and domain_id = @asset_domain_id;


					SELECT @t_count = COUNT(*) FROM inventory_purchase_order a, purchase_order b
					WHERE a.po_id = b.po_id AND a.po_domain_id = @po_domain_id AND a.asset_id = @asset_id AND a.asset_domain_id = @asset_domain_id
						AND a.project_id = @project_id AND po_unit_amt > 0;

					IF (@t_count > 0)
						BEGIN
							IF (@unit_amt > @t_curr_avg)
								BEGIN
									SET @t_min = @unit_amt;
									SET @t_max = @t_min + (@t_curr_max - @t_curr_min);

									UPDATE assets SET min_cost = @t_min, max_cost = @t_max, avg_cost = (@t_min + @t_max) / 2, last_cost = @unit_amt,
											last_budget_update = GETDATE()
									WHERE asset_id = @asset_id and domain_id = @asset_domain_id;
								END
							ELSE
								BEGIN
									UPDATE assets SET  last_cost = @unit_amt, last_budget_UPDATE = GETDATE()
									WHERE asset_id = @asset_id and domain_id = @ASSET_domain_id;
								END

							-- options
							DECLARE [options_cursor] CURSOR LOCAL FOR SELECT ino.domain_id, option_id, unit_price, min_cost, max_cost, avg_cost
								FROM inventory_options ino, assets_options ao 
								WHERE ino.domain_id = ao.domain_id AND ino.option_id = ao.asset_option_id AND inventory_id = @inventory_id;
							OPEN [options_cursor];
							FETCH NEXT FROM [options_cursor] INTO @op_domain_id, @op_id, @op_unit_price;
							WHILE @@FETCH_STATUS = 0
								BEGIN
									IF (@op_unit_price > 0)
										BEGIN
											IF (@op_unit_price > @op_avg)
												BEGIN
													SET @op_new_min = @op_unit_price;
													SET @op_max = @op_new_min + (@op_max - @op_min);

													UPDATE assets_options SET min_cost = @op_new_min, max_cost = @op_max, avg_cost =
														(@op_new_min + @op_max) / 2, last_cost = @op_unit_price
													WHERE domain_id = @op_domain_id AND asset_option_id = @op_id;
												END
											ELSE
												UPDATE assets_options SET last_cost = @op_unit_price
												WHERE domain_id = @op_domain_id AND asset_option_id = @op_id;

										END

									FETCH NEXT FROM [options_cursor] INTO @op_domain_id, @op_id, @op_unit_price;
								END
							CLOSE [options_cursor];
							DEALLOCATE [options_cursor];
						END

						-- insert/update vendor asset

					SELECT @t_vendor_id = vendor_id, @t_vendor_domain_id = vendor_domain_id
					FROM purchase_order
					WHERE domain_id = @po_domain_id AND po_id = @po_id AND project_id = @project_id;

					SELECT @exist_count = COUNT(*) FROM assets_vendor
					WHERE vendor_id = @t_vendor_id AND vendor_domain_id = @t_vendor_domain_id AND asset_id = @asset_id AND asset_domain_id = 
						@asset_domain_id;

					IF (@exist_count = 0)
						INSERT INTO assets_vendor(asset_id, asset_domain_id, vendor_id, vendor_domain_id, min_cost,max_cost,avg_cost,last_cost,date_added,added_by)
							VALUES(@asset_id, @asset_domain_id, @t_vendor_id, @t_vendor_domain_id, @unit_amt, @unit_amt, @unit_amt, @unit_amt, GETDATE(),'system');
					ELSE
						BEGIN
							SELECT @t_min = COALESCE(min(po_unit_amt),0), @t_max = COALESCE(max(po_unit_amt),0), @t_avg = COALESCE(avg(po_unit_amt),0)
							FROM inventory_purchase_order
							WHERE asset_id = @asset_id AND asset_domain_id = @asset_domain_id AND po_id 
								IN(SELECT po_id FROM purchase_order WHERE vendor_id = @t_vendor_id AND vendor_domain_id = @t_vendor_domain_id);

							UPDATE assets_vendor
							SET min_cost = @t_min, max_cost = @t_max, avg_cost = @t_avg, last_cost = @unit_amt
							WHERE vendor_id = @t_vendor_id and vendor_domain_id = @t_vendor_domain_id AND asset_id = @asset_id AND asset_domain_id = 
								@asset_domain_id;
						END
			END
			--FETCH NEXT FROM [cursor] INTO @domain_id, @project_id, @asset_domain_id, @asset_id, @po_id, @unit_amt, @inventory_id;
		--END
	--CLOSE [cursor];
	--DEALLOCATE [cursor];
END

