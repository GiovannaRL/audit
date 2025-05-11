
CREATE VIEW [dbo].[item_status_v] AS 
 SELECT a.project_id,
    a.department_id,
    sum(
        CASE
            WHEN d.quote_requested_date IS NOT NULL THEN COALESCE(d.po_qty, 0)
            ELSE 0
        END) AS item_quote_requested,
    sum(
        CASE
            WHEN d.quote_received_date IS NOT NULL THEN COALESCE(d.po_qty, 0)
            ELSE 0
        END) AS item_quote_received,
    sum(
        CASE
            WHEN d.po_requested_date IS NOT NULL THEN COALESCE(d.po_qty, 0)
            ELSE 0
        END) AS item_po_requested,
    sum(
        CASE
            WHEN d.po_received_date IS NOT NULL THEN COALESCE(d.po_qty, 0)
            ELSE 0
        END) AS item_po_received
   FROM project_department a
     LEFT JOIN ( SELECT b.po_id,
            b.project_id,
            b.po_number,
            b.quote_number,
            b.description,
            b.vendor_id,
            b.quote_requested_date,
            b.quote_received_date,
            b.po_requested_date,
            b.po_received_date,
            b.freight,
            b.warehouse,
            b.tax,
            b.warranty,
            b.misc,
            b.status,
            b.date_added,
            b.added_by,
            b.comment,
                CASE
                    WHEN em.eq_unit_desc = 'per sf' THEN
                    CASE
                        WHEN COALESCE(c.po_qty, 0) = 0 THEN 0
                        ELSE 1
                    END
                    ELSE COALESCE(c.po_qty, 0)
                END AS po_qty
           FROM purchase_order b,
            inventory_purchase_order c,
            assets e,
            assets_measurement em
          WHERE b.po_id = c.po_id AND b.project_id = c.project_id AND c.asset_id = e.asset_id AND e.eq_measurement_id = em.eq_unit_measure_id) d ON a.project_id = d.project_id
  GROUP BY a.project_id, a.department_id;



