export const filterConfigs = {
    cpu: {
        title: "CPU Filters",
        filters: [
            {
                id: "manufacturer",
                label: "Manufacturer",
                type: "checkbox",
                options: ["Intel", "AMD"],
                property: "brand"
            },
            {
                id: "coreCount",
                label: "Core Count",
                type: "checkbox",
                options: ["2", "4", "6", "8"],
                property: "cores"
            },
            {
                id: "frequency",
                label: "Frequency (GHz)",
                type: "range",
                min: 1,
                max: 5,
                step: 0.1,
                property: "maxFrequency"
            }
        ]
    },
    gpu: {
        title: "GPU Filters",
        filters: [
            {
                id: "manufacturer",
                label: "Manufacturer",
                type: "checkbox",
                options: ["NVIDIA", "AMD", "Intel"],
                property: "manufacturer"
            },
            {
                id: "memory",
                label: "VRAM (GB)",
                type: "dropdown",
                options: [4, 6, 8, 12, 16, 24],
                property: "memory"
            }
        ]
    },
    // Add more component types and their filters
    motherboard: {
        title: "Motherboard Filters",
        filters: [
            {
                id: "socketType",
                label: "Socket Type",
                type: "checkbox",
                options: ["Socket AM4", "Socket AM5", "Socket LGA1700", "LGA1200"],
                property: "socket"
            },
            // Add more filters
        ]
    }
    // Continue for other component types...
};