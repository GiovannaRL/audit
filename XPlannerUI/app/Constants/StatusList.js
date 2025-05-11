// Possibles status of a project
xPlanner.constant('StatusListProject', [
    { name: "Active", id: "A" },
    { name: "Canceled", id: "D" },
    { name: "Complete", id: "C" },
    { name: "Inventory", id: "I" },
    { name: "Locked", id: "L" },
    { name: "Pending", id: "P" },
    { name: "Training", id: "T" }
                    
]);

// Possibles status of an asset
xPlanner.constant('StatusListeditMulti', [
    { name: "Plan", value: "Plan" },
    { name: "Approved", value: "Approved" },
    { name: "Delivered", value: "Delivered" },
    { name: "Received", value: "Received" },
    { name: "Completed", value: "Completed" }
]);

// Types of IT Connections
xPlanner.constant('ConnectionTypeList', [
    { name: "Bluetooth", value: "Bluetooth" },
    { name: "Cat 6", value: "Cat6" },
    { name: "Displayport", value: "Displayport" },
    { name: "DVI", value: "DVI" },
    { name: "HDMI", value: "HDMI" },
    { name: "Wireless", value: "Wireless" }
]);