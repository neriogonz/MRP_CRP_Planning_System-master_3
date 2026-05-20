namespace MetallurgyAnalytics.Models
{
    /// <summary>
    /// Типы продукции в системе планирования
    /// </summary>
    public enum ProductType
    {
        /// <summary>
        /// Готовая продукция (конечный продукт для продажи/отгрузки)
        /// </summary>
        FinishedGood = 0,

        /// <summary>
        /// Компонент/полуфабрикат (используется в сборке других продуктов)
        /// </summary>
        Component = 1,

        /// <summary>
        /// Сырьё (исходные материалы: металл, сплавы, добавки)
        /// </summary>
        RawMaterial = 2,

        /// <summary>
        /// Вспомогательный материал (инструмент, расходники)
        /// </summary>
        Consumable = 3,

        /// <summary>
        /// Отходы/лом (для учёта возвратных материалов)
        /// </summary>
        Waste = 4
    }
}