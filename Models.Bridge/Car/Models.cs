using Microsoft.VisualBasic.CompilerServices;
using Models.Bridge.Photo;

namespace Models.Bridge.Car;

public class Car 
{
    #region Стандартные данные
    
    public int Id { get; set; }

    /// <summary> Марка </summary>
    public string? Brand { get; set; }
    
    /// <summary> Цвет </summary>
    public string? Color { get; set; }
    
    /// <summary> Цена </summary>
    public decimal? Price { get; set; }

    #endregion
    
    /// <summary>
    /// Фото
    /// </summary>
    public CarPhoto? Photo { get; set; }
    
    public Priority Priority { get; set; }
    public Condition Condition { get; set; }

    private readonly IList<ICarDetail?> _details = new List<ICarDetail?>();

    public List<ICarDetail?> All() => _details.ToList();
    public void Clear() => _details.Clear();

    public TDetail? GetDetail<TDetail>() where TDetail : class, ICarDetail =>
        _details.OfType<TDetail>().SingleOrDefault() ?? null;

    public void AddDetail<TDetail>(TDetail detail) where TDetail : class, ICarDetail
    {
        _details
            .OfType<TDetail>()
            .ToList()
            .ForEach(d => _details.Remove(d));

        _details.Add(detail);
    }

    public bool RemoveDetail<TDetail>() where TDetail : class, ICarDetail
    {
        var existing = _details.OfType<TDetail>().ToList();
        foreach (var d in existing) _details.Remove(d);
        return existing.Any();
    }
}
