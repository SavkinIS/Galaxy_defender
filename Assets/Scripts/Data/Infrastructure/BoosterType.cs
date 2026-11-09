namespace Galaxy.Data.Infrastructure
{
    public enum BoosterType
    {
        EscapeSelected = 0, // выпускает любой корабль
        DeleteColor = 1,    // удаляет блоки и корабли выбранного цвета
        ExtraSlot = 2,      // увеличивает количество кораблей в бою
        Volley = 3          // залп кораблей из зон ожидания
    }
}
