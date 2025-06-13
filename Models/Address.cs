using System;
using System.Collections.Generic;

namespace E_ticaret_Sitesi.Models;

public partial class Address
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string City { get; set; } = null!;

    public string District { get; set; } = null!;

    public string FullAddress { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
