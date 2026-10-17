namespace SharedKernel.Domain;

public abstract class Entity
{
    public Guid Id { get; protected init; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity()
    {
    }
}
