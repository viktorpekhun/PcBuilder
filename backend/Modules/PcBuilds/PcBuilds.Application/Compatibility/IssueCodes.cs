namespace PcBuilds.Application.Compatibility
{
    public static class IssueCodes
    {
        public const string SocketMismatch = "SOCKET_MISMATCH";
        public const string SocketDataMissing = "SOCKET_DATA_MISSING";

        public const string CoolerTdpInsufficient = "COOLER_TDP_INSUFFICIENT";
        public const string CoolerTdpDataMissing = "COOLER_TDP_DATA_MISSING";
        public const string CoolerSocketMismatch = "COOLER_SOCKET_MISMATCH";
        public const string CoolerSocketDataMissing = "COOLER_SOCKET_DATA_MISSING";
        public const string CoolerCaseFit = "COOLER_CASE_FIT";
        public const string CoolerCaseDataMissing = "COOLER_CASE_DATA_MISSING";

        public const string GpuPcieVersion = "GPU_PCIE_VERSION";
        public const string GpuPcieLane = "GPU_PCIE_LANE";
        public const string GpuLengthExceedsCase = "GPU_LENGTH_EXCEEDS_CASE";
        public const string GpuLengthDataMissing = "GPU_LENGTH_DATA_MISSING";

        public const string RamTypeMismatch = "RAM_TYPE_MISMATCH";
        public const string RamFrequencyExceeded = "RAM_FREQUENCY_EXCEEDED";
        public const string RamSlotsExceeded = "RAM_SLOTS_EXCEEDED";
        public const string RamCapacityExceeded = "RAM_CAPACITY_EXCEEDED";
        public const string RamMixedFrequencies = "RAM_MIXED_FREQUENCIES";

        public const string SataSlotsMissing = "SATA_SLOTS_MISSING";
        public const string SataSlotsExceeded = "SATA_SLOTS_EXCEEDED";
        public const string M2SlotsMissing = "M2_SLOTS_MISSING";
        public const string M2SlotsExceeded = "M2_SLOTS_EXCEEDED";
        public const string NvmeVersionMismatch = "NVME_VERSION_MISMATCH";

        public const string FanHeadersInsufficient = "FAN_HEADERS_INSUFFICIENT";
        public const string FanCaseSlotsInsufficient = "FAN_CASE_SLOTS_INSUFFICIENT";
        public const string FanCaseDataMissing = "FAN_CASE_DATA_MISSING";

        public const string PsuGpuConnectorMissing = "PSU_GPU_CONNECTOR_MISSING";
        public const string PsuCpuConnectorMissing = "PSU_CPU_CONNECTOR_MISSING";
        public const string PsuInsufficient = "PSU_INSUFFICIENT";
        public const string PsuOverkill = "PSU_OVERKILL";

        public const string CpuBottleneck = "CPU_BOTTLENECK";
        public const string GpuBottleneck = "GPU_BOTTLENECK";

        public const string BudgetTooLowForScenario = "BUDGET_TOO_LOW_FOR_SCENARIO";
        public const string InvalidFormFactor = "INVALID_FORM_FACTOR";
        public const string NoCompatibleBuildFound = "NO_COMPATIBLE_BUILD_FOUND";
        public const string NoViableCorePairing = "NO_VIABLE_CORE_PAIRING";
    }
}
