namespace MetallurgyAnalytics.Models
{
    /// <summary>
    /// Статусы производственного заказа
    /// </summary>
    public enum OrderState
    {
        /// <summary>
        /// Черновик: заказ создан, но не подтверждён
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Подтверждён: запущен в планирование
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// В работе: выполняются производственные операции
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// На паузе: ожидание материалов/оборудования
        /// </summary>
        OnHold = 3,

        /// <summary>
        /// Завершён: продукция произведена и оприходована
        /// </summary>
        Done = 4,

        /// <summary>
        /// Отменён: заказ закрыт без выполнения
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// Брак: заказ завершён с отклонением по качеству
        /// </summary>
        Rejected = 6
    }
}