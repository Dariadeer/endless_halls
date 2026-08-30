namespace Shared.Data.Commands;

public interface ICommand
{
    int Id { get; }
    int Tick { get; }

}
