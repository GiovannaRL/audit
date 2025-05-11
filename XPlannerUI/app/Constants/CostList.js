// Possibles costs of a project
xPlanner.constant('CostListProject', [
                    { name: "Min", id: "min_cost" },
                    { name: "Max", id: "max_cost" },
                    { name: "Avg", id: "avg_cost" },
                    { name: "Last", id: "last_cost" }
]);

// Possibles costs of a asset in a asset_inventory
xPlanner.constant('CostFieldList', [
    { name: "Project Default", value: "default" },
    { name: "Min", value: "min_cost" },
    { name: "Max", value: "max_cost" },
    { name: "Average", value: "avg_cost" },
    { name: "Last", value: "last_cost" }
]);

// Possibles costs of a asset in a asset_inventory
xPlanner.constant('CostFieldListWithSource', [
    { name: "Project Default", value: "default" },
    { name: "Source Project", value: "source" },
    { name: "Min", value: "min_cost" },
    { name: "Max", value: "max_cost" },
    { name: "Average", value: "avg_cost" },
    { name: "Last", value: "last_cost" }
]);

// Possibles costs of a asset in a asset_inventory with none value
xPlanner.constant('CostFieldListWithNone', [
    { name: 'None', value: null },
    { name: "Project Default", value: "default" },
    { name: "Min", value: "min_cost" },
    { name: "Max", value: "max_cost" },
    { name: "Average", value: "avg_cost" },
    { name: "Last", value: "last_cost" }
]);

xPlanner.constant('CostFieldListReplace', [
    { name: "Project Default", value: "default" },
    { name: "Min", value: "min_cost" },
    { name: "Max", value: "max_cost" },
    { name: "Average", value: "avg_cost" },
    { name: "Last", value: "last_cost" },
    { name: "Keep current one", value: "" }
]);