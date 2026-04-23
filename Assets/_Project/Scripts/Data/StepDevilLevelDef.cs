namespace StepDevil
{
    public sealed class StepDevilLevelDef
    {
        public int Id { get; }
        public int WorldIndex { get; }
        public string Name { get; }
        public StepDevilStoneDef[][] Forks { get; }

        public StepDevilLevelDef(int id, int worldIndex, string name, StepDevilStoneDef[][] forks)
        {
            Id = id;
            WorldIndex = worldIndex;
            Name = name;
            Forks = forks;
        }

        public int ForkCount => Forks.Length;
    }
}
