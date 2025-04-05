namespace Domain.Abstractions.Interfaces;

public interface IGeneralSearchBy
{
    static abstract List<string>? SearchByProperties { get; }
}