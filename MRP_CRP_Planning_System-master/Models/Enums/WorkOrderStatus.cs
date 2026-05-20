namespace MetallurgyAnalytics.Models
{
    /// <summary>
    /// Статусы рабочего задания (операции на рабочем центре)
    /// </summary>
    public enum WorkOrderStatus
    {
        /// <summary>
        /// Ожидает: задание создано, но не начато
        /// </summary>
        Pending = 0,

        /// <summary>
        /// В работе: оператор выполняет операцию
        /// </summary>
        Started = 1,

        /// <summary>
        /// Приостановлено: пауза по техническим причинам
        /// </summary>
        Paused = 2,

        /// <summary>
        /// Завершено: операция выполнена успешно
        /// </summary>
        Done = 3,

        /// <summary>
        /// Блок: невозможно выполнить (нет материала/инструмента)
        /// </summary>
        Blocked = 4,

        /// <summary>
        /// Отменено: задание снято с выполнения
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// На проверке: ожидает контроля качества
        /// </summary>
        QualityCheck = 6
    }
}