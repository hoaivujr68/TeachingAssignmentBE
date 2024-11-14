namespace TeachingAssignmentApp.Helper
{
    public class ProjectHourEnumItem
    {
        public ProjectHourEnumItem(string name, string type, double value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public double Value { get; set; }
    }

    public class ProjectHourEnum
    {
        public static readonly List<ProjectHourEnumItem> Items = new List<ProjectHourEnumItem>
    {
        new ("IT3150", "CN", 0.24),
        new ("IT3150", "KSCQ", 0.24),
        new ("IT3150", "KSTN", 0.36),
        new ("IT3150", "Viet-Phap", 0.36),
        new ("IT3910E", "CTTT", 0.4),
        new ("IT3910Q", "SIE", 0.6),
        new ("IT3920Q", "SIE", 0.6),
        new ("IT3930", "CN", 0.24),
        new ("IT3930", "CNKH", 0.24),
        new ("IT3930", "CNKT", 0.24),
        new ("IT3930", "KSCQ", 0.24),
        new ("IT3930", "CTTT", 0.36),
        new ("IT3930", "Viet-Phap", 0.36),
        new ("IT3940", "CN", 0.36),
        new ("IT3940", "CNKH", 0.36),
        new ("IT3940", "CNKT", 0.36),
        new ("IT3940", "KSCQ", 0.36),
        new ("IT3943", "KSCQ", 0.36),
        new ("IT3940", "KSTN", 0.54),
        new ("IT3940", "Viet-Phap", 0.54),
        new ("IT4434", "KSCLC", 0.54),
        new ("IT4434", "KSTN", 0.54),
        new ("IT3940E", "CTTT", 0.6),
        new ("IT4940Q", "SIE", 0.6),
        new ("IT5005", "CN", 0.36),
        new ("IT5005", "KSCQ", 0.36),
        new ("IT5005", "CNKT", 0.36),
        new ("IT5006", "CN", 0.36),
        new ("IT5006", "KSCQ", 0.36),
        new ("IT5021", "CTTT", 0.6),
        new ("IT5021E", "CTTT", 0.6),
        new ("IT5021", "HEDSPI", 0.36),
        new ("IT5022", "HEDSPI", 0.36),
        new ("IT5022E", "CTTT", 0.6),
        new ("IT5023E", "CTTT", 0.4),
        new ("IT5030", "CTTT", 0.6),
        new ("IT5030E", "CTTT", 0.6),
        new ("IT5030", "HEDSPI", 0.36),
        new ("IT4993", "KSTN", 1),
        new ("IT4995", "CN", 0.8),
        new ("IT4995", "CNKT", 0.8),
        new ("IT4995", "KSCQ", 0.8),
        new ("IT4995", "CNTH", 0.8),
        new ("IT4995", "CNKH", 0.8),
        new ("IT4997", "KSCQ", 0.8),
        new ("IT4995", "KSTN", 0.8),
        new ("IT4995E", "CTTT", 0.8),
        new ("IT4998", "CNKH", 0.9),
        new ("IT4998", "CNKT", 0.9),
        new ("IT4998", "CNTH", 0.9),
        new ("IT4998", "KSCQ", 0.9),
        new ("IT5120", "CTTT", 1),
        new ("IT5120E", "CTTT", 1),
        new ("IT5904", "KSCLC", 1),
        new ("IT5120", "HEDSPI", 1),
        new ("IT5140", "KSCQ", 1),
        new ("IT5150", "KSCQ", 1),
        new ("IT5150", "CNKT", 1),
        new ("IT5210", "KSCQ", 1),
        new ("IT5230", "KSCQ", 1),
        new ("IT5240Q", "KSCQ", 1),
        new ("IT5315Q", "SIE", 1),
    };
    }
}
