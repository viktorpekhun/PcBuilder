namespace PcBuilderApi.Utilities
{
    public static class SD
    {
        public enum ComponentType
        {
            Cpu,
            Gpu,
            Ram,
            Motherboard,
            CpuCooler,
            PcCase,
            PowerSupply,
            Ssd,
            Hdd,
            Fan
        }

        public enum CompatibilityMessageType
        {
            Problem,
            Warning
        }
    }
}
