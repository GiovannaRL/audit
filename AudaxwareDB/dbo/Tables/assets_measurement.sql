CREATE TABLE [dbo].[assets_measurement] (
    [eq_unit_measure_id] INT          NOT NULL,
    [eq_unit_desc]       VARCHAR (40) NULL,
    CONSTRAINT [equipment_measurement_pk] PRIMARY KEY CLUSTERED ([eq_unit_measure_id] ASC)
);

