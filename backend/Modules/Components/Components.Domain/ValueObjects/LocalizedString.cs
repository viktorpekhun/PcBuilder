namespace Components.Domain.ValueObjects
{
    public class LocalizedString
    {
        public string Uk { get; set; } = string.Empty;
        public string? En { get; set; }

        public override string ToString()
        {
            string name = $"{this.Uk}";
            return name;
        }
    }
}
