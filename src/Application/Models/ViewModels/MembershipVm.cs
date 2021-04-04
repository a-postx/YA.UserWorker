using System;
using YA.UserWorker.Application.Enums;

namespace YA.UserWorker.Application.Models.ViewModels
{
    /// <summary>
    /// Членство пользователя, визуальная модель.
    /// </summary>
    public class MembershipVm
    {
        /// <summary>
        /// Идентификатор членства.
        /// </summary>
        public Guid MembershipID { get; set; }
        /// <summary>
        /// Пользователь.
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Арендатор.
        /// </summary>
        public Guid TenantId { get; set; }
        /// <summary>
        /// Тип доступа пользователя к арендатору.
        /// </summary>
        public MembershipAccessType AccessType { get; set; }
    }
}
