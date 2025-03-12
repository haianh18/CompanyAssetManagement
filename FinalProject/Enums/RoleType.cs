using System.ComponentModel;

public enum RoleType
{
    [Description("Admin with full authorization")]
    ADMIN,

    [Description("Warehouse Manager with inventory management authorization")]
    WAREHOUSE_MANAGER,

    [Description("General User with restricted authorization")]
    GENERAL_USER
}

