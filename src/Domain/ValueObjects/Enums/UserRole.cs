using System.ComponentModel;

namespace Domain.ValueObjects.Enums;

public enum UserRole
{
    [Description("SYSTEM_ADMIN")]
    SystemAdmin,
    [Description("COMPANY_OWNER")]
    CompanyOwner,
    [Description("COMPANY_ADMIN")]
    CompanyAdmin,
    [Description("COMPANY_EMPLOYEE")]
    Employee,
    [Description("CUSTOMER")]
    Customer
}
